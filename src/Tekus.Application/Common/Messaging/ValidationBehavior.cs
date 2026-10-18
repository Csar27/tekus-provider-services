using FluentValidation;
using Tekus.Application.Abstractions.Messaging;
using ValidationException = Tekus.Application.Common.Exceptions.ValidationException;

namespace Tekus.Application.Common.Messaging;

/// <summary>
/// Runs every <see cref="IValidator{T}"/> registered for the request before the handler executes.
/// Validating at this level (instead of inside each handler) guarantees that no use case can
/// forget its input validation and keeps the errors in a single, uniform shape.
/// </summary>
internal sealed class ValidationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly IValidator<TRequest>[] _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators as IValidator<TRequest>[] ?? validators.ToArray();

    public async Task<TResult> HandleAsync(
        TRequest request,
        Func<CancellationToken, Task<TResult>> nextHandler,
        CancellationToken cancellationToken)
    {
        if (_validators.Length == 0)
        {
            return await nextHandler(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        return failures.Length > 0
            ? throw new ValidationException(failures)
            : await nextHandler(cancellationToken);
    }
}
