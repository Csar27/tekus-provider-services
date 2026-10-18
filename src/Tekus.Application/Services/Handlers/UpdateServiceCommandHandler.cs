using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Services.Commands;
using Tekus.Application.Services.Dtos;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Services;

namespace Tekus.Application.Services.Handlers;

/// <summary>Edits a service of the catalogue (name, hourly rate and description).</summary>
internal sealed class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ServiceDetailDto>
{
    private readonly IServiceRepository _services;
    private readonly IServiceReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public UpdateServiceCommandHandler(
        IServiceRepository services,
        IServiceReadRepository readRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _services = services;
        _readRepository = readRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<ServiceDetailDto> HandleAsync(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _services.GetByIdAsync(request.Id, cancellationToken)
                      ?? throw NotFoundException.For<Service>(request.Id);

        if (request.ExpectedVersion is { } expectedVersion && service.Version != expectedVersion)
        {
            throw new ConflictException(
                $"The service was modified by someone else (expected version {expectedVersion}, " +
                $"current version {service.Version}). Reload the resource and try again.",
                "concurrency_conflict");
        }

        var name = Service.NormalizeName(request.Name);

        if (await _services.NameExistsAsync(name, service.Id, cancellationToken))
        {
            throw new ConflictException($"Another service is already named '{name}'.");
        }

        service.Update(name, request.HourlyRateUsd, request.Description, _timeProvider.UtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _readRepository.ReadDetailAsync(service.Id, cancellationToken);
    }
}
