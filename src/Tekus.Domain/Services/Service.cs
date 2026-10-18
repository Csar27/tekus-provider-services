using System.Text.RegularExpressions;
using Tekus.Domain.Common;
using Tekus.Domain.Countries;
using Tekus.Domain.Services.Events;

namespace Tekus.Domain.Services;

/// <summary>
/// Service catalogue aggregate root.
/// <para>
/// A service is a reusable offer ("Descarga espacial de contenidos", "Desaparición forzada de
/// bytes") with an hourly rate in USD and the list of countries where it is available. Providers
/// reference it by id, therefore the same service can be offered by many providers and the rate
/// is defined exactly once.
/// </para>
/// </summary>
public sealed partial class Service : AggregateRoot
{
    public const int NameMaxLength = 150;
    public const int DescriptionMaxLength = 1000;
    public const int MaxCountries = 250;
    public const decimal MaxHourlyRateUsd = 1_000_000m;
    public const int RateDecimals = 2;

    public const string NamePattern = @"^[\p{L}\p{N}][\p{L}\p{N} _.,\-()/&+']*$";

    private readonly List<ServiceCountry> _countries = [];

    /// <summary>Required by EF Core.</summary>
    private Service()
    {
    }

    private Service(Guid id, string name, decimal hourlyRateUsd, string? description, DateTime utcNow)
        : base(id)
    {
        Name = name;
        HourlyRateUsd = hourlyRateUsd;
        Description = description;
        IsActive = true;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
        Version = 1;

        RaiseDomainEvent(new ServiceCreatedDomainEvent(Id, Name, utcNow));
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>Hourly rate in US dollars, rounded to two decimals.</summary>
    public decimal HourlyRateUsd { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>Optimistic concurrency token (see <see cref="Providers.Provider.Version"/>).</summary>
    public long Version { get; private set; }

    /// <summary>Countries where the service can be delivered.</summary>
    public IReadOnlyCollection<ServiceCountry> Countries => _countries.AsReadOnly();

    public static Service Create(string? name, decimal hourlyRateUsd, string? description, DateTime utcNow)
        => new(
            Guid.NewGuid(),
            NormalizeName(name),
            NormalizeHourlyRate(hourlyRateUsd),
            NormalizeDescription(description),
            utcNow);

    public void Update(string? name, decimal hourlyRateUsd, string? description, DateTime utcNow)
    {
        Name = NormalizeName(name);
        HourlyRateUsd = NormalizeHourlyRate(hourlyRateUsd);
        Description = NormalizeDescription(description);

        Touch(utcNow);
        RaiseDomainEvent(new ServiceUpdatedDomainEvent(Id, Name, utcNow));
    }

    /// <summary>Adds a country to the availability list. Idempotent.</summary>
    public void OfferIn(string? countryCode, DateTime utcNow)
    {
        var code = CountryCode.Create(countryCode);

        if (_countries.Any(country => country.CountryCode == code))
        {
            return;
        }

        if (_countries.Count >= MaxCountries)
        {
            throw new DomainException(
                $"A service cannot be offered in more than {MaxCountries} countries.",
                ErrorCodes.Validation);
        }

        _countries.Add(ServiceCountry.Offer(Id, code, utcNow));
        Touch(utcNow);
        RaiseDomainEvent(new ServiceCountriesChangedDomainEvent(Id, code.Value, utcNow));
    }

    /// <summary>Removes a country from the availability list. Idempotent.</summary>
    public void StopOfferingIn(string? countryCode, DateTime utcNow)
    {
        var code = CountryCode.Create(countryCode);

        var existing = _countries.FirstOrDefault(country => country.CountryCode == code);

        if (existing is null)
        {
            return;
        }

        _countries.Remove(existing);
        Touch(utcNow);
        RaiseDomainEvent(new ServiceCountriesChangedDomainEvent(Id, code.Value, utcNow));
    }

    /// <summary>Replaces the whole availability list in one operation (used by PUT).</summary>
    public void ReplaceCountries(IEnumerable<string> countryCodes, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(countryCodes);

        var target = countryCodes
            .Select(CountryCode.Create)
            .Distinct()
            .ToArray();

        foreach (var code in target)
        {
            OfferIn(code.Value, utcNow);
        }

        foreach (var code in _countries.Select(country => country.CountryCode).Except(target).Select(country => country.Value).ToArray())
        {
            StopOfferingIn(code, utcNow);
        }
    }

    public void Activate(DateTime utcNow)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(utcNow);
    }

    /// <summary>Logical delete: the service stays available for historical reporting.</summary>
    public void Deactivate(DateTime utcNow)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(utcNow);
        RaiseDomainEvent(new ServiceUpdatedDomainEvent(Id, Name, utcNow));
    }

    private void Touch(DateTime utcNow)
    {
        UpdatedAtUtc = utcNow;
        Version++;
    }

    /// <summary>Normalises and validates the service name.</summary>
    public static string NormalizeName(string? name)
    {
        var normalized = Guard.NotNullOrWhiteSpace(name, "name", NameMaxLength, minLength: 3).SquashInnerSpaces();

        return NameRegex().IsMatch(normalized)
            ? normalized
            : throw new DomainException(
                $"'{name}' is not a valid service name. Use letters, digits, spaces and _.,-()/&+ only.",
                ErrorCodes.Validation);
    }

    /// <summary>Validates the hourly rate and rounds it to two decimals (USD).</summary>
    public static decimal NormalizeHourlyRate(decimal hourlyRateUsd)
        => decimal.Round(
            Guard.Positive(hourlyRateUsd, "hourlyRateUsd", MaxHourlyRateUsd),
            RateDecimals,
            MidpointRounding.AwayFromZero);

    /// <summary>Normalises the optional description (empty strings become <c>null</c>).</summary>
    public static string? NormalizeDescription(string? description)
        => string.IsNullOrWhiteSpace(description)
            ? null
            : Guard.NotNullOrWhiteSpace(description, "description", DescriptionMaxLength, minLength: 3).SquashInnerSpaces();

    [GeneratedRegex(NamePattern, RegexOptions.CultureInvariant)]
    private static partial Regex NameRegex();
}
