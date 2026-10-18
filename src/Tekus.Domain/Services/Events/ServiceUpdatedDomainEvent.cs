namespace Tekus.Domain.Services.Events;

/// <summary>Raised when the data or the state of a service changes.</summary>
public sealed record ServiceUpdatedDomainEvent(Guid ServiceId, string Name, DateTime OccurredAtUtc)
    : Common.DomainEvent(OccurredAtUtc);
