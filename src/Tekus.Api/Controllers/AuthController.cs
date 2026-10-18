using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Security;
using Tekus.Application.Auth.Commands;
using Tekus.Application.Auth.Dtos;
using Tekus.Application.Auth.Queries;
using Tekus.Application.Common.Messaging;
using Tekus.Api.Contracts;

namespace Tekus.Api.Controllers;

/// <summary>
/// Credentials and session management.
/// <para>
/// <c>POST /login</c> exchanges a e-mail + password for an access token (JWT) and a refresh token.
/// <c>POST /refresh</c> rotates the refresh token (the consumed one is revoked and linked to its
/// replacement). <c>POST /logout</c> revokes a refresh token and is idempotent. <c>GET /me</c>
/// returns the identity behind the current access token.
/// </para>
/// </summary>
[Route("api/v1/auth")]
public sealed class AuthController : ApiControllerBase
{
    public AuthController(ISender sender)
        : base(sender)
    {
    }

    /// <summary>Signs in a user and returns the token pair.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthTokensDto>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var tokens = await SendAsync<LoginCommand, AuthTokensDto>(
            new LoginCommand(request.Email ?? string.Empty, request.Password ?? string.Empty),
            cancellationToken);

        return Ok(tokens);
    }

    /// <summary>Rotates a refresh token into a brand new token pair.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthTokensDto>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokens = await SendAsync<RefreshTokenCommand, AuthTokensDto>(
            new RefreshTokenCommand(request.RefreshToken ?? string.Empty),
            cancellationToken);

        return Ok(tokens);
    }

    /// <summary>Revokes the given refresh token (logout). Idempotent.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(RevokeTokenRequest request, CancellationToken cancellationToken)
    {
        await SendAsync<RevokeRefreshTokenCommand, Unit>(
            new RevokeRefreshTokenCommand(request.RefreshToken ?? string.Empty),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Returns the identity behind the access token (used by the SPA to restore a session).</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken cancellationToken)
    {
        var user = await SendAsync<GetCurrentUserQuery, CurrentUserDto>(
            new GetCurrentUserQuery(),
            cancellationToken);

        return Ok(user);
    }
}
