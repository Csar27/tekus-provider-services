using Tekus.Application.Abstractions.Messaging;
using Tekus.Application.Abstractions.Security;
using Tekus.Application.Auth.Commands;
using Tekus.Application.Auth.Dtos;
using Tekus.Application.Common;
using Tekus.Application.Common.Exceptions;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Common;
using Tekus.Domain.Users;

namespace Tekus.Application.Auth.Handlers;

/// <summary>
/// Authenticates a user and issues an access + refresh token pair.
/// <para>
/// The flow implements the usual protections of a credentials endpoint:
/// </para>
/// <list type="bullet">
///   <item><description>the same generic error is returned for an unknown e-mail and a wrong password, so the endpoint cannot be used to enumerate accounts;</description></item>
///   <item><description>consecutive failures are counted and lock the account for a while (see <see cref="User.RegisterFailedLogin"/>);</description></item>
///   <item><description>inactive accounts cannot sign in.</description></item>
/// </list>
/// </summary>
internal sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthTokensDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<AuthTokensDto> HandleAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.UtcNow();

        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);

        // Unknown / inactive account: fail with the same generic message as a bad password.
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException();
        }

        if (user.IsLockedOut(now))
        {
            throw new UnauthorizedException(
                "The account is temporarily locked after several failed sign-in attempts. Please try again later.",
                ErrorCodes.AccountLocked);
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin(now);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException();
        }

        user.RegisterSuccessfulLogin(now);

        var (plainRefreshToken, refreshToken) =
            AuthTokenFactory.IssueRefreshToken(user, _tokenService, _currentUser, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AuthTokenFactory.Build(_tokenService, plainRefreshToken, user, refreshToken, now);
    }
}
