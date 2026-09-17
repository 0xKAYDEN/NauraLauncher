using System.Threading.Tasks;

namespace NauraLauncher.Core.Interfaces;

public interface ISecurityService
{
    string ProtectSecret(string plainText);
    string UnprotectSecret(string cipherText);
    Task<string> ComputeFileSha256Async(string filePath);
    Task<bool> VerifyFileIntegrityAsync(string filePath, string expectedSha256);
}
