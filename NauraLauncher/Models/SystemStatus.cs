namespace NauraLauncher.Models;

/// <summary>
/// A pipeline / system status tile shown along the bottom of the hero column.
/// </summary>
public class SystemStatus
{
    public string IconKey { get; set; } = string.Empty;  // resource key in Icons.xaml
    public string Category { get; set; } = string.Empty; // e.g. "DIRECTSTORAGE 2.0"
    public string Detail { get; set; } = string.Empty;   // e.g. "Asset Stream: 6.4 GB/s"
    public string State { get; set; } = string.Empty;    // e.g. "OPTIMIZED"
}
