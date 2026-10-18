using Microsoft.EntityFrameworkCore;
using Tekus.Domain.Countries;

namespace Tekus.Infrastructure.Persistence.Repositories;

internal sealed class CountryRepository : ICountryRepository
{
    private readonly TekusDbContext _db;

    public CountryRepository(TekusDbContext db) => _db = db;

    public async Task<bool> IsEmptyAsync(CancellationToken cancellationToken = default)
        => !await _db.Countries.AnyAsync(cancellationToken);

    public async Task<DateTime?> GetLastSyncUtcAsync(CancellationToken cancellationToken = default)
        => await _db.Countries.MaxAsync(country => (DateTime?)country.LastSyncedAtUtc, cancellationToken);

    public Task<Country?> GetByCodeAsync(CountryCode code, CancellationToken cancellationToken = default)
        => _db.Countries.FirstOrDefaultAsync(country => country.Code == code, cancellationToken);

    public async Task<IReadOnlyCollection<CountryCode>> GetAllCodesAsync(
        CancellationToken cancellationToken = default)
        => await _db.Countries
            .AsNoTracking()
            .Select(country => country.Code)
            .ToListAsync(cancellationToken);

    public Task AddRangeAsync(IEnumerable<Country> countries, CancellationToken cancellationToken = default)
        => _db.Countries.AddRangeAsync(countries, cancellationToken);

    public void Update(Country country) => _db.Countries.Update(country);
}
