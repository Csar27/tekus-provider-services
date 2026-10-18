using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Countries.Dtos;
using Tekus.Application.Countries.Queries;
using Tekus.Domain.Countries;

namespace Tekus.Infrastructure.Persistence.Repositories;

internal sealed class CountryReadRepository : ICountryReadRepository
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Country, object?>>> OrderingMap =
        new Dictionary<string, Expression<Func<Country, object?>>>(StringComparer.Ordinal)
        {
            ["name"] = country => country.Name,
            ["code"] = country => country.Code,
            ["region"] = country => country.Region,
        };

    private readonly TekusDbContext _db;

    public CountryReadRepository(TekusDbContext db) => _db = db;

    public async Task<PagedResult<CountryDto>> GetPagedAsync(
        GetCountriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var countries = _db.Countries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Region))
        {
            var region = query.Region.Trim();
            countries = countries.Where(country => country.Region.Contains(region));
        }

        if (query.NormalizedSearch is { } search)
        {
            countries = countries.Where(country =>
                country.Name.Contains(search) || country.Region.Contains(search));
        }

        var totalCount = await countries.CountAsync(cancellationToken);

        var (pageNumber, pageSize) = query.NormalizePaging();

        var keys = countries.ApplyOrdering(query, OrderingMap, country => country.Code);

        var rows = await keys
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(country => new
            {
                country.Code,
                country.Name,
                country.Region,
                country.CurrencyCode,
                country.Alpha3Code,
                country.LastSyncedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new CountryDto(
                row.Code.Value,
                row.Name,
                row.Region,
                row.CurrencyCode,
                row.Alpha3Code,
                row.LastSyncedAtUtc))
            .ToList();

        return new PagedResult<CountryDto>(items, pageNumber, pageSize, totalCount);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _db.Countries.CountAsync(cancellationToken);

    public async Task<IReadOnlySet<string>> GetExistingCodesAsync(
        IEnumerable<string> codes,
        CancellationToken cancellationToken = default)
    {
        var requested = codes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(CountryCode.Create)
            .Distinct()
            .ToArray();

        var existing = await _db.Countries
            .AsNoTracking()
            .Where(country => requested.Contains(country.Code))
            .Select(country => country.Code)
            .ToListAsync(cancellationToken);

        return existing.Select(code => code.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
