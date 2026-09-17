namespace NauraLauncher.Models;

/// <summary>
/// An installed title in the Home page 4-up library grid.
/// </summary>
public class LibraryItem
{
    public string Title { get; set; } = string.Empty;
    public string Studio { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;     // e.g. "READY", "UPDATE 1.2 GB"
    public string Playtime { get; set; } = string.Empty;   // e.g. "62H PLAYED"
    public double Progress { get; set; }                   // 0..1 install / campaign progress
    public bool StatusIsAccent { get; set; }
}
