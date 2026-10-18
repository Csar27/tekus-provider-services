using System.Security.Cryptography;
using System.Text;
using Tekus.Application.Abstractions.Security;

namespace Tekus.Infrastructure.Security;

/// <summary>
/// PBKDF2-HMAC-SHA256 password hashing with a random 16-byte salt and a per-hash iteration count.
/// <para>
/// The stored format is <c>PBKDF2-SHA256${iterations}${saltBase64}${hashBase64}</c>, which is
/// self describing: raising the iteration count in a later release only requires writing new hashes
/// on the next successful login, old hashes keep verifying with the iteration count they carry.
/// </para>
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const string Algorithm = "PBKDF2-SHA256";
    private const int DefaultIterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    /// <summary>Hard ceiling so a corrupted row cannot turn a verify into a denial of service.</summary>
    private const int MaxIterations = 10_000_000;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            DefaultIterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return string.Join('$', Algorithm, DefaultIterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split('$');

        if (parts.Length != 4 || !string.Equals(parts[0], Algorithm, StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations) || iterations < 1 || iterations > MaxIterations)
        {
            return false;
        }

        byte[] salt;
        byte[] expected;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            // A malformed hash must never crash a sign-in attempt.
            return false;
        }

        if (salt.Length == 0 || expected.Length == 0)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);

        // Constant-time comparison: the hash length is data driven, so a plain == would leak the
        // number of matching bytes through the timing of the response.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
