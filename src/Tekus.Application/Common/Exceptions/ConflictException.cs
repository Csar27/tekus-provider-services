using Tekus.Domain.Common;

namespace Tekus.Application.Common.Exceptions;

/// <summary>
/// The request is valid but collides with the current state (duplicated NIT, stale concurrency
/// token, ...). Translated into <c>409 Conflict</c>.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message, string code = ErrorCodes.Conflict)
        : base(message) => Code = code;

    public string Code { get; }
}
