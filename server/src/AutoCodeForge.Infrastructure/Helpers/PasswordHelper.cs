using System.Security.Cryptography;

namespace AutoCodeForge.Infrastructure.Helpers;

/// <summary>
/// Provides password hashing and verification functions.
/// </summary>
public class PasswordHelper
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100000;

    /// <summary>
    /// Hashes plain password with PBKDF2.
    /// </summary>
    /// <param name="password">The plain password.</param>
    /// <returns>The encoded hash string.</returns>
    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies plain password against hash.
    /// </summary>
    /// <param name="password">The plain password.</param>
    /// <param name="passwordHash">The saved hash.</param>
    /// <returns>True when valid.</returns>
    public bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
