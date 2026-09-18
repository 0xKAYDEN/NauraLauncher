using NauraLauncher.Domain.Enums;

namespace NauraLauncher.Domain.Entities;

/// <summary>
/// Core user entity - Conquer Online account
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ConquerClass MainClass { get; set; } = ConquerClass.Trojan;
    public int Level { get; set; } = 1;
    public RebornStage Reborn { get; set; } = RebornStage.None;
    public string AvatarPath { get; set; } = "Assets/avatar_conquer.png";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    public bool IsOnline { get; set; }
    public string StatusMessage { get; set; } = "Online in Twin City";

    // Navigation
    public Wallet Wallet { get; set; } = new();
    public List<InventoryItem> Inventory { get; set; } = new();
}

public class Wallet
{
    public int UserId { get; set; }
    public long Cps { get; set; } = 0;           // Conquer Points
    public long BoundCps { get; set; } = 0;      // Bound CPs
    public long Gold { get; set; } = 0;          // Gold
    public long Silver { get; set; } = 0;        // Silver (optional, for completeness)

    public long GetBalance(CurrencyType type) => type switch
    {
        CurrencyType.Cps => Cps,
        CurrencyType.BoundCps => BoundCps,
        CurrencyType.Gold => Gold,
        CurrencyType.Silver => Silver,
        _ => 0
    };

    public void Add(CurrencyType type, long amount)
    {
        switch (type)
        {
            case CurrencyType.Cps: Cps += amount; break;
            case CurrencyType.BoundCps: BoundCps += amount; break;
            case CurrencyType.Gold: Gold += amount; break;
            case CurrencyType.Silver: Silver += amount; break;
        }
    }

    public bool TryDeduct(CurrencyType type, long amount)
    {
        var current = GetBalance(type);
        if (current < amount) return false;
        Add(type, -amount);
        return true;
    }
}
