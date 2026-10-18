namespace Tekus.Application.Providers.Dtos;

/// <summary>User defined field as exposed to clients.</summary>
public sealed record ProviderCustomFieldDto(string Key, string Value);

/// <summary>Compact projection of a service offered by a provider.</summary>
public sealed record ProviderOfferedServiceDto(
    Guid Id,
    string Name,
    decimal HourlyRateUsd,
    IReadOnlyList<string> CountryCodes);

/// <summary>Row of the paginated provider list (<c>GET /api/v1/providers</c>).</summary>
public sealed record ProviderListItemDto(
    Guid Id,
    string Nit,
    string Name,
    string Email,
    bool IsActive,
    int ServiceCount,
    int CustomFieldCount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>Full provider representation (<c>GET /api/v1/providers/{id}</c>).</summary>
public sealed record ProviderDetailDto(
    Guid Id,
    string Nit,
    string Name,
    string Email,
    bool IsActive,
    IReadOnlyList<ProviderCustomFieldDto> CustomFields,
    IReadOnlyList<ProviderOfferedServiceDto> Services,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    long Version);
