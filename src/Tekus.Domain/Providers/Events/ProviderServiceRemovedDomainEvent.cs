namespace Tekus.Domain.Providers.Events;

/// <summary>Raised when a service is no longer offered by the provider.</summary>
public sealed record ProviderServiceRemovedDomainEvent(Guid ProviderId, Guid ServiceId, DateTime OccurredAtUtc)
    : Common.DomainEvent(OccurredAtUtc);
