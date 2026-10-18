using FluentValidation;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Dashboard.Dtos;

namespace Tekus.Application.Dashboard.Queries;

/// <summary>Summary endpoint with the two required indicators (plus two complementary blocks).</summary>
public sealed record GetDashboardSummaryQuery(int TopProviders = 5) : IRequest<DashboardSummaryDto>;

/// <summary>Validates the parameters of <see cref="GetDashboardSummaryQuery"/>.</summary>
public sealed class GetDashboardSummaryQueryValidator : AbstractValidator<GetDashboardSummaryQuery>
{
    public GetDashboardSummaryQueryValidator()
        => RuleFor(query => query.TopProviders)
            .InclusiveBetween(1, 20)
            .WithMessage("'topProviders' must be between 1 and 20.");
}
