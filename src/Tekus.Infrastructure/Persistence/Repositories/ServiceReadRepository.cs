using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Services.Dtos;
using Tekus.Application.Services.Queries;
using Tekus.Domain.Countries;
using Tekus.Domain.Services;

namespace Tekus.Infrastructure.Persistence.Repositories;

internal sealed class ServiceReadRepository : IServiceReadRepository
{
    private readonly TekusDbContext _db;

    // Built in the constructor (not statically) because the "providercount" key needs to
    // correlate with the ProviderServices table through the injected context.
    private readonly IReadOnlyDictionary<string, Expression<Func<Service, object?>>> _orderingMap;

    public ServiceReadRepository(TekusDbContext db)
    {
        _db = db;

        _orderingMap = new Dictionary<string, Expression<Func<Service, object?>>>(StringComparer.Ordinal)
        {
            ["name"] = service => service.Name,
            ["hourlyrate"] = service => service.HourlyRateUsd,
            ["createdat"] = service => service.CreatedAtUtc,
            ["updatedat"] = service => service.UpdatedAtUtc,
            ["countrycount"] = service => service.Countries.Count,
            ["providercount"] = service => _db.ProviderServices.Count(offering => offering.ServiceId == service.Id),
        };
    }

    public async Task<PagedResult<ServiceListItemDto>> GetPagedAsync(
        GetServicesQuery query,
        CancellationToken cancellationToken = default)
    {
        var services = _db.Services.AsNoTracking();

        if (query.IsActive is { } isActive)
        {
            services = services.Where(service => service.IsActive == isActive);
        }

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var countryCode = CountryCode.Create(query.CountryCode);

            services = services.Where(service => service.Countries
                .Any(availability => availability.CountryCode == countryCode));
        }

        if (query.MinHourlyRateUsd is { } min)
        {
            services = services.Where(service => service.HourlyRateUsd >= min);
        }

        if (query.MaxHourlyRateUsd is { } max)
        {
            services = services.Where(service => service.HourlyRateUsd <= max);
        }

        if (query.NormalizedSearch is { } search)
        {
            services = services.Where(service =>
                service.Name.Contains(search)
                || (service.Description != null && service.Description.Contains(search)));
        }

        var totalCount = await services.CountAsync(cancellationToken);

        var (pageNumber, pageSize) = query.NormalizePaging();

        var keys = services.ApplyOrdering(query, _orderingMap, service => service.Id);

        // CountryCode is a value object: it can be selected from SQL but its .Value is only read
        // after materialisation, so the projection uses an intermediate shape.
        var rows = await keys
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(service => new
            {
                service.Id,
                service.Name,
                service.HourlyRateUsd,
                service.IsActive,
                CountryCount = service.Countries.Count,
                ProviderCount = _db.ProviderServices.Count(offering => offering.ServiceId == service.Id),
                CountryCodes = service.Countries.Select(country => country.CountryCode).ToList(),
                service.CreatedAtUtc,
                service.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new ServiceListItemDto(
                row.Id,
                row.Name,
                row.HourlyRateUsd,
                row.IsActive,
                row.CountryCount,
                row.ProviderCount,
                row.CountryCodes.Select(code => code.Value).ToArray(),
                row.CreatedAtUtc,
                row.UpdatedAtUtc))
            .ToList();

        return new PagedResult<ServiceListItemDto>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<ServiceDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await _db.Services
            .AsNoTracking()
            .Include(entity => entity.Countries)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (service is null)
        {
            return null;
        }

        var countryNames = await _db.ServiceCountries
            .AsNoTracking()
            .Where(availability => availability.ServiceId == id)
            .Join(
                _db.Countries.AsNoTracking(),
                availability => availability.CountryCode,
                country => country.Code,
                (availability, country) => new { availability.CountryCode, country.Name })
            .ToListAsync(cancellationToken);

        var namesByCode = countryNames
            .ToDictionary(row => row.CountryCode.Value, row => row.Name, StringComparer.OrdinalIgnoreCase);

        var countries = service.Countries
            .Select(availability => availability.CountryCode)
            .Distinct()
            .Select(code => new ServiceCountryDto(
                code.Value,
                namesByCode.TryGetValue(code.Value, out var name) ? name : code.Value))
            .OrderBy(country => country.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        // EF Core cannot translate an OrderBy applied over a projection that invokes a
        // record constructor, so the sort happens on the joined row and the DTO is built last.
        var providers = await _db.ProviderServices
            .AsNoTracking()
            .Where(offering => offering.ServiceId == id)
            .Join(
                _db.Providers.AsNoTracking(),
                offering => offering.ProviderId,
                provider => provider.Id,
                (_, provider) => new { provider.Id, provider.Name, provider.Nit })
            .OrderBy(row => row.Name)
            .Select(row => new ProviderReferenceDto(row.Id, row.Name, row.Nit))
            .ToListAsync(cancellationToken);

        return new ServiceDetailDto(
            service.Id,
            service.Name,
            service.HourlyRateUsd,
            service.Description,
            service.IsActive,
            countries,
            providers,
            service.CreatedAtUtc,
            service.UpdatedAtUtc,
            service.Version);
    }
}
