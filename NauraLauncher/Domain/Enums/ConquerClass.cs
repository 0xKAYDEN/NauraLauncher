namespace NauraLauncher.Domain.Enums;

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
    DragonWarrior = 8,
    Windwalker = 9
}

public enum RebornStage
{
    None = 0,
    FirstReborn = 1,
    SecondReborn = 2
}

public static class ConquerClassExtensions
{
    public static string ToDisplayName(this ConquerClass cls) => cls switch
    {
        ConquerClass.Trojan => "Trojan",
        ConquerClass.Warrior => "Warrior",
        ConquerClass.Archer => "Archer",
        ConquerClass.FireTaoist => "Fire Taoist",
        ConquerClass.WaterTaoist => "Water Taoist",
        ConquerClass.Ninja => "Ninja",
        ConquerClass.Monk => "Monk",
        ConquerClass.Pirate => "Pirate",
        ConquerClass.DragonWarrior => "Dragon Warrior",
        ConquerClass.Windwalker => "Windwalker",
        _ => cls.ToString()
    };
}
