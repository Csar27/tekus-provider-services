using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tekus.Application.Common.Exceptions;
using Tekus.Domain.Common;

namespace Tekus.Api.Middleware;

/// <summary>
/// Translates the exceptions raised by the domain and the application layer into the RFC 7807
/// <c>ProblemDetails</c> format, so every error response of the API has the same shape:
/// <code>
/// { "type": "...", "title": "...", "status": 400, "detail": "...", "code": "validation_error", "errors": { ... } }
/// </code>
/// The machine readable <c>code</c> property is what clients branch on (it mirrors
/// <see cref="ErrorCodes"/>), keeping the human readable <c>detail</c> free to change.
/// </summary>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, code) = Map(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            // Only unexpected failures reach this branch: log them with the stack trace.
            _logger.LogError(exception, "Unhandled exception while processing {Path}", httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Request {Path} failed with {StatusCode} ({Code}): {Message}",
                httpContext.Request.Path,
                statusCode,
                code,
                exception.Message);
        }

        ProblemDetails problemDetails = exception switch
        {
            ValidationException validation => new ValidationProblemDetails(BuildModelState(validation.Errors))
            {
                Status = statusCode,
                Title = title,
                Detail = validation.Message,
            },
            _ => new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
            },
        };

        problemDetails.Instance = httpContext.Request.Path;
        problemDetails.Extensions["code"] = code;

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception,
            });
    }

    private static (int StatusCode, string Title, string Code) Map(Exception exception)
        => exception switch
        {
            // --- Domain (business rules) -------------------------------------
            DomainException { Code: ErrorCodes.NotFound } domain
                => (StatusCodes.Status404NotFound, "Resource not found", domain.Code),

            DomainException { Code: ErrorCodes.ExternalServiceUnavailable } domain
                => (StatusCodes.Status503ServiceUnavailable, "External service unavailable", domain.Code),

            DomainException { Code: ErrorCodes.InvalidCredentials or ErrorCodes.AccountLocked } domain
                => (StatusCodes.Status401Unauthorized, "Authentication failed", domain.Code),

            DomainException domain
                => (StatusCodes.Status400BadRequest, "Business rule violation", domain.Code),

            // --- Application (expected failures) ----------------------------
            ValidationException validation
                => (StatusCodes.Status400BadRequest, "One or more validation errors occurred.", validation.Code),

            NotFoundException notFound
                => (StatusCodes.Status404NotFound, "Resource not found", notFound.Code),

            ConflictException conflict
                => (StatusCodes.Status409Conflict, "Conflict with the current state", conflict.Code),

            UnauthorizedException unauthorized
                => (StatusCodes.Status401Unauthorized, "Unauthorized", unauthorized.Code),

            BadHttpRequestException badRequest
                => (badRequest.StatusCode, "Malformed request", "malformed_request"),

            // --- Everything else is a bug ------------------------------------
            _
                => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", "internal_error"),
        };

    /// <summary>Turns the flat error dictionary of the application layer into MVC's model state.</summary>
    private static ModelStateDictionary BuildModelState(IReadOnlyDictionary<string, string[]> errors)
    {
        var modelState = new ModelStateDictionary();

        foreach (var (property, messages) in errors)
        {
            foreach (var message in messages)
            {
                modelState.AddModelError(property, message);
            }
        }

        return modelState;
    }
}
