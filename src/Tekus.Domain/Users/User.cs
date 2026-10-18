using Tekus.Domain.Common;

namespace Tekus.Domain.Users;

/// <summary>
/// User aggregate root. It owns the credentials, the lockout counters and the refresh tokens
/// already issued (so revoking "all sessions" is a single aggregate operation).
/// </summary>
public sealed partial class User : AggregateRoot
{
    public const int EmailMaxLength = 200;
    public const int FullNameMaxLength = 150;
    public const int MaxFailedLoginAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan DefaultRefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly List<RefreshToken> _refreshTokens = [];

    /// <summary>Required by EF Core.</summary>
    private User()
    {
    }

    private User(Guid id, string email, string fullName, string passwordHash, UserRole role, DateTime utcNow)
        : base(id)
    {
        Email = email;
        FullName = fullName;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAtUtc = utcNow;
        Version = 1;
    }

    public string Email { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    /// <summary>Opaque PBKDF2 hash produced by <c>IPasswordHasher</c> (never a clear-text password).</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? LastLoginAtUtc { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTime? LockedUntilUtc { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public static User Create(string? email, string? fullName, string passwordHash, UserRole role, DateTime utcNow)
    {
        var normalizedEmail = EmailAddress.Create(email).Value;

        return new User(
            Guid.NewGuid(),
            normalizedEmail,
            Guard.NotNullOrWhiteSpace(fullName, "fullName", FullNameMaxLength, minLength: 3),
            Guard.NotNullOrWhiteSpace(passwordHash, "passwordHash", maxLength: 512),
            role,
            utcNow);
    }

    public bool IsLockedOut(DateTime utcNow) => LockedUntilUtc is not null && LockedUntilUtc > utcNow;

    public void RegisterSuccessfulLogin(DateTime utcNow)
    {
        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
        LastLoginAtUtc = utcNow;
        Version++;
    }

    public void RegisterFailedLogin(DateTime utcNow)
    {
        FailedLoginAttempts++;
        Version++;

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            LockedUntilUtc = utcNow.Add(LockoutDuration);
            FailedLoginAttempts = 0;
        }
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        Version++;
        RevokeAllRefreshTokens(utcNow, "user_deactivated");
    }

    /// <summary>Issues (and stores) a new refresh token for this user.</summary>
    public RefreshToken IssueRefreshToken(
        string tokenHash,
        string? createdByIp,
        DateTime utcNow,
        TimeSpan? lifetime = null)
    {
        var token = RefreshToken.Issue(
            Id,
            Guard.NotNullOrWhiteSpace(tokenHash, "tokenHash", maxLength: 128),
            utcNow.Add(lifetime ?? DefaultRefreshTokenLifetime),
            createdByIp,
            utcNow);

        _refreshTokens.Add(token);
        Version++;

        return token;
    }

    /// <summary>
    /// Revokes every active token of the user. Used on password change, deactivation and when a
    /// replayed token is detected.
    /// </summary>
    public void RevokeAllRefreshTokens(DateTime utcNow, string reason)
    {
        foreach (var token in _refreshTokens.Where(token => token.IsActive(utcNow)))
        {
            token.Revoke(utcNow, reason);
        }

        Version++;
    }
}
