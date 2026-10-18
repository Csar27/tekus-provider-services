using System.Security.Claims;

namespace Tekus.Infrastructure.Security;

/// <summary>
/// Claim names written into the access token and read back by the API's <c>ICurrentUser</c>
/// implementation. They are kept here (instead of in the API) so the writer and the reader can
/// never disagree on a claim name.
/// </summary>
public static class JwtClaimTypes
{
    public const string UserId = ClaimTypes.NameIdentifier;

    public const string Email = ClaimTypes.Email;

    public const string Role = ClaimTypes.Role;

    /// <summary>Full display name, cosmetic claim used by the UI.</summary>
    public const string FullName = "full_name";
}
