using Tekus.Domain.Common;

namespace Tekus.Domain.Services;

/// <summary>Write side repository of the <see cref="Service"/> aggregate.</summary>
public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>Ids of the given list that actually exist in the catalogue (used to validate input).</summary>
    Task<IReadOnlySet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, Guid? excludingServiceId = null, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Service service, CancellationToken cancellationToken = default);

    void Remove(Service service);
}
