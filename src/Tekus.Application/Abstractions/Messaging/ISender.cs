namespace Tekus.Application.Abstractions.Messaging;

/// <summary>
/// In-process dispatcher. Controllers depend on this abstraction instead of resolving handlers
/// one by one, which keeps the API surface thin and the behaviours (validation, logging) applied
/// uniformly to every use case.
/// </summary>
public interface ISender
{
    Task<TResult> SendAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResult>;
}
