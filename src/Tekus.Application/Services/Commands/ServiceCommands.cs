using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Common.Messaging;
using Tekus.Application.Services.Dtos;

namespace Tekus.Application.Services.Commands;

/// <summary>Adds a service to the catalogue, with the countries where it is offered.</summary>
public sealed record CreateServiceCommand(
    string Name,
    decimal HourlyRateUsd,
    string? Description = null,
    IReadOnlyCollection<string>? CountryCodes = null) : IRequest<ServiceDetailDto>;

/// <summary>Edits a service of the catalogue.</summary>
public sealed record UpdateServiceCommand(
    Guid Id,
    string Name,
    decimal HourlyRateUsd,
    string? Description = null,
    long? ExpectedVersion = null) : IRequest<ServiceDetailDto>;

/// <summary>Activates or deactivates a service.</summary>
public sealed record ChangeServiceStatusCommand(Guid Id, bool IsActive) : IRequest<ServiceDetailDto>;

/// <summary>Logically deletes a service (it stays for historical reporting).</summary>
public sealed record DeleteServiceCommand(Guid Id) : IRequest<Unit>;

/// <summary>Replaces the list of countries where the service is offered.</summary>
public sealed record ReplaceServiceCountriesCommand(
    Guid Id,
    IReadOnlyCollection<string>? CountryCodes) : IRequest<ServiceDetailDto>;
