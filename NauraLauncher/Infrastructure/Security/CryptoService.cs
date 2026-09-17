using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NauraLauncher.Core.Interfaces;

namespace NauraLauncher.Infrastructure.Security;

/// <summary>
/// Production cryptographic security service for credential protection and anti-tamper file verification.
/// </summary>
public class CryptoService : ISecurityService
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("ApexProtocol_MachineEntropy_2026_Secure");

    public string ProtectSecret(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        try
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            // On Windows platforms ProtectedData provides hardware-backed DPAPI encryption
            if (OperatingSystem.IsWindows())
            {
                byte[] encrypted = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }
            else
            {
                // Fallback AES-256 for non-Windows test runners
                using var aes = Aes.Create();
                aes.Key = SHA256.HashData(Entropy);
                aes.GenerateIV();
                using var ms = new MemoryStream();
                ms.Write(aes.IV, 0, aes.IV.Length);
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plainBytes, 0, plainBytes.Length);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CryptoService] ProtectSecret error: {ex.Message}");
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }
    }

    public string UnprotectSecret(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            if (OperatingSystem.IsWindows())
            {
                byte[] decrypted = ProtectedData.Unprotect(cipherBytes, Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            else
            {
                using var aes = Aes.Create();
                aes.Key = SHA256.HashData(Entropy);
                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;
                using var ms = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length);
                using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var reader = new StreamReader(cs, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CryptoService] UnprotectSecret error: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<string> ComputeFileSha256Async(string filePath)
    {
        if (!File.Exists(filePath)) return string.Empty;

        using var sha256 = SHA256.Create();
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        byte[] hashBytes = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task<bool> VerifyFileIntegrityAsync(string filePath, string expectedSha256)
    {
        if (string.IsNullOrWhiteSpace(expectedSha256)) return false;
        string actual = await ComputeFileSha256Async(filePath);
        return string.Equals(actual, expectedSha256, StringComparison.OrdinalIgnoreCase);
    }
}
