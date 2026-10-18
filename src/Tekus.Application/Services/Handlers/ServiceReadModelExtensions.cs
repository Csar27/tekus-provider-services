using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Services.Dtos;

namespace Tekus.Application.Services.Handlers;

/// <summary>Reads back the freshly written service so mutations can answer with the full resource.</summary>
internal static class ServiceReadModelExtensions
{
    public static async Task<ServiceDetailDto> ReadDetailAsync(
        this IServiceReadRepository repository,
        Guid serviceId,
        CancellationToken cancellationToken)
        => await repository.GetDetailAsync(serviceId, cancellationToken)
           ?? throw new NotFoundException($"Service '{serviceId}' could not be read back after the operation.");
}
