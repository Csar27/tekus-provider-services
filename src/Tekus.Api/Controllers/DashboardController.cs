using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Dashboard.Dtos;
using Tekus.Application.Dashboard.Queries;

namespace Tekus.Api.Controllers;

/// <summary>
/// Headline indicators of the catalogue.
/// </summary>
public sealed class DashboardController : ApiControllerBase
{
    public DashboardController(ISender sender)
        : base(sender)
    {
    }

    /// <summary>
    /// Summary of the catalogue: totals, services per country, average hourly rate per country
    /// and the providers with the largest portfolio.
    /// </summary>
    [HttpGet("summary")]
    [Authorize]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
        [FromQuery] int topProviders = 5,
        CancellationToken cancellationToken = default)
        => Ok(await SendAsync<GetDashboardSummaryQuery, DashboardSummaryDto>(
            new GetDashboardSummaryQuery(topProviders),
            cancellationToken));
}
