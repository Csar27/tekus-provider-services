using Tekus.Domain.Common;

namespace Tekus.Domain.Providers;

/// <summary>
/// Join entity that links a provider with a service of the catalogue.
/// It is part of the <see cref="Provider"/> aggregate and carries the moment in which the
/// offering started, which is what makes the dashboard figures explainable over time.
/// </summary>
public sealed class ProviderService : Entity
{
    /// <summary>Required by EF Core.</summary>
    private ProviderService()
    {
    }

    private ProviderService(Guid id, Guid providerId, Guid serviceId, DateTime utcNow)
        : base(id)
    {
        ProviderId = providerId;
        ServiceId = serviceId;
        AssignedAtUtc = utcNow;
    }

    public Guid ProviderId { get; private set; }

    public Guid ServiceId { get; private set; }

    public DateTime AssignedAtUtc { get; private set; }

    internal static ProviderService Assign(Guid providerId, Guid serviceId, DateTime utcNow)
        => new(Guid.NewGuid(), providerId, serviceId, utcNow);
}
