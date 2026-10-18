using Tekus.Domain.Common;

namespace Tekus.Domain.Users;

public sealed partial class User
{
    /// <summary>True when the given (already hashed) refresh token belongs to the user and is still usable.</summary>
    public bool HasActiveRefreshToken(string tokenHash, DateTime utcNow)
        => _refreshTokens.Any(token => token.TokenHash == tokenHash && token.IsActive(utcNow));

    /// <summary>
    /// Rotates a refresh token: issues the replacement first and then revokes the consumed token,
    /// linking both so a replayed token can be detected later.
    /// </summary>
    public RefreshToken RotateRefreshToken(
        string currentTokenHash,
        string newTokenHash,
        string? createdByIp,
        DateTime utcNow,
        TimeSpan? lifetime = null)
    {
        var current = _refreshTokens.FirstOrDefault(token => token.TokenHash == currentTokenHash)
                      ?? throw new DomainException(
                          "The refresh token does not belong to the user.",
                          ErrorCodes.InvalidCredentials);

        if (!current.IsActive(utcNow))
        {
            throw new DomainException(
                "The refresh token is expired or has already been revoked.",
                ErrorCodes.InvalidCredentials);
        }

        var replacement = IssueRefreshToken(newTokenHash, createdByIp, utcNow, lifetime);

        current.Revoke(utcNow, "rotated", newTokenHash);

        return replacement;
    }

    /// <summary>
    /// Revokes a single refresh token (logout of the current session). It is idempotent: revoking
    /// an unknown or already revoked token is a no-op that returns <see langword="false"/>, so the
    /// endpoint can be called twice without failing.
    /// </summary>
    public bool RevokeRefreshToken(string tokenHash, DateTime utcNow, string reason)
    {
        var token = _refreshTokens.FirstOrDefault(candidate => candidate.TokenHash == tokenHash);

        if (token is null || !token.IsActive(utcNow))
        {
            return false;
        }

        token.Revoke(utcNow, reason);
        Version++;

        return true;
    }
}
