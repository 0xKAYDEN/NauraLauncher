namespace NauraLauncher.Models;

/// <summary>
/// A single row in the Home page "Latest Intel" news list.
/// </summary>
public class NewsItem
{
    public string Category { get; set; } = string.Empty;   // e.g. "PATCH NOTES"
    public string Title { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;       // e.g. "2H AGO"
    public string ToneHex { get; set; } = "#34D399";       // category accent
    public bool IsLive { get; set; }                       // pulsing live dot
}
