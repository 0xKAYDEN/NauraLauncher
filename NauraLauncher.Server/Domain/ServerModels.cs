namespace NauraLauncher.Server.Domain;

/// <summary>
/// Server-side domain models - mirrors client domain but with persistence concerns
/// Clean architecture: Domain layer, no dependencies
/// </summary>

public enum CurrencyType
{
    Cps = 0,
    Gold = 1,
    Silver = 2,
    BoundCps = 3
}

public enum ConquerClass
{
    Trojan = 0,
    Warrior = 1,
    Archer = 2,
    FireTaoist = 3,
    WaterTaoist = 4,
    Ninja = 5,
    Monk = 6,
    Pirate = 7,
    DragonWarrior = 8
}

public enum ItemType
{
    Weapon = 0,
    Armor = 1,
    Headgear = 2,
    Necklace = 3,
    Ring = 4,
    Boots = 5,
    Garment = 6,
    Mount = 7,
    Consumable = 8,
    Gem = 9,
    Material = 10,
    Other = 99
}

public enum ItemRarity
{
    Common = 0,
    Rare = 1,
    Elite = 2,
    Super = 3,
    Epic = 4,
    Legendary = 5,
    Mythic = 6
}

// Detailed entity models for server persistence
public class ServerUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ConquerClass MainClass { get; set; }
    public int Level { get; set; }
    public int RebornStage { get; set; }
    public bool IsOnline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
}

public class ServerWallet
{
    public int UserId { get; set; }
    public long Cps { get; set; }
    public long BoundCps { get; set; }
    public long Gold { get; set; }
    public long Silver { get; set; }
}
