using Tekus.Domain.Common;

namespace Tekus.Domain.Users;

/// <summary>
/// Refresh token (one row per issued token). Only the SHA-256 hash of the token is persisted, so a
/// database leak does not hand out usable credentials.
/// <para>
/// Rotation is implemented by revoking the consumed token and linking it to its replacement, which
/// makes replay of an already used token detectable (<c>ReplacedByTokenHash</c> chain).
/// </para>
/// </summary>
public sealed class RefreshToken : Entity
{
    /// <summary>Required by EF Core.</summary>
    private RefreshToken()
    {
    }

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTime expiresAtUtc, string? createdByIp, DateTime utcNow)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedByIp = createdByIp;
        CreatedAtUtc = utcNow;
    }

    public Guid UserId { get; private set; }

    /// <summary>Base64 SHA-256 of the opaque token handed to the client.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public string? CreatedByIp { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public string? RevokedReason { get; private set; }

    /// <summary>Hash of the token that replaced this one, if it was rotated.</summary>
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive(DateTime utcNow) => RevokedAtUtc is null && ExpiresAtUtc > utcNow;

    internal static RefreshToken Issue(
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc,
        string? createdByIp,
        DateTime utcNow)
        => new(Guid.NewGuid(), userId, tokenHash, expiresAtUtc, createdByIp, utcNow);

    internal void Revoke(DateTime utcNow, string reason, string? replacedByTokenHash = null)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RevokedAtUtc = utcNow;
        RevokedReason = reason;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
