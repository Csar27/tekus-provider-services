using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Common.Messaging;
using Tekus.Application.Providers.Commands;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Providers;

namespace Tekus.Application.Providers.Handlers;

/// <summary>Removes a custom field from a provider.</summary>
internal sealed class RemoveProviderCustomFieldCommandHandler
    : IRequestHandler<RemoveProviderCustomFieldCommand, Unit>
{
    private readonly IProviderRepository _providers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public RemoveProviderCustomFieldCommandHandler(
        IProviderRepository providers,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _providers = providers;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> HandleAsync(RemoveProviderCustomFieldCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providers.GetByIdAsync(request.ProviderId, cancellationToken)
                       ?? throw NotFoundException.For<Provider>(request.ProviderId);

        provider.RemoveCustomField(request.Key, _timeProvider.UtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
