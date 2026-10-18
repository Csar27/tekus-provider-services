using System.Linq.Expressions;

namespace Tekus.Application.Common.Pagination;

/// <summary>
/// Ordering helper shared by every read repository. It only accepts the keys declared by the
/// query (<see cref="PageQuery.AllowedSortKeys"/>), which removes any possibility of injecting
/// column names and always appends a deterministic tie-breaker so paging never repeats or skips
/// rows when two records share the same value.
/// </summary>
public static class QueryableOrderingExtensions
{
    public static IQueryable<T> ApplyOrdering<T>(
        this IQueryable<T> source,
        PageQuery query,
        IReadOnlyDictionary<string, Expression<Func<T, object?>>> sortMap,
        Expression<Func<T, object?>> tieBreaker)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(sortMap);
        ArgumentNullException.ThrowIfNull(tieBreaker);

        var key = sortMap.TryGetValue(query.EffectiveSortKey, out var expression) ? expression : tieBreaker;

        return query.SortDirection == SortDirection.Desc
            ? source.OrderByDescending(key).ThenByDescending(tieBreaker)
            : source.OrderBy(key).ThenBy(tieBreaker);
    }
}
