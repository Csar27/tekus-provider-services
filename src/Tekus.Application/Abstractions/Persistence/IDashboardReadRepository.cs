namespace Tekus.Application.Abstractions.Persistence;

/// <summary>Read side of the summary endpoint (requirement #4: two indicators).</summary>
public interface IDashboardReadRepository
{
    Task<Dashboard.Dtos.DashboardSummaryDto> GetSummaryAsync(
        int topProviders,
        CancellationToken cancellationToken = default);
}
