using Tekus.Application.Abstractions.Countries;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Common.Validation;
using Tekus.Application.Services.Commands;
using Tekus.Application.Services.Dtos;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Services;

namespace Tekus.Application.Services.Handlers;

/// <summary>Replaces the countries where the service is offered.</summary>
internal sealed class ReplaceServiceCountriesCommandHandler
    : IRequestHandler<ReplaceServiceCountriesCommand, ServiceDetailDto>
{
    private readonly IServiceRepository _services;
    private readonly ICountryCatalog _countryCatalog;
    private readonly IServiceReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ReplaceServiceCountriesCommandHandler(
        IServiceRepository services,
        ICountryCatalog countryCatalog,
        IServiceReadRepository readRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _services = services;
        _countryCatalog = countryCatalog;
        _readRepository = readRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<ServiceDetailDto> HandleAsync(
        ReplaceServiceCountriesCommand request,
        CancellationToken cancellationToken)
    {
        var service = await _services.GetByIdAsync(request.Id, cancellationToken)
                      ?? throw NotFoundException.For<Service>(request.Id);

        var countries = await CountryAssignmentGuard.NormalizeAndEnsureExistAsync(
            _countryCatalog,
            request.CountryCodes,
            cancellationToken);

        service.ReplaceCountries(countries, _timeProvider.UtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _readRepository.ReadDetailAsync(service.Id, cancellationToken);
    }
}
