namespace Tekus.Application.Auth.Dtos;

/// <summary>Tokens returned by <c>POST /api/v1/auth/login</c> and <c>/auth/refresh</c>.</summary>
public sealed record AuthTokensDto(
    string AccessToken,
    string TokenType,
    int ExpiresInSeconds,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);

/// <summary>Authenticated identity, returned by <c>GET /api/v1/auth/me</c>.</summary>
public sealed record CurrentUserDto(Guid Id, string Email, string FullName, string Role);
