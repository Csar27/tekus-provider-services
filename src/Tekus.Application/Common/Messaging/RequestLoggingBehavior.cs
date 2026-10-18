using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tekus.Application.Abstractions.Messaging;

namespace Tekus.Application.Common.Messaging;

/// <summary>
/// Logs the duration and the outcome of every use case. Cheap to add and very useful in
/// production: it turns structured logs into per-endpoint telemetry without touching the handlers.
/// </summary>
internal sealed class RequestLoggingBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly ILogger<RequestLoggingBehavior<TRequest, TResult>> _logger;

    public RequestLoggingBehavior(ILogger<RequestLoggingBehavior<TRequest, TResult>> logger) => _logger = logger;

    public async Task<TResult> HandleAsync(
        TRequest request,
        Func<CancellationToken, Task<TResult>> nextHandler,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await nextHandler(cancellationToken);
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Use case {RequestName} completed in {ElapsedMilliseconds} ms",
                    requestName,
                    elapsedMilliseconds);
            }

            return response;
        }
        catch (Exception exception)
        {
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(
                    exception,
                    "Use case {RequestName} failed after {ElapsedMilliseconds} ms",
                    requestName,
                    elapsedMilliseconds);
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}
