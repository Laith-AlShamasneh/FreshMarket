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

    /// <summary>
    /// Hashes a plain-text password securely using PBKDF2.
    /// Returns a string containing both salt and hash for storage in the database.
    /// </summary>
    public static string HashPassword(string password)
    {
        Guard.AgainstNullOrWhiteSpace(password, nameof(password));

        using var salt = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256);
        var hash = salt.GetBytes(HashSize);
        var saltBytes = salt.Salt;

        // Combine salt + hash for storage
        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(saltBytes, 0, hashBytes, 0, SaltSize);
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

            // Extract salt from stored hash
            var saltBytes = new byte[SaltSize];
            Array.Copy(hashBytes, 0, saltBytes, 0, SaltSize);

            // Hash the input password with the extracted salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            // Compare the computed hash with the stored hash
            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
