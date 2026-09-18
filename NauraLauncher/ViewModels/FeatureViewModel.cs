using NauraLauncher.Common;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Conquer Online spotlight - featured item in marketplace
/// </summary>
public class FeatureViewModel : ObservableObject
{
    public string SpotlightLabel => "FEATURED ITEM";
    public string LiveLabel => "HOT TRADE";
    public string ItemIndex => "ITEM 01 / 20";
    public string HeroImage => "Assets/hero_conquer.png";

    public string Studio => "TWIN CITY MARKET";
    public string Build => "SUPER 2-SOCKET";
    public string RatingText => "LEGENDARY · +12 ENHANCED";

    public string Title => "DRAGON BLADE +12 - SUPER 2-SOCKET";

    public string Description =>
        "Legendary Trojan blade forged in the depths of Twin City. Super 2-socket with Super Dragon Gem and Super Phoenix Gem. " +
        "Unbound and ready for trade. The ultimate weapon for 2nd Reborn Trojans seeking dominance in Guild War and PK arena. " +
        "Certified authentic by Market Conductor.";

    public string EditionPrimary => "CPs Trade";
    public string EditionSecondary => "Gold Trade";

    public string OldPrice => "75,000 CPs";
    public string Price => "52,400 CPs";
    public string Discount => "-30%";

    private bool _isPreOrdered;
    public bool IsPreOrdered
    {
        get => _isPreOrdered;
        set
        {
            if (SetProperty(ref _isPreOrdered, value))
                OnPropertyChanged(nameof(PreOrderLabel));
        }
    }

    public string PreOrderLabel => IsPreOrdered ? "In Watchlist" : "Add to Watchlist";
}
