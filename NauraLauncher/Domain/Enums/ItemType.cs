namespace NauraLauncher.Domain.Enums;

public enum ItemType
{
    Weapon = 0,         // Blade, Sword, Bow, Backsword, etc.
    Armor = 1,          // Armor, Robe
    Headgear = 2,       // Helmet, Coronet, Hat
    Necklace = 3,       // Bag, Necklace
    Ring = 4,
    Boots = 5,
    Garment = 6,        // Fashion
    Mount = 7,          // Steed, Mount Armor
    Consumable = 8,     // DragonBall, Meteor, Exp Ball
    Gem = 9,            // Dragon Gem, Phoenix Gem, etc.
    Material = 10,      // Stones, etc.
    Other = 99
}

public static class ItemTypeExtensions
{
    public static string ToDisplayName(this ItemType type) => type switch
    {
        ItemType.Weapon => "Weapon",
        ItemType.Armor => "Armor",
        ItemType.Headgear => "Headgear",
        ItemType.Necklace => "Necklace/Bag",
        ItemType.Ring => "Ring",
        ItemType.Boots => "Boots",
        ItemType.Garment => "Garment",
        ItemType.Mount => "Mount",
        ItemType.Consumable => "Consumable",
        ItemType.Gem => "Gem",
        ItemType.Material => "Material",
        _ => "Other"
    };
}
