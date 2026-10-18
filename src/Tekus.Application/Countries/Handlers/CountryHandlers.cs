using Tekus.Application.Abstractions.Countries;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Countries.Dtos;
using Tekus.Application.Countries.Queries;

namespace Tekus.Application.Countries.Handlers;

/// <summary>
/// Lists the countries of the catalogue. Before querying, it asks the catalog adapter to populate
/// the snapshot: that call is a no-op when the data is still fresh, so the endpoint stays fast.
/// </summary>
internal sealed class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, PagedResult<CountryDto>>
{
    private readonly ICountryCatalog _countryCatalog;
    private readonly ICountryReadRepository _readRepository;

    public GetCountriesQueryHandler(ICountryCatalog countryCatalog, ICountryReadRepository readRepository)
    {
        _countryCatalog = countryCatalog;
        _readRepository = readRepository;
    }

    public async Task<PagedResult<CountryDto>> HandleAsync(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        await _countryCatalog.SyncAsync(force: false, cancellationToken);

        return await _readRepository.GetPagedAsync(request, cancellationToken);
    }
}

/// <summary>Forces a synchronisation with the external country service.</summary>
internal sealed class RefreshCountriesCommandHandler : IRequestHandler<RefreshCountriesCommand, CountrySyncResult>
{
    private readonly ICountryCatalog _countryCatalog;

    public RefreshCountriesCommandHandler(ICountryCatalog countryCatalog) => _countryCatalog = countryCatalog;

    public Task<CountrySyncResult> HandleAsync(RefreshCountriesCommand request, CancellationToken cancellationToken)
        => _countryCatalog.SyncAsync(request.Force, cancellationToken);
}
