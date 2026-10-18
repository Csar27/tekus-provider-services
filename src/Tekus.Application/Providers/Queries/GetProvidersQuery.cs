using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Providers.Dtos;

namespace Tekus.Application.Providers.Queries;

/// <summary>
/// Paginated, searchable and sortable list of providers.
/// </summary>
public sealed record GetProvidersQuery : PageQuery, IRequest<PagedResult<ProviderListItemDto>>
{
    private static readonly HashSet<string> SortKeys = new(StringComparer.Ordinal)
    {
        "name",
        "nit",
        "email",
        "createdat",
        "updatedat",
        "servicecount",
    };

    /// <summary>Filters by state. <c>null</c> returns both active and inactive providers.</summary>
    public bool? IsActive { get; init; }

    /// <summary>Only providers offering at least one service available in this country.</summary>
    public string? CountryCode { get; init; }

    /// <summary>Only providers offering this service.</summary>
    public Guid? ServiceId { get; init; }

    public override IReadOnlySet<string> AllowedSortKeys => SortKeys;

    public override string DefaultSortKey => "name";
}

/// <summary>Single provider, including custom fields and offered services.</summary>
public sealed record GetProviderByIdQuery(Guid Id) : IRequest<ProviderDetailDto>;
