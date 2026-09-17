namespace NauraLauncher.Models;

/// <summary>
/// A single entry shown in the "Curated Archives" grid.
/// </summary>
public class GameEntry
{
    public string Title { get; set; } = string.Empty;
    public string Studio { get; set; } = string.Empty;
    public string ReleaseTag { get; set; } = string.Empty;   // e.g. "NOV 2024", "NEW RELEASE"
    public string Ribbon { get; set; } = string.Empty;       // e.g. "96% POSITIVE", "OVERWHELMING"
    public string Price { get; set; } = string.Empty;        // e.g. "$34.00"
    public string? Discount { get; set; }                    // e.g. "-15%"
    public string ImagePath { get; set; } = string.Empty;    // pack/relative resource path
    public bool RibbonIsAccent { get; set; }                 // green ribbon vs neutral
}
