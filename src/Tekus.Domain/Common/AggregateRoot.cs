namespace Tekus.Domain.Common;

/// <summary>
/// Root of a consistency boundary. Only aggregate roots are loaded, saved and referenced from
/// the outside; everything inside the boundary is reached through the root.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(Guid id)
        : base(id)
    {
    }

    /// <summary>
    /// Domain events raised by this aggregate during the current unit of work. They are cleared
    /// by the infrastructure right after a successful <c>SaveChanges</c>.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Records a domain event. It will be dispatched after the transaction commits.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>Clears the events (called by the persistence layer once they have been dispatched).</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
