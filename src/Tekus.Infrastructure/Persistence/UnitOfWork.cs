using Microsoft.EntityFrameworkCore;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Common;

namespace Tekus.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of the unit of work.
/// <para>
/// A single call commits every change tracked during the request and then publishes the domain
/// events raised by the aggregates. A second commit persists the side effects that themselves write
/// to the database (the audit trail), so callers never have to remember to save twice.
/// </para>
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TekusDbContext _db;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public UnitOfWork(TekusDbContext db, IDomainEventDispatcher domainEventDispatcher)
    {
        _db = db;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // DIAG
        foreach (var e in _db.ChangeTracker.Entries())
        {
            var pk = e.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
            Console.WriteLine($"DIAG {e.State} {e.Metadata.ClrType.Name} pk={pk?.Name}:{(pk is null ? "" : e.Property(pk.Name).CurrentValue)}");
        }

        var affectedRows = await _db.SaveChangesAsync(cancellationToken);

        var pendingEvents = _db.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToArray();

        if (pendingEvents.Length == 0)
        {
            return affectedRows;
        }

        // Clear first: a handler that re-reads the aggregate must not see (and re-publish) the
        // event that is currently being handled.
        foreach (var aggregate in _db.ChangeTracker.Entries<AggregateRoot>().Select(entry => entry.Entity))
        {
            aggregate.ClearDomainEvents();
        }

        await _domainEventDispatcher.DispatchAsync(pendingEvents, cancellationToken);

        // Side effects such as AuditLogDomainEventHandler add rows of their own.
        return affectedRows + await _db.SaveChangesAsync(cancellationToken);
    }
}
