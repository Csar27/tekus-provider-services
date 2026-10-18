using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Services.Dtos;
using Tekus.Application.Services.Queries;
using Tekus.Domain.Services;

namespace Tekus.Application.Services.Handlers;

/// <summary>Single service with its countries and the providers that offer it.</summary>
internal sealed class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceDetailDto>
{
    private readonly IServiceReadRepository _readRepository;

    public GetServiceByIdQueryHandler(IServiceReadRepository readRepository) => _readRepository = readRepository;

    public async Task<ServiceDetailDto> HandleAsync(GetServiceByIdQuery request, CancellationToken cancellationToken)
        => await _readRepository.GetDetailAsync(request.Id, cancellationToken)
           ?? throw NotFoundException.For<Service>(request.Id);
}
