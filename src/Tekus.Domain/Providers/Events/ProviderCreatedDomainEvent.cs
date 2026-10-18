namespace Tekus.Domain.Providers.Events;

/// <summary>Raised when a provider is created. Consumed by the audit trail.</summary>
public sealed record ProviderCreatedDomainEvent(Guid ProviderId, string Nit, string Name, DateTime OccurredAtUtc)
    : Common.DomainEvent(OccurredAtUtc);
