using FluentValidation;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Countries.Queries;

namespace Tekus.Application.Countries.Validation;

/// <summary>Validates the filters of <see cref="GetCountriesQuery"/>.</summary>
public sealed class GetCountriesQueryValidator : PageQueryValidator<GetCountriesQuery>
{
    public GetCountriesQueryValidator()
    {
        RuleFor(query => query.Region)
            .MaximumLength(80)
            .When(query => !string.IsNullOrWhiteSpace(query.Region));
    }
}
