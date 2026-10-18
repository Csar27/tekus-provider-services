using FluentValidation;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Auth.Commands;
using Tekus.Application.Auth.Dtos;
using Tekus.Domain.Common;
using Tekus.Domain.Users;

namespace Tekus.Application.Auth.Queries;

/// <summary>Returns the identity behind the access token.</summary>
public sealed record GetCurrentUserQuery : IRequest<CurrentUserDto>;

/// <summary>Validates the login payload.</summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public const int PasswordMaxLength = 128;

    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .MaximumLength(User.EmailMaxLength)
            .Matches(EmailAddress.Pattern);

        // The password is only checked for size: the real rule is the comparison against the hash.
        RuleFor(command => command.Password)
            .NotEmpty()
            .MaximumLength(PasswordMaxLength);
    }
}

/// <summary>Validates the refresh / revoke payloads.</summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
        => RuleFor(command => command.RefreshToken).NotEmpty().MaximumLength(512);
}

/// <summary>Validates the logout payload.</summary>
public sealed class RevokeRefreshTokenCommandValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenCommandValidator()
        => RuleFor(command => command.RefreshToken).NotEmpty().MaximumLength(512);
}
