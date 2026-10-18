namespace Tekus.Domain.Providers.Events;

/// <summary>Raised when a provider's data is edited or when it is (de)activated.</summary>
public sealed record ProviderUpdatedDomainEvent(Guid ProviderId, string Nit, string Name, DateTime OccurredAtUtc)
    : Common.DomainEvent(OccurredAtUtc);
