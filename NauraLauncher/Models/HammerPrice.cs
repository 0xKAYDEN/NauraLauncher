namespace NauraLauncher.Models;

/// <summary>
/// A closed sale in the "Recent Hammer Prices" feed.
/// </summary>
public class HammerPrice
{
    public string Item { get; set; } = string.Empty;       // e.g. "OBSIDIAN KATANA"
    public string Serial { get; set; } = string.Empty;     // e.g. "#0017"
    public string Price { get; set; } = string.Empty;      // e.g. "$48,200"
    public string Delta { get; set; } = string.Empty;      // e.g. "+12.4%"
    public bool DeltaIsPositive { get; set; } = true;
    public string When { get; set; } = string.Empty;       // e.g. "6M AGO"
    public string ToneHex { get; set; } = "#8A8F99";       // rarity tone
}
