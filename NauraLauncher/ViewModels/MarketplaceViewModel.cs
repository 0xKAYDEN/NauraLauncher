using System.Collections.ObjectModel;
using NauraLauncher.Application.Interfaces;
using NauraLauncher.Application.Services;
using NauraLauncher.Common;
using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;
using NauraLauncher.Models;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Marketplace page: Conquer Online trade market - browse, my listings, add from inventory
/// </summary>
public class MarketplaceViewModel : ObservableObject
{
    private readonly IMarketplaceService _marketplaceService;
    private readonly IInventoryService _inventoryService;
    private readonly IWalletService _walletService;

    public MarketplaceViewModel() : this(new MarketplaceService(), new InventoryService(), new WalletService()) { }

    public MarketplaceViewModel(IMarketplaceService marketplaceService, IInventoryService inventoryService, IWalletService walletService)
    {
        _marketplaceService = marketplaceService;
        _inventoryService = inventoryService;
        _walletService = walletService;

        // ----- Category filter chips -----
        Categories = new ObservableCollection<CategoryTab>
        {
            new() { Name = "All Items", IsSelected = true },
            new() { Name = "Weapon" },
            new() { Name = "Armor" },
            new() { Name = "Garment" },
            new() { Name = "Gem" },
            new() { Name = "Consumable" },
            new() { Name = "Mount" },
        };

        SelectCategoryCommand = new RelayCommand(p =>
        {
            if (p is not CategoryTab tab) return;
            foreach (var c in Categories) c.IsSelected = ReferenceEquals(c, tab);
            SelectedCategory = tab.Name;
            _ = LoadMarketplaceAsync();
        });

        // ----- Tradable inventory for listing -----
        TradableInventory = new ObservableCollection<InventoryItem>();
        MyListings = new ObservableCollection<MarketplaceListing>();
        MarketplaceItems = new ObservableCollection<MarketplaceListing>();

        // Feature (hero) spotlight - Conquer Online themed
        Feature = new FeatureViewModel();

        PreOrderCommand = new RelayCommand(() => Feature.IsPreOrdered = !Feature.IsPreOrdered);

        // Marketplace actions
        BuyCommand = new RelayCommand(async p =>
        {
            if (p is MarketplaceListing listing)
                await BuyItemAsync(listing);
        });

        CreateListingCommand = new RelayCommand(async () => await CreateListingAsync(), () => SelectedInventoryItem != null && ListingPrice > 0);
        CancelListingCommand = new RelayCommand(async p =>
        {
            if (p is MarketplaceListing listing)
                await CancelListingAsync(listing);
        });

        SelectInventoryItemCommand = new RelayCommand(p =>
        {
            if (p is InventoryItem item)
                SelectedInventoryItem = item;
        });

        SwitchTabCommand = new RelayCommand(p =>
        {
            if (p is string tab)
                ActiveTab = tab;
        });

        SearchCommand = new RelayCommand(async () => await SearchAsync());

        _ = LoadMarketplaceAsync();
    }

    private int _userId;
    public int UserId
    {
        get => _userId;
        set
        {
            if (SetProperty(ref _userId, value))
            {
                _ = LoadMarketplaceAsync();
                _ = LoadMyListingsAsync();
                _ = LoadTradableInventoryAsync();
            }
        }
    }

    public ObservableCollection<CategoryTab> Categories { get; }
    public ObservableCollection<MarketplaceListing> MarketplaceItems { get; }
    public ObservableCollection<MarketplaceListing> MyListings { get; }
    public ObservableCollection<InventoryItem> TradableInventory { get; }

    public ObservableCollection<SystemStatus> StatusTiles { get; } = new()
    {
        new() { IconKey = "Icon.Gold", Category = "MARKET VOLUME", Detail = "12.4M Gold / 24h", State = "HOT" },
        new() { IconKey = "Icon.Gem", Category = "CPs TRADE", Detail = "850K CPs circulated", State = "ACTIVE" },
        new() { IconKey = "Icon.Users", Category = "TRADERS ONLINE", Detail = "1,248 players trading", State = "LIVE" },
    };

    public FeatureViewModel Feature { get; }

    private string _selectedCategory = "All Items";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    private string _activeTab = "Browse";
    public string ActiveTab
    {
        get => _activeTab;
        set
        {
            if (SetProperty(ref _activeTab, value))
            {
                OnPropertyChanged(nameof(IsBrowseTab));
                OnPropertyChanged(nameof(IsMyListingsTab));
                OnPropertyChanged(nameof(IsSellTab));
            }
        }
    }

