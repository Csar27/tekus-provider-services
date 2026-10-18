namespace Tekus.Application.Common.Pagination;

/// <summary>Direction of the ordering requested by the client.</summary>
public enum SortDirection
{
    /// <summary>From A to Z / lowest to highest.</summary>
    Asc = 1,

    /// <summary>From Z to A / highest to lowest.</summary>
    Desc = 2,
}
