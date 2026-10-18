namespace Tekus.Domain.Services.Events;

/// <summary>Raised when a new service is added to the catalogue.</summary>
public sealed record ServiceCreatedDomainEvent(Guid ServiceId, string Name, DateTime OccurredAtUtc)
    : Common.DomainEvent(OccurredAtUtc);
