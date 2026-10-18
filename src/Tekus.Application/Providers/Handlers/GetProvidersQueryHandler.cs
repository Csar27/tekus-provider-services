using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Providers.Dtos;
using Tekus.Application.Providers.Queries;

namespace Tekus.Application.Providers.Handlers;

/// <summary>List of providers with paging, ordering and search.</summary>
internal sealed class GetProvidersQueryHandler
    : IRequestHandler<GetProvidersQuery, PagedResult<ProviderListItemDto>>
{
    private readonly IProviderReadRepository _readRepository;

    public GetProvidersQueryHandler(IProviderReadRepository readRepository) => _readRepository = readRepository;

    public Task<PagedResult<ProviderListItemDto>> HandleAsync(
        GetProvidersQuery request,
        CancellationToken cancellationToken)
        => _readRepository.GetPagedAsync(request, cancellationToken);
}
