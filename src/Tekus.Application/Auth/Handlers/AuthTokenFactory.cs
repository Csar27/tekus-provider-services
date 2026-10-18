using Tekus.Application.Abstractions.Security;
using Tekus.Application.Auth.Dtos;
using Tekus.Domain.Users;

namespace Tekus.Application.Auth.Handlers;

/// <summary>
/// Small helper shared by the login and refresh flows so both build the token pair in exactly the
/// same way. Keeping the logic here means a change (e.g. a different token type) is made once.
/// </summary>
internal static class AuthTokenFactory
{
    private const string BearerTokenType = "Bearer";

    /// <summary>Issues and stores a new refresh token, returning the clear-text value and the entity.</summary>
    public static (string PlainText, RefreshToken Entity) IssueRefreshToken(
        User user,
        ITokenService tokenService,
        ICurrentUser currentUser,
        DateTime utcNow)
    {
        // Only the hash of the refresh token is persisted: the clear-text value exists solely in
        // this response and on the client, so a database leak cannot be replayed.
        var plainText = tokenService.GenerateRefreshToken();
        var hash = tokenService.HashRefreshToken(plainText);
        var entity = user.IssueRefreshToken(hash, currentUser.IpAddress, utcNow);

        return (plainText, entity);
    }

    /// <summary>Builds the response DTO out of the freshly issued access token and refresh token.</summary>
    public static AuthTokensDto Build(
        ITokenService tokenService,
        string plainRefreshToken,
        User user,
        RefreshToken refreshToken,
        DateTime utcNow)
    {
        var accessToken = tokenService.CreateAccessToken(user, utcNow);

        var expiresInSeconds = (int)Math.Max(0, (accessToken.ExpiresAtUtc - utcNow).TotalSeconds);

        return new AuthTokensDto(
            accessToken.Value,
            BearerTokenType,
            expiresInSeconds,
            accessToken.ExpiresAtUtc,
            plainRefreshToken,
            refreshToken.ExpiresAtUtc);
    }
}
