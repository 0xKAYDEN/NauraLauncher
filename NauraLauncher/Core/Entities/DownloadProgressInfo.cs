namespace NauraLauncher.Core.Entities;

/// <summary>
/// Status and metrics for active game downloads and patches.
/// </summary>
public class DownloadProgressInfo
{
    public string GameTitle { get; set; } = string.Empty;
    public double ProgressFraction { get; set; }
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public double SpeedBytesPerSecond { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }

    public string SpeedFormatted => SpeedBytesPerSecond switch
    {
        >= 1024 * 1024 * 1024 => $"{SpeedBytesPerSecond / (1024 * 1024 * 1024):F1} GB/s",
        >= 1024 * 1024 => $"{SpeedBytesPerSecond / (1024 * 1024):F1} MB/s",
        >= 1024 => $"{SpeedBytesPerSecond / 1024:F0} KB/s",
        _ => $"{SpeedBytesPerSecond:F0} B/s"
    };

    public string PercentFormatted => $"{ProgressFraction * 100:F0}%";
}
