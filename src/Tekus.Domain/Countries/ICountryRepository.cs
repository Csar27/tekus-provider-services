namespace Tekus.Domain.Countries;

/// <summary>
/// Write/refresh side repository of the local snapshot of the external countries catalogue.
/// The snapshot makes the API resilient (it keeps working when the external provider is down)
/// and lets the reporting queries join against a real table.
/// </summary>
public interface ICountryRepository
{
    Task<bool> IsEmptyAsync(CancellationToken cancellationToken = default);

    /// <summary>UTC instant of the most recent successful synchronisation, if any.</summary>
    Task<DateTime?> GetLastSyncUtcAsync(CancellationToken cancellationToken = default);

    Task<Country?> GetByCodeAsync(CountryCode code, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CountryCode>> GetAllCodesAsync(CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<Country> countries, CancellationToken cancellationToken = default);

    void Update(Country country);
}
