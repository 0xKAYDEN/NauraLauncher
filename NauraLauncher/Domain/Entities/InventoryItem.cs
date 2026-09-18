using NauraLauncher.Domain.Enums;

namespace NauraLauncher.Domain.Entities;

/// <summary>
/// Conquer Online item - represents weapons, armor, consumables, etc.
/// Based on real Conquer Online item system.
/// </summary>
public class InventoryItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;           // e.g. "Dragon Blade +12"
    public string Description { get; set; } = string.Empty;
    public ItemType Type { get; set; } = ItemType.Weapon;
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public int LevelRequired { get; set; } = 1;
    public ConquerClass? ClassRestriction { get; set; }        // null = any class

    // Conquer specific stats
    public int Plus { get; set; } = 0;                         // +1 to +12 enhancement
    public int SocketCount { get; set; } = 0;                  // 0-2 sockets
    public string SocketGems { get; set; } = string.Empty;     // e.g. "Super Dragon Gem, Refined Phoenix Gem"
    public bool IsBound { get; set; } = false;
    public bool IsLocked { get; set; } = false;
    public int Durability { get; set; } = 100;
    public int Quantity { get; set; } = 1;                     // For stackable items like DragonBalls

    // Visual
    public string IconPath { get; set; } = "Assets/item_dragonball.png";
    public string ImagePath { get; set; } = "Assets/card_trojan.png";

    // Trading
    public bool IsTradable { get; set; } = true;
    public bool IsSellable { get; set; } = true;

    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    public string GetDisplayName()
    {
        var plusStr = Plus > 0 ? $" +{Plus}" : "";
        var boundStr = IsBound ? " (Bound)" : "";
        return $"{Name}{plusStr}{boundStr}";
    }

    public string GetRarityHex() => Rarity.ToHexColor();

    // Property for XAML binding (WPF cannot bind to methods)
    public string RarityHex => Rarity.ToHexColor();

    public string RarityHexColor => Rarity.ToHexColor();
}

public class ConquerItemTemplate
{
    // Static templates for shop/marketplace
    public string Name { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public ItemRarity Rarity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public long BaseCpsPrice { get; set; }
    public long BaseGoldPrice { get; set; }
}
