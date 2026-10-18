using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tekus.Application.Abstractions.Countries;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Countries.Dtos;
using Tekus.Application.Countries.Queries;

namespace Tekus.Api.Controllers;

/// <summary>
/// Countries of the external catalogue, served from the local snapshot.
/// <para>
/// The snapshot is filled automatically on the first read (cache-fill) and can be forced to
/// refresh with <c>POST /api/v1/countries/refresh</c>. When the external provider is unreachable
/// the endpoint keeps answering from the snapshot instead of failing.
/// </para>
/// </summary>
public sealed class CountriesController : ApiControllerBase
{
    public CountriesController(ISender sender)
        : base(sender)
    {
    }

    /// <summary>
    /// Paginated, searchable and sortable list of countries.
    /// Supports <c>page</c>, <c>pageSize</c>, <c>search</c>, <c>sortBy</c>, <c>sortDirection</c> and <c>region</c>.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<CountryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CountryDto>>> GetCountries(
        [FromQuery] GetCountriesQuery query,
        CancellationToken cancellationToken)
        => Ok(await SendAsync<GetCountriesQuery, PagedResult<CountryDto>>(query, cancellationToken));

    /// <summary>
    /// Forces a synchronisation with the external country service.
    /// Use <c>?force=false</c> to refresh only when the snapshot is stale (the default).
    /// </summary>
    [HttpPost("refresh")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CountrySyncResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<CountrySyncResult>> RefreshCountries(
        [FromQuery] bool force = false,
        CancellationToken cancellationToken = default)
        => Ok(await SendAsync<RefreshCountriesCommand, CountrySyncResult>(
            new RefreshCountriesCommand(force),
            cancellationToken));
}
