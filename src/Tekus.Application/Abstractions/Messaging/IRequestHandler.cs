namespace Tekus.Application.Abstractions.Messaging;

/// <summary>
/// Handles exactly one request type. Handlers are the only place where a use case is orchestrated:
/// they load aggregates through repositories, execute domain behaviour, save through the unit of
/// work and finally re-read the read model that the API returns.
/// </summary>
public interface IRequestHandler<in TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
