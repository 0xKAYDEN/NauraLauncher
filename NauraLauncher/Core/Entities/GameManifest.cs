namespace NauraLauncher.Core.Entities;

/// <summary>
/// File manifest record for checksum integrity verification and patching.
/// </summary>
public class GameManifest
{
    public string Id { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string RelativePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Sha256Hash { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public bool IsExecutable { get; set; }
}
