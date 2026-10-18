namespace Tekus.Api.Contracts;

// ---------------------------------------------------------------------------
// Authentication
// ---------------------------------------------------------------------------

/// <summary>Body of <c>POST /api/v1/auth/login</c>.</summary>
public sealed record LoginRequest(string? Email, string? Password);

/// <summary>Body of <c>POST /api/v1/auth/refresh</c>.</summary>
public sealed record RefreshTokenRequest(string? RefreshToken);

/// <summary>Body of <c>POST /api/v1/auth/logout</c>.</summary>
public sealed record RevokeTokenRequest(string? RefreshToken);

// ---------------------------------------------------------------------------
// Providers
// ---------------------------------------------------------------------------

/// <summary>Custom field as sent by the client (the key is unique per provider).</summary>
public sealed record CustomFieldInput(string? Key, string? Value);

/// <summary>Body of <c>POST /api/v1/providers</c>.</summary>
public sealed record CreateProviderRequest(
    string? Nit,
    string? Name,
    string? Email,
    IReadOnlyCollection<CustomFieldInput>? CustomFields = null,
    IReadOnlyCollection<Guid>? ServiceIds = null);

/// <summary>Body of <c>PUT /api/v1/providers/{id}</c>. The concurrency token travels in <c>If-Match</c>.</summary>
public sealed record UpdateProviderRequest(string? Nit, string? Name, string? Email);

/// <summary>Body of <c>PUT /api/v1/providers/{id}/custom-fields</c>.</summary>
public sealed record ReplaceCustomFieldsRequest(IReadOnlyCollection<CustomFieldInput>? Fields);

/// <summary>Body of <c>PUT /api/v1/providers/{id}/services</c>.</summary>
public sealed record ReplaceProviderServicesRequest(IReadOnlyCollection<Guid>? ServiceIds);

/// <summary>Body of <c>PUT /api/v1/providers/{id}/custom-fields/{key}</c>.</summary>
public sealed record UpsertCustomFieldRequest(string? Key, string? Value);

// ---------------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------------

/// <summary>Body of <c>POST /api/v1/services</c>.</summary>
public sealed record CreateServiceRequest(
    string? Name,
    decimal HourlyRateUsd,
    string? Description = null,
    IReadOnlyCollection<string>? CountryCodes = null);

/// <summary>Body of <c>PUT /api/v1/services/{id}</c>.</summary>
public sealed record UpdateServiceRequest(string? Name, decimal HourlyRateUsd, string? Description = null);

/// <summary>Body of <c>PUT /api/v1/services/{id}/countries</c>.</summary>
public sealed record ReplaceCountriesRequest(IReadOnlyCollection<string>? CountryCodes);

// ---------------------------------------------------------------------------
// Status change (shared by providers and services)
// ---------------------------------------------------------------------------

/// <summary>Body of <c>PATCH /{resource}/{id}/status</c>.</summary>
public sealed record ChangeStatusRequest(bool IsActive);
