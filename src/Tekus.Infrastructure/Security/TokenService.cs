using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tekus.Application.Abstractions.Security;
using Tekus.Domain.Users;

namespace Tekus.Infrastructure.Security;

/// <summary>
/// Issues the signed JWT access tokens and the opaque refresh tokens.
/// <para>
/// Access token: short lived, signed with HMAC-SHA256, carries the user id, e-mail, role and full
/// name. Refresh token: a cryptographically random 64-byte value, only its SHA-256 hash is ever
/// persisted (see <see cref="PasswordHasher"/> sibling <c>TokenService.HashRefreshToken"/>).
/// </para>
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options) => _options = options.Value;

    public AccessToken CreateAccessToken(User user, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(user);

        var expiresAtUtc = utcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtClaimTypes.UserId, user.Id.ToString("D")),
            new(JwtClaimTypes.Email, user.Email),
            new(JwtClaimTypes.Role, user.Role.ToString()),
            new(JwtClaimTypes.FullName, user.FullName),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: utcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }

    /// <summary>Base64 url-safe string of 64 random bytes (512 bits of entropy).</summary>
    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    /// <summary>SHA-256 of the token, base64 encoded. Only this value reaches the database.</summary>
    public string HashRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken.Trim()));

        return Convert.ToBase64String(digest);
    }
}
