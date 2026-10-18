using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Common.Messaging;
using Tekus.Application.Providers.Commands;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Providers;

namespace Tekus.Application.Providers.Handlers;

/// <summary>
/// Logical delete of a provider (the row is kept so the historical indicators stay consistent).
/// </summary>
internal sealed class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, Unit>
{
    private readonly IProviderRepository _providers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public DeleteProviderCommandHandler(
        IProviderRepository providers,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _providers = providers;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> HandleAsync(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providers.GetByIdAsync(request.Id, cancellationToken)
                       ?? throw NotFoundException.For<Provider>(request.Id);

        provider.Deactivate(_timeProvider.UtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
