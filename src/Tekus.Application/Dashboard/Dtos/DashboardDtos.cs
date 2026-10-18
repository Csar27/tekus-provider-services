namespace Tekus.Application.Dashboard.Dtos;

/// <summary>Headline figures of the whole catalogue.</summary>
public sealed record DashboardTotalsDto(
    int Providers,
    int ActiveProviders,
    int Services,
    int ActiveServices,
    int Countries,
    int Offerings);

/// <summary>
/// Indicator #1: how many services (and how many providers offering them) are available per
/// country. Answers "where can we sell what".
/// </summary>
public sealed record ServicesPerCountryDto(
    string CountryCode,
    string CountryName,
    string Region,
    int ServiceCount,
    int ProviderCount);

/// <summary>
/// Indicator #2: average hourly rate per country (USD). Answers "how expensive is each market".
/// </summary>
public sealed record AverageRatePerCountryDto(
    string CountryCode,
    string CountryName,
    decimal AverageHourlyRateUsd,
    decimal MinHourlyRateUsd,
    decimal MaxHourlyRateUsd,
    int ServiceCount);

/// <summary>Providers with the largest portfolio, useful to spot the most complete partners.</summary>
public sealed record TopProviderDto(
    Guid ProviderId,
    string Name,
    string Nit,
    int ServiceCount,
    decimal TotalHourlyRateUsd);

/// <summary>Payload of <c>GET /api/v1/dashboard/summary</c>.</summary>
public sealed record DashboardSummaryDto(
    DashboardTotalsDto Totals,
    IReadOnlyList<ServicesPerCountryDto> ServicesPerCountry,
    IReadOnlyList<AverageRatePerCountryDto> AverageHourlyRatePerCountry,
    IReadOnlyList<TopProviderDto> TopProvidersByServices,
    DateTime GeneratedAtUtc);
