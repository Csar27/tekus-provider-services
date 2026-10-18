using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Providers.Dtos;

namespace Tekus.Application.Providers.Handlers;

/// <summary>
/// Every write handler answers with the freshly materialised read model, so clients do not need a
/// second request after a POST/PUT (and the API can still return 201 + Location).
/// </summary>
internal static class ProviderReadModelExtensions
{
    public static async Task<ProviderDetailDto> ReadDetailAsync(
        this IProviderReadRepository repository,
        Guid providerId,
        CancellationToken cancellationToken)
        => await repository.GetDetailAsync(providerId, cancellationToken)
           ?? throw new NotFoundException($"Provider '{providerId}' could not be read back after the operation.");
}
