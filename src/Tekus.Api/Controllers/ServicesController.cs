using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Messaging;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Services.Commands;
using Tekus.Application.Services.Dtos;
using Tekus.Application.Services.Queries;
using Tekus.Api.Contracts;

namespace Tekus.Api.Controllers;

/// <summary>
/// The service catalogue. A service is a reusable offer (with an hourly rate in USD) that many
/// providers can reference; it also carries the list of countries where it is available.
/// </summary>
public sealed class ServicesController : ApiControllerBase
{
    public ServicesController(ISender sender)
        : base(sender)
    {
    }

    /// <summary>
    /// Paginated, searchable and sortable list of services.
    /// Supports <c>page</c>, <c>pageSize</c>, <c>search</c>, <c>sortBy</c>, <c>sortDirection</c>,
    /// <c>isActive</c>, <c>countryCode</c>, <c>minHourlyRateUsd</c> and <c>maxHourlyRateUsd</c>.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<ServiceListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ServiceListItemDto>>> GetServices(
        [FromQuery] GetServicesQuery query,
        CancellationToken cancellationToken)
        => Ok(await SendAsync<GetServicesQuery, PagedResult<ServiceListItemDto>>(query, cancellationToken));

    /// <summary>Single service with its countries and the providers that offer it.</summary>
    [HttpGet("{id:guid}", Name = "GetServiceById")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceDetailDto>> GetServiceById(Guid id, CancellationToken cancellationToken)
        => Ok(await SendAsync<GetServiceByIdQuery, ServiceDetailDto>(new GetServiceByIdQuery(id), cancellationToken));

    /// <summary>Adds a service to the catalogue, with the countries where it is offered.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ServiceDetailDto>> CreateService(
        CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var service = await SendAsync<CreateServiceCommand, ServiceDetailDto>(
            new CreateServiceCommand(
                request.Name ?? string.Empty,
                request.HourlyRateUsd,
                request.Description,
                request.CountryCodes),
            cancellationToken);

        return CreatedAtRoute("GetServiceById", new { id = service.Id }, service);
    }

    /// <summary>Edits a service. Send <c>If-Match: {"version"}</c> for optimistic concurrency.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceDetailDto>> UpdateService(
        Guid id,
        UpdateServiceRequest request,
        CancellationToken cancellationToken)
        => Ok(await SendAsync<UpdateServiceCommand, ServiceDetailDto>(
            new UpdateServiceCommand(id, request.Name ?? string.Empty, request.HourlyRateUsd, request.Description, GetExpectedVersion()),
            cancellationToken));

    /// <summary>Activates or deactivates a service.</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceDetailDto>> ChangeServiceStatus(
        Guid id,
        ChangeStatusRequest request,
        CancellationToken cancellationToken)
        => Ok(await SendAsync<ChangeServiceStatusCommand, ServiceDetailDto>(
            new ChangeServiceStatusCommand(id, request.IsActive),
            cancellationToken));

    /// <summary>Logically deletes a service (it stays available for historical reporting).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteService(Guid id, CancellationToken cancellationToken)
    {
        await SendAsync<DeleteServiceCommand, Unit>(new DeleteServiceCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>Replaces the list of countries where the service is offered.</summary>
    [HttpPut("{id:guid}/countries")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServiceDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceDetailDto>> ReplaceCountries(
        Guid id,
        ReplaceCountriesRequest request,
        CancellationToken cancellationToken)
        => Ok(await SendAsync<ReplaceServiceCountriesCommand, ServiceDetailDto>(
            new ReplaceServiceCountriesCommand(id, request.CountryCodes),
            cancellationToken));
}
