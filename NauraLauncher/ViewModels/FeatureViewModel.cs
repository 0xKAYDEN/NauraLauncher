using NauraLauncher.Common;

namespace NauraLauncher.ViewModels;

/// <summary>
/// The large "Premiere Spotlight" hero card (PROTOCOL 9: ECLIPSE).
/// </summary>
public class FeatureViewModel : ObservableObject
{
    public string SpotlightLabel => "PREMIERE SPOTLIGHT";
    public string LiveLabel => "LIVE DROP";
    public string ItemIndex => "ITEM 01 / 05";
    public string HeroImage => "Assets/hero_protocol9.png";

    public string Studio => "NEXUS ENTERTAINMENT";
    public string Build => "BUILD 09.4";
    public string RatingText => "99% POSITIVE (14.2K)";

    public string Title => "PROTOCOL 9: ECLIPSE";

    public string Description =>
        "A high-concept tactical espionage odyssey staged across the layered monolithic sectors of " +
        "Neo-Brno. Master systemic electronic warfare, augment cranial memory arrays, and unravel a\u2026";

    public string EditionPrimary => "Standard";
    public string EditionSecondary => "Collector's Rig";

    public string OldPrice => "$79.99";
    public string Price => "$59.99";
    public string Discount => "-25%";

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

    public string PreOrderLabel => IsPreOrdered ? "Pass Reserved" : "Pre-Order Pass";
}
