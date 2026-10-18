namespace Tekus.Domain.Providers.Events;

/// <summary>Raised when a service of the catalogue starts being offered by the provider.</summary>
public sealed record ProviderServiceAssignedDomainEvent(Guid ProviderId, Guid ServiceId, DateTime OccurredAtUtc)
    : Common.DomainEvent(OccurredAtUtc);
