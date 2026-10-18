namespace Tekus.Application.Abstractions.Messaging;

/// <summary>
/// Cross-cutting step that wraps the execution of a request (validation, logging, metrics,
/// transaction handling ...). Behaviours are resolved from the container and executed in
/// registration order, the first one registered being the outermost.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    Task<TResult> HandleAsync(
        TRequest request,
        Func<CancellationToken, Task<TResult>> nextHandler,
        CancellationToken cancellationToken);
}
