using Microsoft.Extensions.Logging;
using Tekus.Domain.Common;
using Tekus.Domain.Providers;
using Tekus.Domain.Providers.Events;
using Tekus.Domain.Services;
using Tekus.Domain.Services.Events;

namespace Tekus.Infrastructure.Persistence.Auditing;

/// <summary>
/// Turns every domain event published by the <see cref="UnitOfWork"/> into an append-only
/// <see cref="AuditLog"/> row.
/// <para>
/// One class implements all the event handler interfaces so the whole audit behaviour is visible
/// in a single file: adding a new event means adding one interface and one mapping here.
/// </para>
/// </summary>
internal sealed class AuditLogDomainEventHandler :
    IDomainEventHandler<ProviderCreatedDomainEvent>,
    IDomainEventHandler<ProviderUpdatedDomainEvent>,
    IDomainEventHandler<ProviderCustomFieldChangedDomainEvent>,
    IDomainEventHandler<ProviderServiceAssignedDomainEvent>,
    IDomainEventHandler<ProviderServiceRemovedDomainEvent>,
    IDomainEventHandler<ServiceCreatedDomainEvent>,
    IDomainEventHandler<ServiceUpdatedDomainEvent>,
    IDomainEventHandler<ServiceCountriesChangedDomainEvent>
{
    private readonly TekusDbContext _db;
    private readonly ILogger<AuditLogDomainEventHandler> _logger;

    public AuditLogDomainEventHandler(TekusDbContext db, ILogger<AuditLogDomainEventHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Task HandleAsync(ProviderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Append(
            domainEvent.OccurredAtUtc,
            "provider.created",
            nameof(Provider),
            domainEvent.ProviderId,
            $"Provider '{domainEvent.Name}' (NIT {domainEvent.Nit}) was created.");

        return Task.CompletedTask;
    }

    public Task HandleAsync(ProviderUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Append(
            domainEvent.OccurredAtUtc,
            "provider.updated",
            nameof(Provider),
            domainEvent.ProviderId,
            $"Provider '{domainEvent.Name}' (NIT {domainEvent.Nit}) was updated.");

        return Task.CompletedTask;
    }

    public Task HandleAsync(ProviderCustomFieldChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Append(
            domainEvent.OccurredAtUtc,
            "provider.custom_field_changed",
            nameof(Provider),
            domainEvent.ProviderId,
            $"Custom field '{domainEvent.Key}' of provider {domainEvent.ProviderId} was changed.");

        return Task.CompletedTask;
    }

    public Task HandleAsync(ProviderServiceAssignedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Append(
            domainEvent.OccurredAtUtc,
            "provider.service_assigned",
            nameof(Provider),
            domainEvent.ProviderId,
            $"Service {domainEvent.ServiceId} was assigned to provider {domainEvent.ProviderId}.");

        return Task.CompletedTask;
    }

    public Task HandleAsync(ProviderServiceRemovedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Append(
            domainEvent.OccurredAtUtc,
            "provider.service_removed",
            nameof(Provider),
            domainEvent.ProviderId,
            $"Service {domainEvent.ServiceId} was removed from provider {domainEvent.ProviderId}.");

        return Task.CompletedTask;
    }

    public Task HandleAsync(ServiceCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Append(
            domainEvent.OccurredAtUtc,
            "service.created",
            nameof(Service),
            domainEvent.ServiceId,
            $"Service '{domainEvent.Name}' was created.");

        return Task.CompletedTask;
    }

    public Task HandleAsync(ServiceUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Append(
            domainEvent.OccurredAtUtc,
            "service.updated",
            nameof(Service),
            domainEvent.ServiceId,
            $"Service '{domainEvent.Name}' was updated.");

        return Task.CompletedTask;
    }

    public Task HandleAsync(ServiceCountriesChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Append(
            domainEvent.OccurredAtUtc,
            "service.countries_changed",
            nameof(Service),
            domainEvent.ServiceId,
            $"Availability of service {domainEvent.ServiceId} changed for country {domainEvent.CountryCode}.");

        return Task.CompletedTask;
    }

    private void Append(
        DateTime occurredAtUtc,
        string eventType,
        string aggregateType,
        Guid aggregateId,
        string summary)
    {
        _db.AuditLogs.Add(new AuditLog(Guid.NewGuid(), occurredAtUtc, eventType, aggregateType, aggregateId, summary));

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Audit {EventType} for {AggregateType} {AggregateId}", eventType, aggregateType, aggregateId);
        }
    }
}
