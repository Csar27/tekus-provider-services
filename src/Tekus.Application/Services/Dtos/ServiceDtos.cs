namespace Tekus.Application.Services.Dtos;

/// <summary>Country where a service is offered, with the display name resolved from the snapshot.</summary>
public sealed record ServiceCountryDto(string Code, string Name);

/// <summary>Compact reference to a provider, used by the service detail projection.</summary>
public sealed record ProviderReferenceDto(Guid Id, string Name, string Nit);

/// <summary>Row of the paginated service list (<c>GET /api/v1/services</c>).</summary>
public sealed record ServiceListItemDto(
    Guid Id,
    string Name,
    decimal HourlyRateUsd,
    bool IsActive,
    int CountryCount,
    int ProviderCount,
    IReadOnlyList<string> CountryCodes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>Full service representation (<c>GET /api/v1/services/{id}</c>).</summary>
public sealed record ServiceDetailDto(
    Guid Id,
    string Name,
    decimal HourlyRateUsd,
    string? Description,
    bool IsActive,
    IReadOnlyList<ServiceCountryDto> Countries,
    IReadOnlyList<ProviderReferenceDto> Providers,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    long Version);
