namespace NauraLauncher.Domain.Enums;

public enum CurrencyType
{
    Cps = 0,        // Conquer Points - premium currency
    Gold = 1,       // Gold - trade currency
    Silver = 2,     // Silver - basic currency (optional)
    BoundCps = 3    // Bound CPs - non-tradable
}

public static class CurrencyTypeExtensions
{
    public static string ToDisplayName(this CurrencyType type) => type switch
    {
        CurrencyType.Cps => "CPs",
        CurrencyType.Gold => "Gold",
        CurrencyType.Silver => "Silver",
        CurrencyType.BoundCps => "Bound CPs",
        _ => type.ToString()
    };

    public static string ToIconKey(this CurrencyType type) => type switch
    {
        CurrencyType.Cps => "Icon.Gem",
        CurrencyType.Gold => "Icon.Gold",
        CurrencyType.Silver => "Icon.Silver",
        CurrencyType.BoundCps => "Icon.Gem",
        _ => "Icon.Coin"
    };
}
