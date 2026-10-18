namespace Tekus.Infrastructure.Persistence.Auditing;

/// <summary>
/// Append-only audit trail. One row is written for every domain event published by the unit of
/// work, which gives the system an explainable history of who changed what and when without
/// touching the write model.
/// </summary>
public sealed class AuditLog
{
    public const int EventTypeMaxLength = 120;
    public const int AggregateTypeMaxLength = 64;
    public const int SummaryMaxLength = 1000;

    /// <summary>Required by EF Core.</summary>
    private AuditLog()
    {
        EventType = string.Empty;
        AggregateType = string.Empty;
        Summary = string.Empty;
    }

    public AuditLog(
        Guid id,
        DateTime occurredAtUtc,
        string eventType,
        string aggregateType,
        Guid aggregateId,
        string summary)
    {
        Id = id;
        OccurredAtUtc = occurredAtUtc;
        EventType = eventType;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        Summary = summary;
    }

    public Guid Id { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    /// <summary>Stable event name (e.g. <c>provider.created</c>).</summary>
    public string EventType { get; private set; } = string.Empty;

    public string AggregateType { get; private set; } = string.Empty;

    public Guid AggregateId { get; private set; }

    /// <summary>Human readable description, useful when inspecting the table directly.</summary>
    public string Summary { get; private set; } = string.Empty;
}
