using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Common.Validation;
using Tekus.Application.Providers.Commands;
using Tekus.Application.Providers.Dtos;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Providers;
using Tekus.Domain.Services;

namespace Tekus.Application.Providers.Handlers;

/// <summary>Replaces the set of services offered by a provider.</summary>
internal sealed class ReplaceProviderServicesCommandHandler
    : IRequestHandler<ReplaceProviderServicesCommand, ProviderDetailDto>
{
    private readonly IProviderRepository _providers;
    private readonly IServiceRepository _services;
    private readonly IProviderReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ReplaceProviderServicesCommandHandler(
        IProviderRepository providers,
        IServiceRepository services,
        IProviderReadRepository readRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _providers = providers;
        _services = services;
        _readRepository = readRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<ProviderDetailDto> HandleAsync(
        ReplaceProviderServicesCommand request,
        CancellationToken cancellationToken)
    {
        var provider = await _providers.GetByIdAsync(request.ProviderId, cancellationToken)
                       ?? throw NotFoundException.For<Provider>(request.ProviderId);

        var serviceIds = request.ServiceIds?.Distinct().ToArray() ?? [];

        await ServiceAssignmentGuard.EnsureExistAsync(_services, serviceIds, cancellationToken);

        provider.ReplaceServices(serviceIds, _timeProvider.UtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _readRepository.ReadDetailAsync(provider.Id, cancellationToken);
    }
}
