using Tekus.Domain.Users;

namespace Tekus.Application.Abstractions.Security;

/// <summary>
/// Password hashing port. The implementation uses PBKDF2-HMAC-SHA256 with a random salt and a
/// per-hash iteration count, so hashes can be rotated without invalidating existing passwords.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}

/// <summary>Issues and hashes the tokens involved in the authentication flow.</summary>
public interface ITokenService
{
    /// <summary>Creates the signed, short lived JWT access token of the user.</summary>
    AccessToken CreateAccessToken(User user, DateTime utcNow);

    /// <summary>Creates a cryptographically strong, opaque refresh token (returned to the client).</summary>
    string GenerateRefreshToken();

    /// <summary>Hashes a refresh token before persisting it (only the hash is stored).</summary>
    string HashRefreshToken(string refreshToken);
}

/// <summary>A signed access token and the instant at which it expires.</summary>
public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);

/// <summary>Ambient information about the caller, provided by the API layer.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Email { get; }

    UserRole? Role { get; }

    bool IsAuthenticated { get; }

    /// <summary>Remote address of the request, stored with the refresh token for auditing.</summary>
    string? IpAddress { get; }

    bool IsInRole(UserRole role);
}
