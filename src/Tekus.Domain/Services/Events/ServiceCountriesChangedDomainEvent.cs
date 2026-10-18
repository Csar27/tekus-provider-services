namespace Tekus.Domain.Services.Events;

/// <summary>Raised when a country is added to or removed from the availability list.</summary>
public sealed record ServiceCountriesChangedDomainEvent(Guid ServiceId, string CountryCode, DateTime OccurredAtUtc)
    : Common.DomainEvent(OccurredAtUtc);
