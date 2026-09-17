using System.Collections.ObjectModel;
using NauraLauncher.Common;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Shell coordinator. Owns the persistent chrome data (header meta, status bar)
/// plus the four page view models, and exposes one boolean per nav tab so each
/// page can bind its <c>Visibility</c> directly.
/// </summary>
public class MainViewModel : ObservableObject
{
    public const string HomeNav = "Home";
    public const string MarketplaceNav = "Marketplace";
    public const string AuctionNav = "Auction";
    public const string SettingsNav = "Settings";

    public MainViewModel()
    {
        NavItems = new ObservableCollection<string> { HomeNav, MarketplaceNav, AuctionNav, SettingsNav };

        Home = new HomeViewModel();
        Marketplace = new MarketplaceViewModel();
        Auction = new AuctionViewModel();
        Settings = new SettingsViewModel();

        SelectNavCommand = new RelayCommand(p => { if (p is string s) SelectedNav = s; });
    }

    // ----- Pages -----
    public HomeViewModel Home { get; }
    public MarketplaceViewModel Marketplace { get; }
    public AuctionViewModel Auction { get; }
    public SettingsViewModel Settings { get; }

    // ----- Nav -----
    public ObservableCollection<string> NavItems { get; }
    public RelayCommand SelectNavCommand { get; }

    private string _selectedNav = HomeNav;
    public string SelectedNav
    {
        get => _selectedNav;
        set
        {
            if (!SetProperty(ref _selectedNav, value)) return;
            OnPropertyChanged(nameof(IsHome));
            OnPropertyChanged(nameof(IsMarketplace));
            OnPropertyChanged(nameof(IsAuction));
            OnPropertyChanged(nameof(IsSettings));
        }
    }

    public bool IsHome => SelectedNav == HomeNav;
    public bool IsMarketplace => SelectedNav == MarketplaceNav;
    public bool IsAuction => SelectedNav == AuctionNav;
    public bool IsSettings => SelectedNav == SettingsNav;

    // ----- Header meta -----
    public string AppVersion => "v2.4.0";
    public string Ping => "18ms";
    public string Credits => "$148.50";
    public string UserName => "VALKYRIE";
    public string UserState => "ONLINE";

    // ----- Status bar -----
    public string SystemState => "SYSTEM READY";
    public string PatchLabel => "PATCH 2.4.1";
    public double PatchProgress => 0.75;
    public string PatchPercent => "75%";
}
