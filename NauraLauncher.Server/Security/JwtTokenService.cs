using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NauraLauncher.Server.Security;

/// <summary>
/// Cryptographic JWT token service using HMAC-SHA256 for secure session authentication.
/// </summary>
public class JwtTokenService
{
    private readonly byte[] _key;

    public JwtTokenService(string secret = "apex-protocol-super-secure-jwt-secret-key-2026-x99!")
    {
        _key = Encoding.UTF8.GetBytes(secret);
    }

    public string CreateToken(string userId, string username, string role = "OPERATOR", int expiresInSeconds = 86400)
    {
        var header = new { alg = "HS256", typ = "JWT" };
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long exp = now + expiresInSeconds;
        var payload = new
        {
            sub = userId,
            name = username,
            role,
            iat = now,
            exp
        };

        string headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header)));
        string payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
        string signatureBase64 = CreateHmacSignature($"{headerBase64}.{payloadBase64}");

        return $"{headerBase64}.{payloadBase64}.{signatureBase64}";
    }

    public (bool IsValid, string? UserId, string? Username) ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return (false, null, null);
        var parts = token.Split('.');
        if (parts.Length != 3) return (false, null, null);

        string expectedSig = CreateHmacSignature($"{parts[0]}.{parts[1]}");
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedSig);
        byte[] actualBytes = Encoding.UTF8.GetBytes(parts[2]);

        if (expectedBytes.Length != actualBytes.Length || !CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes))
        {
            return (false, null, null);
        }

        try
        {
            string payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("exp", out var expProp))
            {
                long exp = expProp.GetInt64();
                if (exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    return (false, null, null); // Expired
                }
            }

            string? userId = root.TryGetProperty("sub", out var subProp) ? subProp.GetString() : null;
            string? username = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;

            return (true, userId, username);
        }
        catch
        {
            return (false, null, null);
        }
    }

    private string CreateHmacSignature(string data)
    {
        using var hmac = new HMACSHA256(_key);
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static byte[] Base64UrlDecode(string input)
    {
        string padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}
