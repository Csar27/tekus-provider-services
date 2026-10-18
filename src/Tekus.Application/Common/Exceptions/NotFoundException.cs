using Tekus.Domain.Common;

namespace Tekus.Application.Common.Exceptions;

/// <summary>Requested resource does not exist. Translated into <c>404 Not Found</c>.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message, string code = ErrorCodes.NotFound)
        : base(message) => Code = code;

    public string Code { get; }

    public static NotFoundException For<TEntity>(object id)
        => new($"{typeof(TEntity).Name} with identifier '{id}' was not found.");
}
