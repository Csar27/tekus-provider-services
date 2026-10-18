using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Services.Commands;
using Tekus.Application.Services.Dtos;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Services;

namespace Tekus.Application.Services.Handlers;

/// <summary>Activates or deactivates a service of the catalogue.</summary>
internal sealed class ChangeServiceStatusCommandHandler : IRequestHandler<ChangeServiceStatusCommand, ServiceDetailDto>
{
    private readonly IServiceRepository _services;
    private readonly IServiceReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ChangeServiceStatusCommandHandler(
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

    public async Task<ServiceDetailDto> HandleAsync(
        ChangeServiceStatusCommand request,
        CancellationToken cancellationToken)
    {
        var service = await _services.GetByIdAsync(request.Id, cancellationToken)
                      ?? throw NotFoundException.For<Service>(request.Id);

        var now = _timeProvider.UtcNow();

        if (request.IsActive)
        {
            service.Activate(now);
        }
        else
        {
            service.Deactivate(now);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _readRepository.ReadDetailAsync(service.Id, cancellationToken);
    }
}
