using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tekus.Domain.Common;

namespace Tekus.Infrastructure.Messaging;

/// <summary>
/// Publishes the domain events collected by the aggregates of the current unit of work.
/// <para>
/// Handlers are resolved from the container through the closed <c>IDomainEventHandler&lt;T&gt;</c>
/// interface, discovered once per event type by reflection and then cached as a compiled delegate
/// (no <c>dynamic</c>, no repeated <c>MakeGenericMethod</c> on the hot path).
/// </para>
/// </summary>
internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, Func<DomainEventDispatcher, IDomainEvent, CancellationToken, Task>>
        DispatchMethodCache = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            var dispatch = DispatchMethodCache.GetOrAdd(domainEvent.GetType(), CreateDispatchMethod);

            try
            {
                await dispatch(this, domainEvent, cancellationToken);
            }
            catch (Exception exception)
            {
                // A failing side effect must not lose the information about what went wrong.
                _logger.LogError(
                    exception,
                    "Domain event {EventType} could not be dispatched",
                    domainEvent.GetType().Name);

                throw;
            }
        }
    }

    private static Func<DomainEventDispatcher, IDomainEvent, CancellationToken, Task> CreateDispatchMethod(Type eventType)
    {
        var method = typeof(DomainEventDispatcher)
            .GetMethod(nameof(DispatchSingleAsync), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(eventType);

        return (dispatcher, domainEvent, token) =>
            (Task)method.Invoke(dispatcher, [domainEvent, token])!;
    }

    private async Task DispatchSingleAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

        await Task.WhenAll(handlers.Select(handler => handler.HandleAsync(domainEvent, cancellationToken)));
    }
}
