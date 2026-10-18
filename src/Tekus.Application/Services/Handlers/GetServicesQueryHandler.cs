using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Services.Dtos;
using Tekus.Application.Services.Queries;

namespace Tekus.Application.Services.Handlers;

/// <summary>List of services with paging, ordering and search.</summary>
internal sealed class GetServicesQueryHandler
    : IRequestHandler<GetServicesQuery, PagedResult<ServiceListItemDto>>
{
    private readonly IServiceReadRepository _readRepository;

    public GetServicesQueryHandler(IServiceReadRepository readRepository) => _readRepository = readRepository;

    public Task<PagedResult<ServiceListItemDto>> HandleAsync(
        GetServicesQuery request,
        CancellationToken cancellationToken)
        => _readRepository.GetPagedAsync(request, cancellationToken);
}
