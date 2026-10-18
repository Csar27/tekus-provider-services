using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Auth.Dtos;
using Tekus.Application.Common.Messaging;

namespace Tekus.Application.Auth.Commands;

/// <summary>Exchanges user credentials for a pair of access + refresh tokens.</summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<AuthTokensDto>;

/// <summary>Rotates a refresh token: the consumed token is revoked and replaced.</summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthTokensDto>;

/// <summary>Logout: revokes the given refresh token (idempotent).</summary>
public sealed record RevokeRefreshTokenCommand(string RefreshToken) : IRequest<Unit>;
