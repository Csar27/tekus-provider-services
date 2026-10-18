using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Security;
using Tekus.Application.Auth.Commands;
using Tekus.Application.Auth.Dtos;
using Tekus.Application.Auth.Queries;
using Tekus.Application.Common;
using Tekus.Application.Common.Exceptions;
using Tekus.Application.Common.Messaging;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Users;

namespace Tekus.Application.Auth.Handlers;

/// <summary>
/// Logout of a single session: revokes the refresh token presented by the client. It is
/// idempotent (calling it twice, or with an unknown token, still succeeds) which keeps clients
/// from having to special-case "already logged out".
/// </summary>
internal sealed class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, Unit>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public RevokeRefreshTokenCommandHandler(
        IUserRepository users,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _users = users;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> HandleAsync(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.UtcNow();
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

        var user = await _users.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);

        if (user is not null && user.RevokeRefreshToken(tokenHash, now, "logout"))
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}

/// <summary>Returns the identity behind the access token (used by the SPA to restore its session).</summary>
internal sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserQueryHandler(IUserRepository users, ICurrentUser currentUser)
    {
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<CurrentUserDto> HandleAsync(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            throw new UnauthorizedException();
        }

        // The token carries the identity, but reading the aggregate keeps the response correct even
        // if the profile changed (or the account was deactivated) after the token was issued.
        var user = await _users.GetByIdAsync(userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException();
        }

        return new CurrentUserDto(user.Id, user.Email, user.FullName, user.Role.ToString());
    }
}
