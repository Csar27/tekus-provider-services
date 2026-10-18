namespace Tekus.Domain.Providers.Events;

/// <summary>Raised when a custom field is added, updated or removed.</summary>
public sealed record ProviderCustomFieldChangedDomainEvent(Guid ProviderId, string Key, DateTime OccurredAtUtc)
    : Common.DomainEvent(OccurredAtUtc);
