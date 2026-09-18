using System.Collections.ObjectModel;
using NauraLauncher.Common;
using NauraLauncher.Domain.Entities;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Shell coordinator - now with Conquer Online theme, auth, wallet, friends, and user panel
/// Clean architecture: owns all sub view models and coordinates auth state
/// </summary>
public class MainViewModel : ObservableObject
{
    public const string HomeNav = "Home";
    public const string MarketplaceNav = "Marketplace";
    public const string AuctionNav = "Auction";
    public const string FriendsNav = "Friends";
    public const string SettingsNav = "Settings";

    public const string ProfileNav = "Profile";
    public const string InventoryNav = "Inventory";

    public MainViewModel()
    {
        NavItems = new ObservableCollection<string> { HomeNav, MarketplaceNav, AuctionNav, FriendsNav, SettingsNav };

        Home = new HomeViewModel();
        Marketplace = new MarketplaceViewModel();
        Auction = new AuctionViewModel();
        Settings = new SettingsViewModel();
        Friends = new FriendsViewModel();
        Auth = new AuthViewModel();
        Profile = new ProfileViewModel();
        Inventory = new InventoryViewModel();

        SelectNavCommand = new RelayCommand(p => { if (p is string s) SelectedNav = s; });
        ShowUserMenuCommand = new RelayCommand(() => IsUserMenuOpen = !IsUserMenuOpen);
        LogoutCommand = new RelayCommand(() => Logout());
        ShowProfileCommand = new RelayCommand(() =>
        {
            IsUserMenuOpen = false;
            SelectedNav = ProfileNav;
        });
        ShowInventoryCommand = new RelayCommand(() =>
        {
            IsUserMenuOpen = false;
            SelectedNav = InventoryNav;
        });
        CloseUserMenuCommand = new RelayCommand(() => IsUserMenuOpen = false);

        // Auth events
        Auth.OnLoginSuccess += (s, user) =>
        {
            CurrentUser = user;
            IsAuthenticated = true;
            IsUserMenuOpen = false;
            SelectedNav = HomeNav;

            // Propagate userId to sub view models
            Marketplace.UserId = user.Id;
            Auction.UserId = user.Id;
            Friends.UserId = user.Id;
            Inventory.UserId = user.Id;
            Profile.User = user;

            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(UserState));
            OnPropertyChanged(nameof(CpsDisplay));
            OnPropertyChanged(nameof(GoldDisplay));
            OnPropertyChanged(nameof(SilverDisplay));
            OnPropertyChanged(nameof(Credits));
            OnPropertyChanged(nameof(AvatarPath));
        };

        // Default to not authenticated - show login
        IsAuthenticated = false;
    }

    // ----- Pages -----
    public HomeViewModel Home { get; }
    public MarketplaceViewModel Marketplace { get; }
    public AuctionViewModel Auction { get; }
    public SettingsViewModel Settings { get; }
    public FriendsViewModel Friends { get; }
    public AuthViewModel Auth { get; }
    public ProfileViewModel Profile { get; }
    public InventoryViewModel Inventory { get; }

    // ----- Nav -----
    public ObservableCollection<string> NavItems { get; }
    public RelayCommand SelectNavCommand { get; }
    public RelayCommand ShowUserMenuCommand { get; }
    public RelayCommand LogoutCommand { get; }
    public RelayCommand ShowProfileCommand { get; }
    public RelayCommand ShowInventoryCommand { get; }
    public RelayCommand CloseUserMenuCommand { get; }

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
            OnPropertyChanged(nameof(IsFriends));
            OnPropertyChanged(nameof(IsSettings));
            OnPropertyChanged(nameof(IsProfile));
            OnPropertyChanged(nameof(IsInventory));
            OnPropertyChanged(nameof(IsAuth));
        }
    }

    public bool IsHome => SelectedNav == HomeNav;
    public bool IsMarketplace => SelectedNav == MarketplaceNav;
    public bool IsAuction => SelectedNav == AuctionNav;
    public bool IsFriends => SelectedNav == FriendsNav;
    public bool IsSettings => SelectedNav == SettingsNav;
    public bool IsProfile => SelectedNav == ProfileNav;
    public bool IsInventory => SelectedNav == InventoryNav;
    public bool IsAuth => !IsAuthenticated;

    // ----- Auth -----
    private bool _isAuthenticated;
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set
        {
            if (SetProperty(ref _isAuthenticated, value))
            {
                OnPropertyChanged(nameof(IsAuth));
                OnPropertyChanged(nameof(IsUserAuthenticated));
            }
        }
    }

    public bool IsUserAuthenticated => IsAuthenticated;

    private User? _currentUser;
    public User? CurrentUser
    {
        get => _currentUser;
        set
        {
            if (SetProperty(ref _currentUser, value))
            {
                OnPropertyChanged(nameof(UserName));
                OnPropertyChanged(nameof(UserState));
                OnPropertyChanged(nameof(CpsDisplay));
                OnPropertyChanged(nameof(GoldDisplay));
                OnPropertyChanged(nameof(SilverDisplay));
                OnPropertyChanged(nameof(Credits));
                OnPropertyChanged(nameof(AvatarPath));
                OnPropertyChanged(nameof(UserLevel));
                OnPropertyChanged(nameof(UserClass));
            }
        }
    }

    private bool _isUserMenuOpen;
    public bool IsUserMenuOpen
    {
        get => _isUserMenuOpen;
        set => SetProperty(ref _isUserMenuOpen, value);
    }

    // ----- Header meta - Conquer Online themed -----
    public string AppVersion => "v2.4.0 - CO 7009";
    public string Ping => "18ms - Eternity";

    // Wallet display - 3 currencies as requested: CPs, Gold, plus original Credits replaced
    public string CpsDisplay => CurrentUser != null ? $"{CurrentUser.Wallet.Cps:N0} CPs" : "0 CPs";
    public string GoldDisplay => CurrentUser != null ? $"{CurrentUser.Wallet.Gold:N0} Gold" : "0 Gold";
    public string SilverDisplay => CurrentUser != null ? $"{CurrentUser.Wallet.Silver:N0} Silver" : "0 Silver";

    public string Credits => CpsDisplay; // Keep for backward compat, but now shows CPs

    public string UserName => CurrentUser?.DisplayName ?? "GUEST";
    public string UserState => CurrentUser != null ? (CurrentUser.IsOnline ? "ONLINE - Twin City" : "OFFLINE") : "NOT LOGGED IN";
    public string UserLevel => CurrentUser != null ? $"Lv.{CurrentUser.Level} {CurrentUser.MainClass}" : "Lv.1";
    public string UserClass => CurrentUser?.MainClass.ToString() ?? "Trojan";
    public string AvatarPath => CurrentUser?.AvatarPath ?? "Assets/avatar_conquer.png";

    // ----- Status bar -----
    public string SystemState => IsAuthenticated ? "TWIN CITY CONNECTED" : "OFFLINE - PLEASE LOGIN";
    public string PatchLabel => "PATCH 7009 - CONQUER ONLINE";
    public double PatchProgress => 1.0;
    public string PatchPercent => "READY";

    private void Logout()
    {
        if (CurrentUser != null)
        {
            CurrentUser.IsOnline = false;
        }
        CurrentUser = null;
        IsAuthenticated = false;
        IsUserMenuOpen = false;
        Auth.Reset();
        SelectedNav = HomeNav;
    }
}
