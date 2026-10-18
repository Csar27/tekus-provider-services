using Tekus.Domain.Common;

namespace Tekus.Application.Common.Exceptions;

/// <summary>
/// The caller could not be authenticated (bad credentials, expired/revoked refresh token).
/// Translated into <c>401 Unauthorized</c>. Messages are intentionally generic so the endpoint
/// never discloses whether an account exists.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public const string GenericMessage = "The credentials provided are not valid.";

    public UnauthorizedException(string? message = null, string code = ErrorCodes.InvalidCredentials)
        : base(message ?? GenericMessage) => Code = code;

    public string Code { get; }
}
