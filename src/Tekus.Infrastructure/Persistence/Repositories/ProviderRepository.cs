using Microsoft.EntityFrameworkCore;
using Tekus.Domain.Providers;

namespace Tekus.Infrastructure.Persistence.Repositories;

internal sealed class ProviderRepository : IProviderRepository
{
    private readonly TekusDbContext _db;

    public ProviderRepository(TekusDbContext db) => _db = db;

    public Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Providers
            .Include(provider => provider.CustomFields)
            .Include(provider => provider.OfferedServices)
            .FirstOrDefaultAsync(provider => provider.Id == id, cancellationToken);

    public Task<bool> NitExistsAsync(
        string nit,
        Guid? excludingProviderId = null,
        CancellationToken cancellationToken = default)
        => _db.Providers.AnyAsync(
            provider => provider.Nit == nit
                        && (excludingProviderId == null || provider.Id != excludingProviderId),
            cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Providers.AnyAsync(provider => provider.Id == id, cancellationToken);

    public Task AddAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        _db.Providers.Add(provider);
        return Task.CompletedTask;
    }

    public void Remove(Provider provider) => _db.Providers.Remove(provider);
}
