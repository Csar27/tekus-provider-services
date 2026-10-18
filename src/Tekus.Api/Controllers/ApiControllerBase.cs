using Microsoft.AspNetCore.Mvc;
using Tekus.Application.Abstractions.Messaging;

namespace Tekus.Api.Controllers;

/// <summary>
/// Base class of every controller. It carries the shared conventions of the API:
/// JSON only, a uniform <c>ProblemDetails</c> error shape (see <c>GlobalExceptionHandler</c>) and
/// a single place that forwards a request to the CQRS dispatcher.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public abstract class ApiControllerBase : ControllerBase
{
    private readonly ISender _sender;

    protected ApiControllerBase(ISender sender) => _sender = sender;

    /// <summary>Forwards the request to the application dispatcher (pipeline runs validators + logging).</summary>
    protected Task<TResult> SendAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResult>
        => _sender.SendAsync<TRequest, TResult>(request, cancellationToken);

    /// <summary>
    /// Reads the concurrency token from the <c>If-Match</c> header.
    /// <para>
    /// Accepts the plain form (<c>If-Match: 5</c>), the quoted form (<c>If-Match: "5"</c>) and the
    /// weak form (<c>If-Match: W/"5"</c>). A missing or unparseable header means "no concurrency
    /// check", so the client can opt in without having to track the version.
    /// </para>
    /// </summary>
    protected long? GetExpectedVersion()
    {
        if (!Request.Headers.TryGetValue("If-Match", out var values))
        {
            return null;
        }

        var raw = values.ToString().Trim();

        if (raw.Length == 0 || raw == "*")
        {
            return null;
        }

        if (raw.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
        {
            raw = raw[2..].Trim();
        }

        raw = raw.Trim('"').Trim();

        return long.TryParse(raw, out var version) ? version : null;
    }
}
