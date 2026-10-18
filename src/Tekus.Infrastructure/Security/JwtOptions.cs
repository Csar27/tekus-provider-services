namespace Tekus.Infrastructure.Security;

/// <summary>
/// Settings of the access tokens issued by the API. The same values are consumed by the
/// authentication middleware (see <c>DependencyInjection.AddAuthentication</c>), so the token
/// validation and the token creation can never drift apart.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "tekus.provider-services";

    public string Audience { get; set; } = "tekus.provider-services.web";

    /// <summary>Symmetric signing key. In production it comes from user-secrets / Key Vault.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Lifetime of the (short lived) access token, in minutes.</summary>
    public int AccessTokenMinutes { get; set; } = 30;

    /// <summary>Lifetime of the refresh token, in days. Mirrors the domain default.</summary>
    public int RefreshTokenDays { get; set; } = 7;
}
