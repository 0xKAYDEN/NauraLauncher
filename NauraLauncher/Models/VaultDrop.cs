namespace NauraLauncher.Models;

/// <summary>
/// A tile in the "Active Vault Drops" carousel.
/// </summary>
public class VaultDrop
{
    public string Name { get; set; } = string.Empty;
    public string Edition { get; set; } = string.Empty;    // e.g. "SERIAL EDITION · 1 OF 120"
    public string Rarity { get; set; } = string.Empty;     // e.g. "MYTHIC"
    public string ToneHex { get; set; } = "#8A8F99";
    public string DropWindow { get; set; } = string.Empty; // e.g. "DROPS IN 04:12:09"
    public string ImagePath { get; set; } = string.Empty;
    public double Progress { get; set; }                   // 0..1 claimed / allocated
}
