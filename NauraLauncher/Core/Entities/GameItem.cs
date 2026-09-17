namespace NauraLauncher.Core.Entities;

/// <summary>
/// Domain model for game entries in the catalog or library.
/// </summary>
public class GameItem
{
    public string Id { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Studio { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Tactical RPG";
    public string ReleaseTag { get; set; } = "NEW RELEASE";
    public string Ribbon { get; set; } = "96% POSITIVE";
    public bool RibbonIsAccent { get; set; } = true;
    public double Price { get; set; }
    public int DiscountPct { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string HeroImagePath { get; set; } = string.Empty;
    public string ExecutableName { get; set; } = "game.exe";
    public string CurrentVersion { get; set; } = "1.0.0";
    public long InstallSizeBytes { get; set; } = 1073741824;
    public bool IsFeatured { get; set; }

    // Library specific fields
    public string? LicenseKey { get; set; }
    public string Status { get; set; } = "READY";
    public long PlaytimeSeconds { get; set; }
    public double CampaignProgress { get; set; }
    public string? InstalledPath { get; set; }
    public bool IsOwned { get; set; }
}
