using Microsoft.EntityFrameworkCore;
using Tekus.Domain.Services;

namespace Tekus.Infrastructure.Persistence.Repositories;

internal sealed class ServiceRepository : IServiceRepository
{
    private readonly TekusDbContext _db;

    public ServiceRepository(TekusDbContext db) => _db = db;

    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Services
            .Include(service => service.Countries)
            .FirstOrDefaultAsync(service => service.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Service>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var target = ids.Distinct().ToArray();

        return await _db.Services
            .Include(service => service.Countries)
            .Where(service => target.Contains(service.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlySet<Guid>> GetExistingIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var target = ids.Distinct().ToArray();

        var existing = await _db.Services
            .Where(service => target.Contains(service.Id))
            .Select(service => service.Id)
            .ToListAsync(cancellationToken);

        return existing.ToHashSet();
    }

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludingServiceId = null,
        CancellationToken cancellationToken = default)
        => _db.Services.AnyAsync(
            service => service.Name == name
                       && (excludingServiceId == null || service.Id != excludingServiceId),
            cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _db.Services.CountAsync(cancellationToken);

    public Task AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        _db.Services.Add(service);
        return Task.CompletedTask;
    }

    public void Remove(Service service) => _db.Services.Remove(service);
}
