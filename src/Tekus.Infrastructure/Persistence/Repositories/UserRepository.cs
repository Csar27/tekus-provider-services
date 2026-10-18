using Microsoft.EntityFrameworkCore;
using Tekus.Domain.Users;

namespace Tekus.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly TekusDbContext _db;

    public UserRepository(TekusDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // E-mails are stored lower-cased by the domain; normalising here makes the lookup
        // case-insensitive on every provider, not only where the collation happens to be CI.
        var normalized = email.Trim().ToLowerInvariant();

        return _db.Users
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user => user.Email == normalized, cancellationToken);
    }

    public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _db.Users
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(
                user => user.RefreshTokens.Any(token => token.TokenHash == tokenHash),
                cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Add(user);
        return Task.CompletedTask;
    }
}
