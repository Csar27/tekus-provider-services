using FluentValidation;
using Tekus.Application.Common.Pagination;
using Tekus.Application.Providers.Commands;
using Tekus.Application.Providers.Queries;
using Tekus.Domain.Providers;

namespace Tekus.Application.Providers.Validation;

/// <summary>Validates the payload of <see cref="CreateProviderCommand"/>.</summary>
public sealed class CreateProviderCommandValidator : AbstractValidator<CreateProviderCommand>
{
    public CreateProviderCommandValidator()
    {
        RuleFor(command => command.Nit)
            .NotEmpty()
            .MaximumLength(Provider.NitMaxLength)
            .Matches(Provider.NitPattern)
            .WithMessage("'nit' must contain 5 to 20 digits with an optional check digit (e.g. 900123456-7).");

        RuleFor(command => command.Name)
            .NotEmpty()
            .Length(3, Provider.NameMaxLength);

        RuleFor(command => command.Email)
            .NotEmpty()
            .MaximumLength(Provider.EmailMaxLength)
            .Matches(Provider.EmailPattern)
            .WithMessage("'email' is not a valid e-mail address.");

        RuleFor(command => command.CustomFields)
            .Must(fields => fields is null || fields.Count <= Provider.MaxCustomFields)
            .WithMessage($"A provider cannot define more than {Provider.MaxCustomFields} custom fields.");

        RuleForEach(command => command.CustomFields)
            .SetValidator(new ProviderCustomFieldInputValidator());

        RuleFor(command => command.ServiceIds)
            .Must(serviceIds => serviceIds is null || serviceIds.Count <= Provider.MaxServicesPerProvider)
            .WithMessage($"A provider cannot offer more than {Provider.MaxServicesPerProvider} services.");

        RuleForEach(command => command.ServiceIds)
            .NotEmpty()
            .WithMessage("'serviceIds' cannot contain empty identifiers.");
    }
}

/// <summary>Validates the payload of <see cref="UpdateProviderCommand"/>.</summary>
public sealed class UpdateProviderCommandValidator : AbstractValidator<UpdateProviderCommand>
{
    public UpdateProviderCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();

        RuleFor(command => command.Nit)
            .NotEmpty()
            .MaximumLength(Provider.NitMaxLength)
            .Matches(Provider.NitPattern);

        RuleFor(command => command.Name)
            .NotEmpty()
            .Length(3, Provider.NameMaxLength);

        RuleFor(command => command.Email)
            .NotEmpty()
            .MaximumLength(Provider.EmailMaxLength)
            .Matches(Provider.EmailPattern);

        RuleFor(command => command.ExpectedVersion)
            .GreaterThanOrEqualTo(0)
            .When(command => command.ExpectedVersion.HasValue);
    }
}

/// <summary>Validates the payload of <see cref="UpsertProviderCustomFieldCommand"/>.</summary>
public sealed class UpsertProviderCustomFieldCommandValidator : AbstractValidator<UpsertProviderCustomFieldCommand>
{
    public UpsertProviderCustomFieldCommandValidator()
    {
        RuleFor(command => command.ProviderId).NotEmpty();

        RuleFor(command => command.Key)
            .NotEmpty()
            .Length(2, ProviderCustomField.KeyMaxLength)
            .Matches(Provider.CustomFieldKeyPattern)
            .WithMessage("'key' accepts letters, digits, spaces and _.,-()/&+ only.");

        RuleFor(command => command.Value)
            .NotEmpty()
            .MaximumLength(ProviderCustomField.ValueMaxLength);
    }
}

/// <summary>Validates the payload of <see cref="RemoveProviderCustomFieldCommand"/>.</summary>
public sealed class RemoveProviderCustomFieldCommandValidator : AbstractValidator<RemoveProviderCustomFieldCommand>
{
    public RemoveProviderCustomFieldCommandValidator()
    {
        RuleFor(command => command.ProviderId).NotEmpty();
        RuleFor(command => command.Key).NotEmpty().MaximumLength(ProviderCustomField.KeyMaxLength);
    }
}

/// <summary>Validates the payload of <see cref="ReplaceProviderCustomFieldsCommand"/>.</summary>
public sealed class ReplaceProviderCustomFieldsCommandValidator : AbstractValidator<ReplaceProviderCustomFieldsCommand>
{
    public ReplaceProviderCustomFieldsCommandValidator()
    {
        RuleFor(command => command.ProviderId).NotEmpty();

        RuleFor(command => command.Fields)
            .Must(fields => fields is null || fields.Count <= Provider.MaxCustomFields);

        RuleForEach(command => command.Fields)
            .SetValidator(new ProviderCustomFieldInputValidator());
    }
}

/// <summary>Validates the payload of <see cref="ReplaceProviderServicesCommand"/>.</summary>
public sealed class ReplaceProviderServicesCommandValidator : AbstractValidator<ReplaceProviderServicesCommand>
{
    public ReplaceProviderServicesCommandValidator()
    {
        RuleFor(command => command.ProviderId).NotEmpty();

        RuleFor(command => command.ServiceIds)
            .Must(serviceIds => serviceIds is null || serviceIds.Count <= Provider.MaxServicesPerProvider);

        RuleForEach(command => command.ServiceIds).NotEmpty();
    }
}

/// <summary>Validates the payload of <see cref="ChangeProviderStatusCommand"/>.</summary>
public sealed class ChangeProviderStatusCommandValidator : AbstractValidator<ChangeProviderStatusCommand>
{
    public ChangeProviderStatusCommandValidator() => RuleFor(command => command.Id).NotEmpty();
}

/// <summary>Validates the filters of <see cref="GetProvidersQuery"/>.</summary>
public sealed class GetProvidersQueryValidator : PageQueryValidator<GetProvidersQuery>
{
    public GetProvidersQueryValidator()
    {
        RuleFor(query => query.CountryCode)
            .Length(2)
            .Matches("^[A-Za-z]{2}$")
            .WithMessage("'countryCode' must be a two letter ISO 3166-1 alpha-2 code.")
            .When(query => !string.IsNullOrWhiteSpace(query.CountryCode));

        RuleFor(query => query.ServiceId)
            .NotEmpty()
            .When(query => query.ServiceId.HasValue);
    }
}

/// <summary>Validates the identifier of <see cref="GetProviderByIdQuery"/>.</summary>
public sealed class GetProviderByIdQueryValidator : AbstractValidator<GetProviderByIdQuery>
{
    public GetProviderByIdQueryValidator() => RuleFor(query => query.Id).NotEmpty();
}

/// <summary>Field level rules shared by every command that receives custom fields.</summary>
public sealed class ProviderCustomFieldInputValidator : AbstractValidator<ProviderCustomFieldInput>
{
    public ProviderCustomFieldInputValidator()
    {
        RuleFor(field => field.Key)
            .NotEmpty()
            .Length(2, ProviderCustomField.KeyMaxLength)
            .Matches(Provider.CustomFieldKeyPattern)
            .WithMessage("Custom field names accept letters, digits, spaces and _.,-()/&+ only.");

        RuleFor(field => field.Value)
            .NotEmpty()
            .MaximumLength(ProviderCustomField.ValueMaxLength);
    }
}
