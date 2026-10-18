namespace Tekus.Application.Common;

/// <summary>Helpers to obtain UTC timestamps from the injected <see cref="TimeProvider"/>.</summary>
public static class TimeProviderExtensions
{
    /// <summary>
    /// Current UTC instant as a <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/>.
    /// All the timestamps of the system are UTC ("all dates in UTC" convention, see
    /// docs/architecture.md) and this is the single place where "now" is read, which makes the
    /// whole application testable with a fake clock.
    /// </summary>
    public static DateTime UtcNow(this TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        return timeProvider.GetUtcNow().UtcDateTime;
    }
}
