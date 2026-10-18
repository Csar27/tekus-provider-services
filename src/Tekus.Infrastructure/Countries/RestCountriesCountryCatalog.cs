using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tekus.Application.Abstractions.Countries;
using Tekus.Application.Common;
using Tekus.Domain.Countries;
using Tekus.Domain.Common;
using Tekus.Infrastructure.Persistence;

namespace Tekus.Infrastructure.Countries;

/// <summary>
/// Adapter over an external REST countries catalogue (REST Countries v3.1 by default).
/// <para>
/// The external API offers no filtering, paging or ordering, and it can be slow or unavailable, so
/// the data is reconciled into a local <c>Countries</c> snapshot that the rest of the system reads
/// from. The first read populates the snapshot (cache-fill); later reads are served locally until
/// the cache window elapses. When the external provider is down the snapshot keeps answering,
/// which is why a service can still be created during an outage.
/// </para>
/// </summary>
internal sealed class RestCountriesCountryCatalog : ICountryCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly TekusDbContext _db;
    private readonly CountryCatalogOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RestCountriesCountryCatalog> _logger;

    public RestCountriesCountryCatalog(
        HttpClient httpClient,
        TekusDbContext db,
        IOptions<CountryCatalogOptions> options,
        TimeProvider timeProvider,
        ILogger<RestCountriesCountryCatalog> logger)
    {
        _httpClient = httpClient;
        _db = db;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<CountrySyncResult> SyncAsync(bool force, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.UtcNow();
        var lastSyncUtc = await _db.Countries.MaxAsync(country => (DateTime?)country.LastSyncedAtUtc, cancellationToken);

        var isFresh = lastSyncUtc.HasValue && now - lastSyncUtc.Value < TimeSpan.FromMinutes(_options.CacheDurationMinutes);

        if (!force && isFresh)
        {
            return new CountrySyncResult(await _db.Countries.CountAsync(cancellationToken), 0, 0, lastSyncUtc!.Value, FromCache: true);
        }

        if (!_options.UseExternalService)
        {
            // Offline / demo mode: serve whatever the snapshot already contains.
            return new CountrySyncResult(await _db.Countries.CountAsync(cancellationToken), 0, 0, lastSyncUtc ?? now, FromCache: true);
        }

        IReadOnlyList<ExternalCountry> external;

        try
        {
            external = await FetchExternalAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(
                exception,
                "The external country catalogue could not be reached; serving the local snapshot instead");

            var cachedCount = await _db.Countries.CountAsync(cancellationToken);

            if (cachedCount == 0)
            {
                // Nothing to fall back to: the client gets a clear, retryable error.
                throw new DomainException(
                    "The external country catalogue is unavailable and there is no local snapshot yet. Please try again later.",
                    ErrorCodes.ExternalServiceUnavailable);
            }

            return new CountrySyncResult(cachedCount, 0, 0, lastSyncUtc ?? now, FromCache: true);
        }

        return await ReconcileAsync(external, now, cancellationToken);
    }

    public async Task<IReadOnlySet<string>> GetAvailableCodesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SyncAsync(force: false, cancellationToken);
        }
        catch (DomainException)
        {
            // The snapshot is empty and the external service is down. Callers (country validation)
            // fall back to accepting the codes and let the database foreign key be the guard.
            _logger.LogDebug("Country snapshot unavailable; skipping catalogue validation for this request");
        }

        var codes = await _db.Countries
            .Select(country => country.Code)
            .ToListAsync(cancellationToken);

        // CountryCode is a value object: EF can select it (via the converter) but its members are
        // only readable after materialisation.
        return codes
            .Select(code => code.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<IReadOnlyList<ExternalCountry>> FetchExternalAsync(CancellationToken cancellationToken)
    {
        var url = $"/all?fields={Uri.EscapeDataString(_options.Fields)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<List<ExternalCountry>>(JsonOptions, cancellationToken);

        return payload ?? [];
    }

    private async Task<CountrySyncResult> ReconcileAsync(
        IReadOnlyList<ExternalCountry> external,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // Load the whole snapshot once: a sync touches almost every row, so per-code queries would
        // turn a single HTTP call into hundreds of round trips.
        var existing = await _db.Countries.ToListAsync(cancellationToken);
        var byCode = existing.ToDictionary(country => country.Code.Value, StringComparer.OrdinalIgnoreCase);

        var added = 0;
        var updated = 0;

        foreach (var candidate in external)
        {
            if (string.IsNullOrWhiteSpace(candidate.CommonName))
            {
                continue;
            }

            CountryCode code;

            try
            {
                code = CountryCode.Create(candidate.Cca2);
            }
            catch (DomainException)
            {
                // The external payload occasionally contains entries without a usable alpha-2 code.
                continue;
            }

            if (byCode.TryGetValue(code.Value, out var country))
            {
                country.Sync(
                    candidate.CommonName,
                    candidate.Region,
                    FirstCurrencyCode(candidate.Currencies),
                    candidate.Cca3,
                    now);

                updated++;
            }
            else
            {
                var created = Country.Create(
                    code.Value,
                    candidate.CommonName,
                    candidate.Region,
                    FirstCurrencyCode(candidate.Currencies),
                    candidate.Cca3,
                    now);

                _db.Countries.Add(created);
                byCode[created.Code.Value] = created;
                added++;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Country catalogue synchronised: {Total} total, {Added} added, {Updated} updated",
                byCode.Count,
                added,
                updated);
        }

        return new CountrySyncResult(byCode.Count, added, updated, now, FromCache: false);
    }

    private static string? FirstCurrencyCode(Dictionary<string, ExternalCurrency>? currencies)
        => currencies is { Count: > 0 } ? currencies.Keys.FirstOrDefault() : null;
}

/// <summary>Subset of the external payload that the snapshot actually needs.</summary>
internal sealed class ExternalCountry
{
    public string? Cca2 { get; set; }

    public string? Cca3 { get; set; }

    public string? Region { get; set; }

    public ExternalName? Name { get; set; }

    public Dictionary<string, ExternalCurrency>? Currencies { get; set; }

    [JsonIgnore]
    public string? CommonName => Name?.Common;
}

internal sealed class ExternalName
{
    public string? Common { get; set; }
}

internal sealed class ExternalCurrency
{
    public string? Name { get; set; }
}
