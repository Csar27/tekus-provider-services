using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Providers.Commands;
using Tekus.Application.Providers.Dtos;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Providers;

namespace Tekus.Application.Providers.Handlers;

/// <summary>Adds or updates a single custom field of a provider.</summary>
internal sealed class UpsertProviderCustomFieldCommandHandler
    : IRequestHandler<UpsertProviderCustomFieldCommand, ProviderDetailDto>
{
    private readonly IProviderRepository _providers;
    private readonly IProviderReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public UpsertProviderCustomFieldCommandHandler(
        IProviderRepository providers,
        IProviderReadRepository readRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _providers = providers;
        _readRepository = readRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<ProviderDetailDto> HandleAsync(
        UpsertProviderCustomFieldCommand request,
        CancellationToken cancellationToken)
    {
        var provider = await _providers.GetByIdAsync(request.ProviderId, cancellationToken)
                       ?? throw NotFoundException.For<Provider>(request.ProviderId);

        provider.UpsertCustomField(request.Key, request.Value, _timeProvider.UtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _readRepository.ReadDetailAsync(provider.Id, cancellationToken);
    }
}
