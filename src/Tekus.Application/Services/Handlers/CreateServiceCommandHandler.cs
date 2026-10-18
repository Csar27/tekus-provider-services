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

/// <summary>Adds a service to the catalogue and records the countries where it is offered.</summary>
internal sealed class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ServiceDetailDto>
{
    private readonly IServiceRepository _services;
    private readonly ICountryCatalog _countryCatalog;
    private readonly IServiceReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CreateServiceCommandHandler(
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

    public async Task<ServiceDetailDto> HandleAsync(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var name = Service.NormalizeName(request.Name);

        if (await _services.NameExistsAsync(name, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"A service named '{name}' already exists in the catalogue.");
        }

        var now = _timeProvider.UtcNow();

        var service = Service.Create(name, request.HourlyRateUsd, request.Description, now);

        var countries = await CountryAssignmentGuard.NormalizeAndEnsureExistAsync(
            _countryCatalog,
            request.CountryCodes,
            cancellationToken);

        foreach (var countryCode in countries)
        {
            service.OfferIn(countryCode, now);
        }

        await _services.AddAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _readRepository.ReadDetailAsync(service.Id, cancellationToken);
    }
}
