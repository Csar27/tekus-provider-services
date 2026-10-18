namespace Tekus.Application.Common.Pagination;

/// <summary>
/// Envelope returned by every list endpoint. Besides the rows it exposes the metadata the client
/// needs to build a pager without a second request.
/// </summary>
public sealed record PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => PageNumber > 1 && TotalCount > 0;

    public bool HasNext => PageNumber < TotalPages;
}
