namespace Tekus.Application.Abstractions.Persistence;

/// <summary>Read side of the service catalogue feature.</summary>
public interface IServiceReadRepository
{
    Task<Common.Pagination.PagedResult<Services.Dtos.ServiceListItemDto>> GetPagedAsync(
        Services.Queries.GetServicesQuery query,
        CancellationToken cancellationToken = default);

    Task<Services.Dtos.ServiceDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
}
