using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Providers.Commands;
using Tekus.Application.Providers.Dtos;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Providers;

namespace Tekus.Application.Providers.Handlers;

/// <summary>Edits a provider, honouring the optional optimistic concurrency token.</summary>
internal sealed class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, ProviderDetailDto>
{
    private readonly IProviderRepository _providers;
    private readonly IProviderReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public UpdateProviderCommandHandler(
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

    public async Task<ProviderDetailDto> HandleAsync(UpdateProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _providers.GetByIdAsync(request.Id, cancellationToken)
                       ?? throw NotFoundException.For<Provider>(request.Id);

        if (request.ExpectedVersion is { } expectedVersion && provider.Version != expectedVersion)
        {
            throw new ConflictException(
                $"The provider was modified by someone else (expected version {expectedVersion}, " +
                $"current version {provider.Version}). Reload the resource and try again.",
                "concurrency_conflict");
        }

        var nit = Provider.NormalizeNit(request.Nit);

        if (await _providers.NitExistsAsync(nit, provider.Id, cancellationToken))
        {
            throw new ConflictException($"Another provider already uses the NIT '{nit}'.");
        }

        provider.Update(nit, request.Name, request.Email, _timeProvider.UtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _readRepository.ReadDetailAsync(provider.Id, cancellationToken);
    }
}
