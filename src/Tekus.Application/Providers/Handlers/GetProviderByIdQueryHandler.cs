using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Providers.Dtos;
using Tekus.Application.Providers.Queries;
using Tekus.Domain.Providers;

namespace Tekus.Application.Providers.Handlers;

/// <summary>Single provider with its custom fields and offered services.</summary>
internal sealed class GetProviderByIdQueryHandler : IRequestHandler<GetProviderByIdQuery, ProviderDetailDto>
{
    private readonly IProviderReadRepository _readRepository;

    public GetProviderByIdQueryHandler(IProviderReadRepository readRepository) => _readRepository = readRepository;

    public async Task<ProviderDetailDto> HandleAsync(GetProviderByIdQuery request, CancellationToken cancellationToken)
        => await _readRepository.GetDetailAsync(request.Id, cancellationToken)
           ?? throw NotFoundException.For<Provider>(request.Id);
}
