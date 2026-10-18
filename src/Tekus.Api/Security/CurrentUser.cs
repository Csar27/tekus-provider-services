using System.Security.Claims;
using Tekus.Application.Abstractions.Security;
using Tekus.Domain.Users;
using Tekus.Infrastructure.Security;

namespace Tekus.Api.Security;

/// <summary>
/// Ambient information about the caller, extracted from the JWT claims of the current HTTP
/// request. It is the only bridge between the transport (HTTP) and the application layer, which
/// never reads <c>HttpContext</c> directly and therefore stays unit-testable.
/// </summary>
internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
        => Guid.TryParse(Principal?.FindFirstValue(JwtClaimTypes.UserId), out var id) ? id : null;

    public string? Email => Principal?.FindFirstValue(JwtClaimTypes.Email);

    public UserRole? Role
        => Enum.TryParse<UserRole>(Principal?.FindFirstValue(JwtClaimTypes.Role), out var role) ? role : null;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    /// <summary>Remote address of the caller, stored alongside refresh tokens for auditing.</summary>
    public string? IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;

            return context?.Connection.RemoteIpAddress?.ToString();
        }
    }

    public bool IsInRole(UserRole role) => Role == role;
}
