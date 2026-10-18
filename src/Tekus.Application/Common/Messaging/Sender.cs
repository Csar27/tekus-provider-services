using Microsoft.Extensions.DependencyInjection;
using Tekus.Application.Abstractions.Messaging;

namespace Tekus.Application.Common.Messaging;

/// <summary>
/// Default dispatcher: resolves the handler of the request from the container and composes the
/// registered pipeline behaviours around it (the first one registered ends up being the outermost).
/// <para>
/// Writing these ~20 lines instead of taking a dependency on a mediator library keeps the
/// contract explicit and fully type-safe (no reflection over the response type, no dynamic).
/// See docs/decisions/0003-in-house-cqrs-dispatcher.md.
/// </para>
/// </summary>
internal sealed class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;

    public Sender(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public Task<TResult> SendAsync<TRequest, TResult>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResult>
    {
        ArgumentNullException.ThrowIfNull(request);

        var handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResult>>();

        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResult>>()
            .Reverse()
            .ToArray();

        Func<CancellationToken, Task<TResult>> pipeline = token => handler.HandleAsync(request, token);

        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            pipeline = token => behavior.HandleAsync(request, next, token);
        }

        return pipeline(cancellationToken);
    }
}
