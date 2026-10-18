using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Messaging;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Providers.Commands;
using Tekus.Application.Providers.Dtos;
using Tekus.Application.Providers.Queries;
using Tekus.Api.Contracts;

namespace Tekus.Api.Controllers;

/// <summary>
/// CRUD of providers plus the sub-resources that belong to the provider aggregate: the user
/// defined custom fields and the set of services the provider offers.
/// <para>
/// Reads are available to every authenticated user; writes require the <c>Admin</c> role. Updates
/// support the <c>If-Match</c> header (optimistic concurrency): sending a stale version returns
/// <c>409 Conflict</c> instead of silently overwriting another user's change.
/// </para>
/// </summary>
public sealed class ProvidersController : ApiControllerBase
{
    public ProvidersController(ISender sender)
        : base(sender)
    {
    }

    /// <summary>
    /// Paginated, searchable and sortable list of providers.
    /// Supports <c>page</c>, <c>pageSize</c>, <c>search</c>, <c>sortBy</c>, <c>sortDirection</c>,
    /// <c>isActive</c>, <c>countryCode</c> and <c>serviceId</c>.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<ProviderListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProviderListItemDto>>> GetProviders(
        [FromQuery] GetProvidersQuery query,
        CancellationToken cancellationToken)
        => Ok(await SendAsync<GetProvidersQuery, PagedResult<ProviderListItemDto>>(query, cancellationToken));

    /// <summary>Single provider, including its custom fields and the services it offers.</summary>
    [HttpGet("{id:guid}", Name = "GetProviderById")]
    [Authorize]
    [ProducesResponseType(typeof(ProviderDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderDetailDto>> GetProviderById(Guid id, CancellationToken cancellationToken)
    {
        var provider = await SendAsync<GetProviderByIdQuery, ProviderDetailDto>(
            new GetProviderByIdQuery(id),
            cancellationToken);

        return Ok(provider);
    }

    /// <summary>Creates a provider, optionally with custom fields and offered services.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProviderDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProviderDetailDto>> CreateProvider(
        CreateProviderRequest request,
        CancellationToken cancellationToken)
    {
        var provider = await SendAsync<CreateProviderCommand, ProviderDetailDto>(
            new CreateProviderCommand(
                request.Nit ?? string.Empty,
                request.Name ?? string.Empty,
                request.Email ?? string.Empty,
                request.CustomFields?.Select(field => new ProviderCustomFieldInput(field.Key ?? string.Empty, field.Value ?? string.Empty)).ToArray(),
                request.ServiceIds),
            cancellationToken);

        return CreatedAtRoute("GetProviderById", new { id = provider.Id }, provider);
    }

    /// <summary>Edits the data of a provider. Send <c>If-Match: {"version"}</c> for optimistic concurrency.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProviderDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderDetailDto>> UpdateProvider(
        Guid id,
        UpdateProviderRequest request,
        CancellationToken cancellationToken)
    {
        var provider = await SendAsync<UpdateProviderCommand, ProviderDetailDto>(
            new UpdateProviderCommand(
                id,
                request.Nit ?? string.Empty,
                request.Name ?? string.Empty,
                request.Email ?? string.Empty,
                GetExpectedVersion()),
            cancellationToken);

        return Ok(provider);
    }

    /// <summary>Activates or deactivates a provider (logical delete / restore).</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProviderDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderDetailDto>> ChangeProviderStatus(
        Guid id,
        ChangeStatusRequest request,
        CancellationToken cancellationToken)
        => Ok(await SendAsync<ChangeProviderStatusCommand, ProviderDetailDto>(
            new ChangeProviderStatusCommand(id, request.IsActive),
            cancellationToken));

    /// <summary>Logically deletes a provider. Its offerings are kept for historical reporting.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProvider(Guid id, CancellationToken cancellationToken)
    {
        await SendAsync<DeleteProviderCommand, Unit>(new DeleteProviderCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>Replaces the whole set of custom fields (an empty list clears them all).</summary>
    [HttpPut("{id:guid}/custom-fields")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProviderDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderDetailDto>> ReplaceCustomFields(
        Guid id,
        ReplaceCustomFieldsRequest request,
        CancellationToken cancellationToken)
    {
        var provider = await SendAsync<ReplaceProviderCustomFieldsCommand, ProviderDetailDto>(
            new ReplaceProviderCustomFieldsCommand(
                id,
                request.Fields?.Select(field => new ProviderCustomFieldInput(field.Key ?? string.Empty, field.Value ?? string.Empty)).ToArray()),
            cancellationToken);

        return Ok(provider);
    }

    /// <summary>Creates or updates a single custom field, matched by key.</summary>
    [HttpPut("{id:guid}/custom-fields/{key}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProviderDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderDetailDto>> UpsertCustomField(
        Guid id,
        string key,
        UpsertCustomFieldRequest request,
        CancellationToken cancellationToken)
        => Ok(await SendAsync<UpsertProviderCustomFieldCommand, ProviderDetailDto>(
            new UpsertProviderCustomFieldCommand(id, key, request.Value ?? string.Empty),
            cancellationToken));

    /// <summary>Removes a single custom field by key.</summary>
    [HttpDelete("{id:guid}/custom-fields/{key}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveCustomField(Guid id, string key, CancellationToken cancellationToken)
    {
        await SendAsync<RemoveProviderCustomFieldCommand, Unit>(
            new RemoveProviderCustomFieldCommand(id, key),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Replaces the whole set of services offered by the provider.</summary>
    [HttpPut("{id:guid}/services")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProviderDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderDetailDto>> ReplaceServices(
        Guid id,
        ReplaceProviderServicesRequest request,
        CancellationToken cancellationToken)
        => Ok(await SendAsync<ReplaceProviderServicesCommand, ProviderDetailDto>(
            new ReplaceProviderServicesCommand(id, request.ServiceIds),
            cancellationToken));
}
