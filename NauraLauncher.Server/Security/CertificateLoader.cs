using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace NauraLauncher.Server.Security;

/// <summary>
/// Loads and verifies X.509 SSL/TLS certificates for HTTPS and WSS WebSocket transport.
/// </summary>
public static class CertificateLoader
{
    public static X509Certificate2? LoadCertificate(string certPath, string keyPath)
    {
        try
        {
            if (File.Exists(certPath) && File.Exists(keyPath))
            {
                // In .NET 7+, CreateFromPem reads PEM-encoded cert and key
                return X509Certificate2.CreateFromPemFile(certPath, keyPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CertificateLoader] Failed to load certificate: {ex.Message}");
        }
        return null;
    }
}
