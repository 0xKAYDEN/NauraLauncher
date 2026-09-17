using System;
using System.Security.Cryptography;

namespace NauraLauncher.Server.Security;

/// <summary>
/// Production password hashing using PBKDF2-SHA256 with 100,000 iterations
/// and constant-time equality comparison.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 64;
    private const int Iterations = 100_000;

    public static (string Hash, string Salt) HashPassword(string password, string? existingSaltHex = null)
    {
        byte[] salt = string.IsNullOrEmpty(existingSaltHex)
            ? RandomNumberGenerator.GetBytes(SaltSize)
            : Convert.FromHexString(existingSaltHex);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize
        );

        return (Convert.ToHexString(hash).ToLowerInvariant(), Convert.ToHexString(salt).ToLowerInvariant());
    }

    public static bool VerifyPassword(string password, string saltHex, string expectedHashHex)
    {
        var (computedHash, _) = HashPassword(password, saltHex);
        byte[] computedBytes = Convert.FromHexString(computedHash);
        byte[] expectedBytes = Convert.FromHexString(expectedHashHex);

        if (computedBytes.Length != expectedBytes.Length) return false;
        return CryptographicOperations.FixedTimeEquals(computedBytes, expectedBytes);
    }
}
