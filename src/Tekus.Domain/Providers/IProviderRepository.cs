namespace Tekus.Domain.Providers;

/// <summary>
/// Write side repository of the <see cref="Provider"/> aggregate.
/// It only exposes aggregate-oriented operations; all the read models required by the UI live in
/// <c>IProviderReadRepository</c> (application layer), which keeps query concerns out of the domain.
/// </summary>
public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>True when another provider (different from <paramref name="excludingProviderId"/>) already uses the NIT.</summary>
    Task<bool> NitExistsAsync(string nit, Guid? excludingProviderId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Provider provider, CancellationToken cancellationToken = default);

    void Remove(Provider provider);
}
