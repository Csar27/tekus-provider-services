using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Dashboard.Dtos;
using Tekus.Application.Dashboard.Queries;

namespace Tekus.Application.Dashboard.Handlers;

/// <summary>Delegates the aggregation to the read repository (single SQL round trip set).</summary>
internal sealed class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IDashboardReadRepository _readRepository;

    public GetDashboardSummaryQueryHandler(IDashboardReadRepository readRepository)
        => _readRepository = readRepository;

    public Task<DashboardSummaryDto> HandleAsync(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
        => _readRepository.GetSummaryAsync(request.TopProviders, cancellationToken);
}
