namespace Tekus.Domain.Abstractions;

/// <summary>
/// Commits every change tracked during the current use case in a single transaction and then
/// publishes the domain events raised by the aggregates.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes and returns how many rows were affected.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
