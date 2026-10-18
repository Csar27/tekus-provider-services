namespace Tekus.Application.Common.Pagination;

/// <summary>
/// Base class of every list query. It carries the three features required by the challenge
/// (paging, ordering and searching) and normalises the values so a badly formatted query string
/// can never take down the endpoint nor force a full table scan without a limit.
/// </summary>
public abstract record PageQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int MaxSearchLength = 200;

    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Rows per page (1..<see cref="MaxPageSize"/>).</summary>
    public int PageSize { get; init; } = DefaultPageSize;

    /// <summary>Free text search applied over the searchable columns of each projection.</summary>
    public string? Search { get; init; }

    /// <summary>Key of the ordering to apply. Must be one of <see cref="AllowedSortKeys"/>.</summary>
    public string? SortBy { get; init; }

    /// <summary>Ordering direction.</summary>
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;

    /// <summary>
    /// Sort keys accepted by the query. Whitelisting them keeps the ordering safe (no dynamic
    /// SQL from user input) and documents what the client may ask for.
    /// </summary>
    public abstract IReadOnlySet<string> AllowedSortKeys { get; }

    /// <summary>Sort applied when the client does not send a valid key.</summary>
    public abstract string DefaultSortKey { get; }

    public int EffectivePageNumber => PageNumber < 1 ? 1 : PageNumber;

    public int EffectivePageSize => PageSize is < 1 or > MaxPageSize ? DefaultPageSize : PageSize;

    public int Skip => (EffectivePageNumber - 1) * EffectivePageSize;

    public int Take => EffectivePageSize;

    public string? NormalizedSearch
        => string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();

    public string EffectiveSortKey
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SortBy))
            {
                return DefaultSortKey;
            }

            var candidate = SortBy.Trim().ToLowerInvariant();

            return AllowedSortKeys.Contains(candidate) ? candidate : DefaultSortKey;
        }
    }
}
