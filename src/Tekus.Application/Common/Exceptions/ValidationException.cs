using FluentValidation.Results;
using Tekus.Domain.Common;

namespace Tekus.Application.Common.Exceptions;

/// <summary>
/// One or more inputs broke a validation rule. Translated by the API into a
/// <c>400 Bad Request</c> with a <c>ValidationProblemDetails</c> body.
/// </summary>
public sealed class ValidationException : Exception
{
    private const string DefaultMessage = "One or more validation errors occurred.";

    public ValidationException(string propertyName, string message)
        : this(new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) { [propertyName] = [message] })
    {
    }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base(DefaultMessage) => Errors = errors;

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this(GroupFailures(failures))
    {
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public string Code { get; } = ErrorCodes.Validation;

    private static Dictionary<string, string[]> GroupFailures(IEnumerable<ValidationFailure> failures)
        => failures
            .GroupBy(failure => string.IsNullOrWhiteSpace(failure.PropertyName) ? "request" : failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray(),
                StringComparer.OrdinalIgnoreCase);
}
