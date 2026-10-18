using FluentValidation;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Services.Commands;
using Tekus.Application.Services.Queries;
using Tekus.Domain.Services;

namespace Tekus.Application.Services.Validation;

/// <summary>Validates the payload of <see cref="CreateServiceCommand"/>.</summary>
public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .Length(3, Service.NameMaxLength)
            .Matches(Service.NamePattern)
            .WithMessage("'name' accepts letters, digits, spaces and _.,-()/&+ only.");

        RuleFor(command => command.HourlyRateUsd)
            .GreaterThan(0)
            .LessThanOrEqualTo(Service.MaxHourlyRateUsd)
            .WithMessage($"'hourlyRateUsd' must be greater than 0 and lower than or equal to {Service.MaxHourlyRateUsd}.");

        RuleFor(command => command.Description)
            .MaximumLength(Service.DescriptionMaxLength);

        RuleFor(command => command.CountryCodes)
            .Must(codes => codes is null || codes.Count <= Service.MaxCountries);

        RuleForEach(command => command.CountryCodes).CountryCode();
    }
}

/// <summary>Validates the payload of <see cref="UpdateServiceCommand"/>.</summary>
public sealed class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .Length(3, Service.NameMaxLength);

        RuleFor(command => command.HourlyRateUsd)
            .GreaterThan(0)
            .LessThanOrEqualTo(Service.MaxHourlyRateUsd);

        RuleFor(command => command.Description)
            .MaximumLength(Service.DescriptionMaxLength);

        RuleFor(command => command.ExpectedVersion)
            .GreaterThanOrEqualTo(0)
            .When(command => command.ExpectedVersion.HasValue);
    }
}

/// <summary>Validates the payload of <see cref="ChangeServiceStatusCommand"/>.</summary>
public sealed class ChangeServiceStatusCommandValidator : AbstractValidator<ChangeServiceStatusCommand>
{
    public ChangeServiceStatusCommandValidator() => RuleFor(command => command.Id).NotEmpty();
}

/// <summary>Validates the payload of <see cref="ReplaceServiceCountriesCommand"/>.</summary>
public sealed class ReplaceServiceCountriesCommandValidator : AbstractValidator<ReplaceServiceCountriesCommand>
{
    public ReplaceServiceCountriesCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();

        RuleFor(command => command.CountryCodes)
            .Must(codes => codes is null || codes.Count <= Service.MaxCountries);

        RuleForEach(command => command.CountryCodes).CountryCode();
    }
}

/// <summary>Validates the filters of <see cref="GetServicesQuery"/>.</summary>
public sealed class GetServicesQueryValidator : PageQueryValidator<GetServicesQuery>
{
    public GetServicesQueryValidator()
    {
        RuleFor(query => query.CountryCode)
            .Length(2)
            .Matches("^[A-Za-z]{2}$")
            .When(query => !string.IsNullOrWhiteSpace(query.CountryCode));

        RuleFor(query => query.MinHourlyRateUsd)
            .GreaterThanOrEqualTo(0)
            .When(query => query.MinHourlyRateUsd.HasValue);

        RuleFor(query => query.MaxHourlyRateUsd)
            .GreaterThanOrEqualTo(0)
            .When(query => query.MaxHourlyRateUsd.HasValue);

        RuleFor(query => query).Must(query => query.MinHourlyRateUsd is null
                                              || query.MaxHourlyRateUsd is null
                                              || query.MinHourlyRateUsd <= query.MaxHourlyRateUsd)
            .WithMessage("'minHourlyRateUsd' must be lower than or equal to 'maxHourlyRateUsd'.");
    }
}

/// <summary>Validates the identifier of <see cref="GetServiceByIdQuery"/>.</summary>
public sealed class GetServiceByIdQueryValidator : AbstractValidator<GetServiceByIdQuery>
{
    public GetServiceByIdQueryValidator() => RuleFor(query => query.Id).NotEmpty();
}

/// <summary>Reusable ISO 3166-1 alpha-2 rule.</summary>
public static class CountryCodeValidationExtensions
{
    public static IRuleBuilderOptions<T, string> CountryCode<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder
            .NotEmpty()
            .Length(2)
            .Matches("^[A-Za-z]{2}$")
            .WithMessage("Country codes must be two letter ISO 3166-1 alpha-2 codes (e.g. CO, PE, MX).");
}
