using Microsoft.EntityFrameworkCore;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Dashboard.Dtos;

namespace Tekus.Infrastructure.Persistence.Repositories;

/// <summary>
/// Builds the dashboard indicators.
/// <para>
/// The integer figures (totals, services per country) are aggregated by the database. The
/// monetary figures (average rate and portfolio value per country/provider) are aggregated in
/// memory: SQLite - used by the automated test suite - cannot aggregate <c>decimal</c> in SQL, and
/// doing it here keeps one implementation that behaves identically on SQL Server and SQLite. The
/// projections are deliberately narrow (ids, rates and codes) and the catalogue is small.
/// </para>
/// </summary>
internal sealed class DashboardReadRepository : IDashboardReadRepository
{
    private readonly TekusDbContext _db;
    private readonly TimeProvider _timeProvider;

    public DashboardReadRepository(TekusDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        int topProviders,
        CancellationToken cancellationToken = default)
    {
        var totals = new DashboardTotalsDto(
            await _db.Providers.CountAsync(cancellationToken),
            await _db.Providers.CountAsync(provider => provider.IsActive, cancellationToken),
            await _db.Services.CountAsync(cancellationToken),
            await _db.Services.CountAsync(service => service.IsActive, cancellationToken),
            await _db.Countries.CountAsync(cancellationToken),
            await _db.ProviderServices.CountAsync(cancellationToken));

        var countries = await _db.Countries
            .AsNoTracking()
            .Select(country => new { country.Code, country.Name, country.Region })
            .ToListAsync(cancellationToken);

        var availability = await _db.ServiceCountries
            .AsNoTracking()
            .Select(availability => new { availability.ServiceId, availability.CountryCode })
            .ToListAsync(cancellationToken);

        var offerings = await _db.ProviderServices
            .AsNoTracking()
            .Select(offering => new { offering.ServiceId, offering.ProviderId })
            .ToListAsync(cancellationToken);

        var serviceRates = await _db.Services
            .AsNoTracking()
            .Select(service => new { service.Id, service.HourlyRateUsd })
            .ToListAsync(cancellationToken);

        var providers = await _db.Providers
            .AsNoTracking()
            .Select(provider => new { provider.Id, provider.Name, provider.Nit })
            .ToListAsync(cancellationToken);

        var rateByService = serviceRates.ToDictionary(service => service.Id, service => service.HourlyRateUsd);
        var providerById = providers.ToDictionary(provider => provider.Id);

        var countryMeta = countries.ToDictionary(
            country => country.Code.Value,
            country => country,
            StringComparer.OrdinalIgnoreCase);

        var serviceIdsPerCountry = availability
            .GroupBy(entry => entry.CountryCode.Value, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(entry => entry.ServiceId).Distinct().ToArray(),
                StringComparer.OrdinalIgnoreCase);

        var servicesPerCountry = serviceIdsPerCountry
            .Select(pair =>
            {
                var serviceIds = pair.Value.ToHashSet();

                var providerCount = offerings
                    .Where(offering => serviceIds.Contains(offering.ServiceId))
                    .Select(offering => offering.ProviderId)
                    .Distinct()
                    .Count();

                countryMeta.TryGetValue(pair.Key, out var meta);

                return new ServicesPerCountryDto(
                    pair.Key,
                    meta?.Name ?? pair.Key,
                    meta?.Region ?? "Unknown",
                    serviceIds.Count,
                    providerCount);
            })
            .OrderByDescending(dto => dto.ServiceCount)
            .ThenBy(dto => dto.CountryName, StringComparer.Ordinal)
            .ToArray();

        var averageRatePerCountry = serviceIdsPerCountry
            .Select(pair =>
            {
                var rates = pair.Value
                    .Where(rateByService.ContainsKey)
                    .Select(id => rateByService[id])
                    .ToArray();

                countryMeta.TryGetValue(pair.Key, out var meta);

                return new AverageRatePerCountryDto(
                    pair.Key,
                    meta?.Name ?? pair.Key,
                    rates.Length == 0 ? 0m : decimal.Round(rates.Average(), 2, MidpointRounding.AwayFromZero),
                    rates.Length == 0 ? 0m : rates.Min(),
                    rates.Length == 0 ? 0m : rates.Max(),
                    rates.Length);
            })
            .OrderByDescending(dto => dto.AverageHourlyRateUsd)
            .ThenBy(dto => dto.CountryName, StringComparer.Ordinal)
            .ToArray();

        var topProvidersByServices = offerings
            .GroupBy(offering => offering.ProviderId)
            .Select(group =>
            {
                var serviceIds = group.Select(offering => offering.ServiceId).Distinct().ToArray();
                var totalRate = serviceIds.Where(rateByService.ContainsKey).Sum(id => rateByService[id]);

                providerById.TryGetValue(group.Key, out var provider);

                return new TopProviderDto(
                    group.Key,
                    provider?.Name ?? string.Empty,
                    provider?.Nit ?? string.Empty,
                    serviceIds.Length,
                    totalRate);
            })
            .OrderByDescending(dto => dto.ServiceCount)
            .ThenByDescending(dto => dto.TotalHourlyRateUsd)
            .Take(topProviders)
            .ToArray();

        return new DashboardSummaryDto(
            totals,
            servicesPerCountry,
            averageRatePerCountry,
            topProvidersByServices,
            _timeProvider.GetUtcNow().UtcDateTime);
    }
}
