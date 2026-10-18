using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Common.Messaging;
using Tekus.Application.Services.Commands;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Services;

namespace Tekus.Application.Services.Handlers;

/// <summary>Logical delete of a service.</summary>
internal sealed class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, Unit>
{
    private readonly IServiceRepository _services;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public DeleteServiceCommandHandler(
        IServiceRepository services,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _services = services;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> HandleAsync(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _services.GetByIdAsync(request.Id, cancellationToken)
                      ?? throw NotFoundException.For<Service>(request.Id);

        service.Deactivate(_timeProvider.UtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
