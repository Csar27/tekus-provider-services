using Tekus.Application.Abstractions.Countries;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Countries.Dtos;

namespace Tekus.Application.Countries.Queries;

/// <summary>
/// Paginated, searchable and sortable list of the countries of the external catalogue.
/// The rows are served from the local snapshot, which is populated on first use.
/// </summary>
public sealed record GetCountriesQuery : PageQuery, IRequest<PagedResult<CountryDto>>
{
    private static readonly HashSet<string> SortKeys = new(StringComparer.Ordinal)
    {
        "name",
        "code",
        "region",
    };

    /// <summary>Only countries of this region (e.g. <c>Americas</c>).</summary>
    public string? Region { get; init; }

    public override IReadOnlySet<string> AllowedSortKeys => SortKeys;

    public override string DefaultSortKey => "name";
}

/// <summary>Forces a synchronisation against the external country service.</summary>
public sealed record RefreshCountriesCommand(bool Force = false) : IRequest<CountrySyncResult>;