    public bool IsBrowseTab => ActiveTab == "Browse";
    public bool IsMyListingsTab => ActiveTab == "MyListings";
    public bool IsSellTab => ActiveTab == "Sell";

    private InventoryItem? _selectedInventoryItem;
    public InventoryItem? SelectedInventoryItem
    {
        get => _selectedInventoryItem;
        set => SetProperty(ref _selectedInventoryItem, value);
    }

    private long _listingPrice = 100;
    public long ListingPrice
    {
        get => _listingPrice;
        set => SetProperty(ref _listingPrice, value);
    }

    private CurrencyType _listingCurrency = CurrencyType.Cps;
    public CurrencyType ListingCurrency
    {
        get => _listingCurrency;
        set => SetProperty(ref _listingCurrency, value);
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string SortLabel => "Most Recent";
    public string ArchivesShowing => $"SHOWING {MarketplaceItems.Count} ITEMS";

    public RelayCommand SelectCategoryCommand { get; }
    public RelayCommand PreOrderCommand { get; }
    public RelayCommand BuyCommand { get; }
    public RelayCommand CreateListingCommand { get; }
    public RelayCommand CancelListingCommand { get; }
    public RelayCommand SelectInventoryItemCommand { get; }
    public RelayCommand SwitchTabCommand { get; }
    public RelayCommand SearchCommand { get; }

    public async Task LoadMarketplaceAsync()
    {
        IsLoading = true;
        try
        {
            ItemType? filterType = null;
            if (SelectedCategory != "All Items" && Enum.TryParse<ItemType>(SelectedCategory, out var type))
                filterType = type;

            var listings = await _marketplaceService.GetActiveListingsAsync(0, 20, filterType);
            MarketplaceItems.Clear();
            foreach (var l in listings)
                MarketplaceItems.Add(l);

            OnPropertyChanged(nameof(ArchivesShowing));
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadMyListingsAsync()
    {
        if (UserId == 0) return;
        try
        {
            var listings = await _marketplaceService.GetUserListingsAsync(UserId);
            MyListings.Clear();
            foreach (var l in listings)
                MyListings.Add(l);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    public async Task LoadTradableInventoryAsync()
    {
        if (UserId == 0) return;
        try
        {
            var items = await _inventoryService.GetTradableItemsAsync(UserId);
            TradableInventory.Clear();
            foreach (var item in items)
                TradableInventory.Add(item);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    public async Task BuyItemAsync(MarketplaceListing listing)
    {
        if (UserId == 0) return;
        IsLoading = true;
        try
        {
            var tx = await _marketplaceService.BuyItemAsync(UserId, listing.Id);
            StatusMessage = $"Purchased {listing.Title} for {listing.Price} {listing.Currency.ToDisplayName()}!";
            MarketplaceItems.Remove(listing);
            await LoadTradableInventoryAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task CreateListingAsync()
    {
        if (UserId == 0 || SelectedInventoryItem == null) return;
        if (ListingPrice <= 0)
        {
            StatusMessage = "Price must be greater than 0";
            return;
        }

        IsLoading = true;
        try
        {
            var listing = await _marketplaceService.CreateListingAsync(UserId, SelectedInventoryItem.Id, ListingPrice, ListingCurrency);
            StatusMessage = $"Listed {listing.Title} for {ListingPrice} {ListingCurrency.ToDisplayName()}";
            MyListings.Insert(0, listing);
            TradableInventory.Remove(SelectedInventoryItem);
            SelectedInventoryItem = null;
            ActiveTab = "MyListings";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task CancelListingAsync(MarketplaceListing listing)
    {
        if (UserId == 0) return;
        IsLoading = true;
        try
        {
            var success = await _marketplaceService.CancelListingAsync(UserId, listing.Id);
            if (success)
            {
                MyListings.Remove(listing);
                StatusMessage = $"Cancelled listing for {listing.Title}";
                await LoadTradableInventoryAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadMarketplaceAsync();
            return;
        }

        IsLoading = true;
        try
        {
            var results = await _marketplaceService.SearchAsync(SearchQuery);
            MarketplaceItems.Clear();
            foreach (var r in results)
                MarketplaceItems.Add(r);
            OnPropertyChanged(nameof(ArchivesShowing));
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
