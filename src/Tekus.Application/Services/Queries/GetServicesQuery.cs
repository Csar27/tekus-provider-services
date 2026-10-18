using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Services.Dtos;

namespace Tekus.Application.Services.Queries;

/// <summary>Paginated, searchable and sortable list of the service catalogue.</summary>
public sealed record GetServicesQuery : PageQuery, IRequest<PagedResult<ServiceListItemDto>>
{
    private static readonly HashSet<string> SortKeys = new(StringComparer.Ordinal)
    {
        "name",
        "hourlyrate",
        "createdat",
        "updatedat",
        "countrycount",
        "providercount",
    };

    /// <summary>Filters by state. <c>null</c> returns both active and inactive services.</summary>
    public bool? IsActive { get; init; }

    /// <summary>Only services offered in this country.</summary>
    public string? CountryCode { get; init; }

    /// <summary>Only services with an hourly rate lower than or equal to this value (USD).</summary>
    public decimal? MaxHourlyRateUsd { get; init; }

    /// <summary>Only services with an hourly rate greater than or equal to this value (USD).</summary>
    public decimal? MinHourlyRateUsd { get; init; }

    public override IReadOnlySet<string> AllowedSortKeys => SortKeys;

    public override string DefaultSortKey => "name";
}

/// <summary>Single service with its countries and the providers that offer it.</summary>
public sealed record GetServiceByIdQuery(Guid Id) : IRequest<ServiceDetailDto>;
