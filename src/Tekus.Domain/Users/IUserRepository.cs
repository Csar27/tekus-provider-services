namespace Tekus.Domain.Users;

/// <summary>Write side repository of the <see cref="User"/> aggregate.</summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Loads the user (with its refresh tokens) by e-mail, case-insensitive.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Loads the user that owns the given refresh token hash (used by the refresh flow).</summary>
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
