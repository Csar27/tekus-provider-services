namespace Tekus.Application.Abstractions.Countries;

/// <summary>
/// Outbound port to the external country catalogue. The challenge requires the list of countries
/// to be queried from an external service, so this is the only dependency the application layer
/// has on it; the adapter (infrastructure) decides how to call it, how to cache it and how to
/// survive its outages.
/// </summary>
public interface ICountryCatalog
{
    /// <summary>
    /// Reconciles the local snapshot with the external provider.
    /// </summary>
    /// <param name="force">When <c>false</c> the sync is skipped if the snapshot is still fresh.</param>
    /// <param name="cancellationToken">Token used to cancel the outbound call.</param>
    Task<CountrySyncResult> SyncAsync(bool force, CancellationToken cancellationToken = default);

    /// <summary>
    /// Codes currently available. It populates the snapshot on first use (cache-fill), so
    /// validation of service countries works out of the box.
    /// </summary>
    Task<IReadOnlySet<string>> GetAvailableCodesAsync(CancellationToken cancellationToken = default);
}

/// <summary>Result of a synchronisation with the external catalogue.</summary>
/// <param name="Total">Rows in the snapshot after the sync.</param>
/// <param name="Added">New rows inserted.</param>
/// <param name="Updated">Rows whose data changed.</param>
/// <param name="SyncedAtUtc">UTC instant of the sync.</param>
/// <param name="FromCache">True when the external provider was not called (snapshot still fresh).</param>
public sealed record CountrySyncResult(int Total, int Added, int Updated, DateTime SyncedAtUtc, bool FromCache);
