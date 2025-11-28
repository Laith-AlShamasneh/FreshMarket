using System.Security.Cryptography;

namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Password hashing and verification helper using PBKDF2 algorithm.
/// Provides secure password storage and validation for authentication workflows.
/// </summary>
public static class PasswordHelper
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Hashes a plain-text password securely using PBKDF2.
    /// Returns a string containing both salt and hash for storage in the database.
    /// </summary>
    public static string HashPassword(string password)
    {
        Guard.AgainstNullOrWhiteSpace(password, nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            _algorithm,
            HashSize);

        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a plain-text password against a stored hash.
    /// Returns true if the password matches the hash; false otherwise.
    /// </summary>
    public static bool VerifyPassword(string password, string storedHash)
    {
        Guard.AgainstNullOrWhiteSpace(password, nameof(password));
        Guard.AgainstNullOrWhiteSpace(storedHash, nameof(storedHash));

        try
        {
            var hashBytes = Convert.FromBase64String(storedHash);

            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            var saltBytes = new byte[SaltSize];
            Array.Copy(hashBytes, 0, saltBytes, 0, SaltSize);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                _algorithm,
                HashSize);

            return CryptographicOperations.FixedTimeEquals(
                hash,
                hashBytes.AsSpan(SaltSize));
        }
        catch
        {
            return false;
        }
    }
}