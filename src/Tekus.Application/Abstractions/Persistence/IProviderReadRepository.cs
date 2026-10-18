namespace Tekus.Application.Abstractions.Persistence;

/// <summary>
/// Read side of the providers feature. Implemented with EF Core projections (no tracking), which
/// means the API returns exactly the shape the client needs in a single round trip.
/// </summary>
public interface IProviderReadRepository
{
    Task<Common.Pagination.PagedResult<Providers.Dtos.ProviderListItemDto>> GetPagedAsync(
        Providers.Queries.GetProvidersQuery query,
        CancellationToken cancellationToken = default);

    Task<Providers.Dtos.ProviderDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
}
