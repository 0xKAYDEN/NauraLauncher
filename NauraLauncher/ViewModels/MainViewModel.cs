using System;
using System.Collections.ObjectModel;
using System.Globalization;
using NauraLauncher.Common;
using NauraLauncher.Core.Entities;
using NauraLauncher.Infrastructure.DI;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Production shell coordinator. Owns persistent chrome data (real-time header metrics,
/// live ping from WebSocket, live user credits, downloader patch progress) and coordinates
/// page sub-view models.
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

        // Initialize production backend connections
        ServiceContainer.Initialize();

        // Listen for live latency updates from WebSocket
        ServiceContainer.WebSocket.LatencyUpdated += (s, latency) =>
        {
            _currentPing = $"{Math.Round(latency, 0)}ms";
            OnPropertyChanged(nameof(Ping));
        };

        // Listen for live credit / balance updates
        ServiceContainer.Auth.CreditsChanged += (s, credits) =>
        {
            _currentCredits = "$" + credits.ToString("F2", CultureInfo.InvariantCulture);
            OnPropertyChanged(nameof(Credits));
        };

        // Listen for user profile state changes
        ServiceContainer.Auth.UserChanged += (s, user) =>
        {
            _userName = user.Username;
            _userState = user.Status;
            _currentCredits = "$" + user.Credits.ToString("F2", CultureInfo.InvariantCulture);
            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(UserState));
            OnPropertyChanged(nameof(Credits));
        };

        // Listen for live patching / download progress
        ServiceContainer.Downloader.ProgressChanged += (s, info) =>
        {
            _patchProgress = info.ProgressFraction;
            _patchPercent = info.PercentFormatted;
            _patchLabel = info.StatusMessage;
            if (info.IsCompleted)
            {
                _systemState = "SYSTEM READY";
            }
            else if (info.IsError)
            {
                _systemState = "SYNC WARNING";
            }
            else
            {
                _systemState = "STREAMING ASSETS";
            }
            OnPropertyChanged(nameof(PatchProgress));
            OnPropertyChanged(nameof(PatchPercent));
            OnPropertyChanged(nameof(PatchLabel));
            OnPropertyChanged(nameof(SystemState));
        };
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

    private string _currentPing = "18ms";
    public string Ping => _currentPing;

    private string _currentCredits = "$148.50";
    public string Credits => _currentCredits;

    private string _userName = "VALKYRIE";
    public string UserName => _userName;

    private string _userState = "ONLINE";
    public string UserState => _userState;

    // ----- Status bar -----
    private string _systemState = "SYSTEM READY";
    public string SystemState => _systemState;

    private string _patchLabel = "PATCH 2.4.1";
    public string PatchLabel => _patchLabel;

    private double _patchProgress = 0.75;
    public double PatchProgress => _patchProgress;

    private string _patchPercent = "75%";
    public string PatchPercent => _patchPercent;
}
