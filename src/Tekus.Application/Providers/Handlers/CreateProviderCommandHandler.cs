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

/// <summary>Creates a provider with its custom fields and offered services.</summary>
internal sealed class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, ProviderDetailDto>
{
    private readonly IProviderRepository _providers;
    private readonly IServiceRepository _services;
    private readonly IProviderReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CreateProviderCommandHandler(
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

    public async Task<ProviderDetailDto> HandleAsync(CreateProviderCommand request, CancellationToken cancellationToken)
    {
        // The NIT is normalised before looking for duplicates so "900.123.456-7" and "900123456-7"
        // are recognised as the same legal identifier.
        var nit = Provider.NormalizeNit(request.Nit);

        if (await _providers.NitExistsAsync(nit, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"A provider with NIT '{nit}' already exists.");
        }

        var now = _timeProvider.UtcNow();
        var provider = Provider.Create(nit, request.Name, request.Email, now);

        foreach (var field in request.CustomFields ?? [])
        {
            provider.UpsertCustomField(field.Key, field.Value, now);
        }

        var serviceIds = request.ServiceIds?.Distinct().ToArray() ?? [];

        await ServiceAssignmentGuard.EnsureExistAsync(_services, serviceIds, cancellationToken);

        foreach (var serviceId in serviceIds)
        {
            provider.AssignService(serviceId, now);
        }

        await _providers.AddAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _readRepository.ReadDetailAsync(provider.Id, cancellationToken);
    }
}
