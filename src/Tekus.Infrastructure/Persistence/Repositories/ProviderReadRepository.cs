using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Providers.Dtos;
using Tekus.Application.Providers.Queries;
using Tekus.Domain.Countries;
using Tekus.Domain.Providers;

namespace Tekus.Infrastructure.Persistence.Repositories;

internal sealed class ProviderReadRepository : IProviderReadRepository
{
    private static readonly IReadOnlyDictionary<string, Expression<Func<Provider, object?>>> OrderingMap =
        new Dictionary<string, Expression<Func<Provider, object?>>>(StringComparer.Ordinal)
        {
            ["name"] = provider => provider.Name,
            ["nit"] = provider => provider.Nit,
            ["email"] = provider => provider.Email,
            ["createdat"] = provider => provider.CreatedAtUtc,
            ["updatedat"] = provider => provider.UpdatedAtUtc,
            ["servicecount"] = provider => provider.OfferedServices.Count,
        };

    private readonly TekusDbContext _db;

    public ProviderReadRepository(TekusDbContext db) => _db = db;

    public async Task<PagedResult<ProviderListItemDto>> GetPagedAsync(
        GetProvidersQuery query,
        CancellationToken cancellationToken = default)
    {
        var providers = _db.Providers.AsNoTracking();

        if (query.IsActive is { } isActive)
        {
            providers = providers.Where(provider => provider.IsActive == isActive);
        }

        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var countryCode = CountryCode.Create(query.CountryCode);

            // Only providers that offer at least one service available in that country.
            providers = providers.Where(provider => provider.OfferedServices
                .Any(offering => _db.ServiceCountries
                    .Any(availability => availability.ServiceId == offering.ServiceId
                                         && availability.CountryCode == countryCode)));
        }

        if (query.ServiceId is { } serviceId)
        {
            providers = providers.Where(provider => provider.OfferedServices
                .Any(offering => offering.ServiceId == serviceId));
        }

        if (query.NormalizedSearch is { } search)
        {
            providers = providers.Where(provider =>
                provider.Name.Contains(search)
                || provider.Nit.Contains(search)
                || provider.Email.Contains(search));
        }

        var totalCount = await providers.CountAsync(cancellationToken);

        var (pageNumber, pageSize) = query.NormalizePaging();

        // Order on the entity (so computed keys such as ServiceCount translate to SQL) and only
        // then project and page.
        var keys = providers.ApplyOrdering(query, OrderingMap, provider => provider.Id);

        var items = await keys
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(provider => new ProviderListItemDto(
                provider.Id,
                provider.Nit,
                provider.Name,
                provider.Email,
                provider.IsActive,
                provider.OfferedServices.Count,
                provider.CustomFields.Count,
                provider.CreatedAtUtc,
                provider.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProviderListItemDto>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<ProviderDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _db.Providers
            .AsNoTracking()
            .Include(entity => entity.CustomFields)
            .Include(entity => entity.OfferedServices)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (provider is null)
        {
            return null;
        }

        var serviceIds = provider.OfferedServices.Select(offering => offering.ServiceId).ToArray();

        var services = await _db.Services
            .AsNoTracking()
            .Where(service => serviceIds.Contains(service.Id))
            .Select(service => new
            {
                service.Id,
                service.Name,
                service.HourlyRateUsd,
                CountryCodes = service.Countries.Select(country => country.CountryCode).ToList(),
            })
            .ToListAsync(cancellationToken);

        var servicesById = services.ToDictionary(service => service.Id);

        var customFields = provider.CustomFields
            .OrderBy(field => field.Key, StringComparer.OrdinalIgnoreCase)
            .Select(field => new ProviderCustomFieldDto(field.Key, field.Value))
            .ToArray();

        var offeredServices = provider.OfferedServices
            .Select(offering => servicesById.TryGetValue(offering.ServiceId, out var service)
                ? new ProviderOfferedServiceDto(
                    service.Id,
                    service.Name,
                    service.HourlyRateUsd,
                    service.CountryCodes.Select(code => code.Value).ToArray())
                : null)
            .Where(dto => dto is not null)
            .Select(dto => dto!)
            .OrderBy(dto => dto.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ProviderDetailDto(
            provider.Id,
            provider.Nit,
            provider.Name,
            provider.Email,
            provider.IsActive,
            provider.CustomFields
                .OrderBy(field => field.Key, StringComparer.OrdinalIgnoreCase)
                .Select(field => new ProviderCustomFieldDto(field.Key, field.Value))
                .ToArray(),
            offeredServices,
            provider.CreatedAtUtc,
            provider.UpdatedAtUtc,
            provider.Version);
    }
}
