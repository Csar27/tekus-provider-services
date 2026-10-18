using FluentValidation;

namespace Tekus.Application.Common.Pagination;

/// <summary>
/// Rules shared by every paged query: paging limits, search length and the sort whitelist.
/// Query specific validators simply inherit from it and add their own filters.
/// </summary>
public abstract class PageQueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : PageQuery
{
    protected PageQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("'pageNumber' must be greater than or equal to 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, PageQuery.MaxPageSize)
            .WithMessage($"'pageSize' must be between 1 and {PageQuery.MaxPageSize}.");

        RuleFor(query => query.Search)
            .MaximumLength(PageQuery.MaxSearchLength);

        RuleFor(query => query.SortBy)
            .Must((query, sortBy) => sortBy is null
                                     || query.AllowedSortKeys.Contains(sortBy.Trim().ToLowerInvariant()))
            .WithMessage(query => $"'sortBy' must be one of: {string.Join(", ", query.AllowedSortKeys.OrderBy(key => key, StringComparer.Ordinal))}.");

        RuleFor(query => query.SortDirection).IsInEnum();
    }
}
