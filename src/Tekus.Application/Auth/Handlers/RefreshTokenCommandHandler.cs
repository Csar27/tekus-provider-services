using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Security;
using Tekus.Application.Auth.Commands;
using Tekus.Application.Auth.Dtos;
using Tekus.Application.Common;
using Tekus.Application.Common.Exceptions;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Users;

namespace Tekus.Application.Auth.Handlers;

/// <summary>
/// Exchanges a valid refresh token for a brand new pair (rotation). The consumed token is revoked
/// and linked to its replacement.
/// <para>
/// If a token that has already been used (or revoked) shows up again, the whole session family is
/// revoked: that is the standard mitigation for a stolen refresh token, because the legitimate
/// client and the attacker cannot both keep refreshing. See <c>docs/architecture.md</c>.
/// </para>
/// </summary>
internal sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthTokensDto>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public RefreshTokenCommandHandler(
        IUserRepository users,
        ITokenService tokenService,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _users = users;
        _tokenService = tokenService;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<AuthTokensDto> HandleAsync(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.UtcNow();
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

        var user = await _users.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException();
        }

        if (!user.HasActiveRefreshToken(tokenHash, now))
        {
            // Replay detected: this token belongs to the user but is no longer active, so it was
            // already consumed. Revoke every active session of the family as a precaution.
            user.RevokeAllRefreshTokens(now, "refresh_token_replay");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException(
                "The refresh token is expired or has already been used. Please sign in again.");
        }

        // RotateRefreshToken issues the replacement *and* revokes the consumed token, linking both
        // so a later replay of the consumed token can be detected.
        var plainRefreshToken = _tokenService.GenerateRefreshToken();
        var newTokenHash = _tokenService.HashRefreshToken(plainRefreshToken);
        var newToken = user.RotateRefreshToken(tokenHash, newTokenHash, _currentUser.IpAddress, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AuthTokenFactory.Build(_tokenService, plainRefreshToken, user, newToken, now);
    }
}
