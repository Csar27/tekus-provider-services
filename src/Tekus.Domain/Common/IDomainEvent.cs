namespace Tekus.Domain.Common;

/// <summary>
/// Something that already happened inside the domain and that other parts of the system may
/// care about (audit, notifications, projections). Events are raised while the aggregate is
/// mutated and published *after* the unit of work commits.
/// </summary>
public interface IDomainEvent
{
    /// <summary>UTC instant at which the event was raised.</summary>
    DateTime OccurredAtUtc { get; }
}

/// <summary>Convenience base record so every event carries the occurrence timestamp.</summary>
public abstract record DomainEvent(DateTime OccurredAtUtc) : IDomainEvent;

/// <summary>
/// Handler of a concrete domain event. Implementations live outside the domain
/// (typically in the infrastructure) so the domain stays free of I/O concerns.
/// </summary>
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}

/// <summary>Dispatches the events collected by the aggregates of a unit of work.</summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
