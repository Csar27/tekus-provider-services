using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Providers.Commands;
using Tekus.Application.Providers.Dtos;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Providers;

namespace Tekus.Application.Providers.Handlers;

/// <summary>Activates or deactivates a provider without deleting its history.</summary>
internal sealed class ChangeProviderStatusCommandHandler
    : IRequestHandler<ChangeProviderStatusCommand, ProviderDetailDto>
{
    private readonly IProviderRepository _providers;
    private readonly IProviderReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ChangeProviderStatusCommandHandler(
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
        ChangeProviderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var provider = await _providers.GetByIdAsync(request.Id, cancellationToken)
                       ?? throw NotFoundException.For<Provider>(request.Id);

        var now = _timeProvider.UtcNow();

        if (request.IsActive)
        {
            provider.Activate(now);
        }
        else
        {
            provider.Deactivate(now);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _readRepository.ReadDetailAsync(provider.Id, cancellationToken);
    }
}
