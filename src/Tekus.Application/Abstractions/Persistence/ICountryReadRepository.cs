using Tekus.Application.Common.Pagination;
using Tekus.Application.Countries.Dtos;
using Tekus.Application.Countries.Queries;

namespace Tekus.Application.Abstractions.Persistence;

/// <summary>
/// Read side of the countries snapshot. The data always comes from the local table so that
/// filtering, ordering and paging are done by the database (the external API offers none of them).
/// </summary>
public interface ICountryReadRepository
{
    Task<PagedResult<CountryDto>> GetPagedAsync(
        GetCountriesQuery query,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetExistingCodesAsync(
        IEnumerable<string> codes,
        CancellationToken cancellationToken = default);
}
