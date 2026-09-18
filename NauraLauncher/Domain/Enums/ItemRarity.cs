namespace NauraLauncher.Domain.Enums;

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

public static class ItemRarityExtensions
{
    public static string ToDisplayName(this ItemRarity rarity) => rarity switch
    {
        ItemRarity.Common => "Common",
        ItemRarity.Rare => "Rare",
        ItemRarity.Elite => "Elite",
        ItemRarity.Super => "Super",
        ItemRarity.Epic => "Epic",
        ItemRarity.Legendary => "Legendary",
        ItemRarity.Mythic => "Mythic",
        _ => rarity.ToString()
    };

    public static string ToHexColor(this ItemRarity rarity) => rarity switch
    {
        ItemRarity.Common => "#8A8F99",
        ItemRarity.Rare => "#60A5FA",
        ItemRarity.Elite => "#34D399",
        ItemRarity.Super => "#A78BFA",
        ItemRarity.Epic => "#C084FC",
        ItemRarity.Legendary => "#F5A524",
        ItemRarity.Mythic => "#F87171",
        _ => "#8A8F99"
    };
}
