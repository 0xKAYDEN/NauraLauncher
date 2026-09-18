using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Threading;
using NauraLauncher.Application.Interfaces;
using NauraLauncher.Application.Services;
using NauraLauncher.Common;
using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;
using NauraLauncher.Models;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Auction page: Conquer Online auction house - Dragon Balls, Gems, Super items
/// Supports selling from inventory and bidding/buyout flow
/// </summary>
public class AuctionViewModel : ObservableObject
{
    private readonly IAuctionService _auctionService;
    private readonly IInventoryService _inventoryService;
    private readonly DispatcherTimer _countdownTimer;
    private TimeSpan _remaining = new(0, 2, 41, 17);

    private const double BuyoutValue = 78_000d;
    private double _currentBidValue = 52_400d;
    private int _bidCount = 137;

    public AuctionViewModel() : this(new AuctionService(), new InventoryService()) { }

    public AuctionViewModel(IAuctionService auctionService, IInventoryService inventoryService)
    {
        _auctionService = auctionService;
        _inventoryService = inventoryService;

        RarityFilters = new ObservableCollection<FilterChip>
        {
            new() { Name = "ALL LOTS",   Count = "148", ToneHex = "#F4F5F7", IsSelected = true },
            new() { Name = "COMMON",     Count = "62",  ToneHex = "#8A8F99" },
            new() { Name = "RARE",       Count = "38",  ToneHex = "#60A5FA" },
            new() { Name = "ELITE",     Count = "24",  ToneHex = "#34D399" },
            new() { Name = "SUPER",       Count = "15",  ToneHex = "#A78BFA" },
            new() { Name = "EPIC",     Count = "06",  ToneHex = "#C084FC" },
            new() { Name = "LEGENDARY",   Count = "03",  ToneHex = "#F5A524" },
        };

        SelectRarityCommand = new RelayCommand(p =>
        {
            if (p is not FilterChip chip) return;
            foreach (var c in RarityFilters) c.IsSelected = ReferenceEquals(c, chip);
            SelectedRarityName = chip.Name;
            _ = LoadAuctionsAsync();
        });

        Increments = new ObservableCollection<BidIncrement>
        {
            new() { Label = "+100", Value = 100 },
            new() { Label = "+250", Value = 250 },
            new() { Label = "+500", Value = 500, IsSelected = true },
            new() { Label = "+1K",  Value = 1_000 },
        };

        SelectIncrementCommand = new RelayCommand(p =>
        {
            if (p is not BidIncrement inc) return;
            foreach (var i in Increments) i.IsSelected = ReferenceEquals(i, inc);
            OnPropertyChanged(nameof(SelectedIncrementValue));
            OnPropertyChanged(nameof(CurrentBidMeta));
            OnPropertyChanged(nameof(PlaceBidLabel));
        });

        Attributes = new ObservableCollection<AttributeRow>
        {
            new() { Label = "ORIGIN",          Value = "TWIN CITY MARKET (178,182)" },
            new() { Label = "FORGE DATE",      Value = "14 · 09 · 2024", MonoValue = "14·09·2024" },
            new() { Label = "ENHANCEMENT",           Value = "+12 SUPER 2-SOCKET" },
            new() { Label = "SOCKETS",   Value = "SUPER DRAGON GEM / SUPER PHOENIX GEM" },
            new() { Label = "DURABILITY",            Value = "100 / 100" },
            new() { Label = "OWNER HISTORY",   Value = "3 REGISTERED HEROES" },
            new() { Label = "CERTIFICATION",   Value = "CO-9F2C-0042", MonoValue = "CO-9F2C-0042", IsAccent = true },
            new() { Label = "TRADE STATUS",    Value = "UNBOUND · TRADABLE", IsAccent = true },
        };

        HammerPrices = new ObservableCollection<HammerPrice>
        {
            new() { Item = "DRAGON BLADE +12", Serial = "#0017", Price = "48,200 CPs", Delta = "+12.4%", DeltaIsPositive = true,  When = "6M AGO",  ToneHex = "#34D399" },
            new() { Item = "SUPER DRAGON GEM",   Serial = "#0231", Price = "21,900 CPs", Delta = "+4.1%",  DeltaIsPositive = true,  When = "22M AGO", ToneHex = "#F87171" },
            new() { Item = "DRAGON BALL x10",      Serial = "#0008", Price = "6,750 Gold", Delta = "+18.9%", DeltaIsPositive = true,  When = "48M AGO", ToneHex = "#C084FC" },
            new() { Item = "HEAVEN FAN +9",     Serial = "#0444", Price = "7,400 CPs",  Delta = "-2.6%",  DeltaIsPositive = false, When = "1H AGO",  ToneHex = "#8A8F99" },
            new() { Item = "NINJA KATANA +8",    Serial = "#0102", Price = "15,150 Gold", Delta = "+0.8%",  DeltaIsPositive = true,  When = "2H AGO",  ToneHex = "#F5A524" },
        };

        VaultDrops = new ObservableCollection<VaultDrop>
        {
            new()
            {
                Name = "SUPER DRAGON GEM", Edition = "SUPER GEM · 1 OF 50", Rarity = "MYTHIC",
                ToneHex = "#F87171", DropWindow = "DROPS IN 04:12:09", Progress = 0.72,
                ImagePath = "Assets/card_trojan.png",
            },
            new()
            {
                Name = "DRAGON BALL PACK", Edition = "27 DBs · LOTTERY READY", Rarity = "LEGENDARY",
                ToneHex = "#C084FC", DropWindow = "DROPS IN 11:40:55", Progress = 0.35,
                ImagePath = "Assets/item_dragonball.png",
            },
            new()
            {
                Name = "HEAVEN FAN +12", Edition = "WATER TAOIST · SUPER", Rarity = "RARE",
                ToneHex = "#60A5FA", DropWindow = "DROPS IN 1D 02:15", Progress = 0.91,
                ImagePath = "Assets/card_taoist.png",
            },
            new()
            {
                Name = "TROJAN ARMOR +12", Edition = "SUPER ARMOR · 2 SOCKET", Rarity = "OBSIDIAN",
                ToneHex = "#34D399", DropWindow = "DROPS IN 2D 06:30", Progress = 0.18,
                ImagePath = "Assets/card_warrior.png",
            },
        };

        // Conquer Online themed lots
        AuctionLots = new ObservableCollection<AuctionLot>();
        MyLots = new ObservableCollection<AuctionLot>();
        MyBids = new ObservableCollection<AuctionLot>();
        TradableInventory = new ObservableCollection<InventoryItem>();

        // ----- Live lot (demo) -----
        LotName = "DRAGON BLADE +12";
        LotSerial = "SUPER 2-SOCKET";
        LotRarity = "SUPER";
        LotToneHex = "#A78BFA";
        LotImagePath = "Assets/card_trojan.png";
        LotCertifiedLabel = "CERTIFIED";
        LotCertifiedId = "CO-9F2C-0042";
        LotProvenance = "FORGED IN TWIN CITY · 3 OWNERS · UNBOUND";
        LotFloor = "FLOOR 38,000 CPs";
        TopBidder = "DragonLord";
        TopBidderMeta = "LEVEL 130 · 2ND REBORN TROJAN";

        PlaceBidCommand = new RelayCommand(async () => await PlaceBidOnSelectedAsync());
        WatchCommand = new RelayCommand(() => IsWatching = !IsWatching);

        BuyoutCommand = new RelayCommand(async () => await BuyoutSelectedAsync());
        CreateAuctionCommand = new RelayCommand(async () => await CreateAuctionAsync());
        CancelAuctionCommand = new RelayCommand(async p => { if (p is AuctionLot lot) await CancelAuctionAsync(lot); });

        SelectLotCommand = new RelayCommand(p =>
        {
            if (p is AuctionLot lot)
            {
                SelectedLot = lot;
                LotName = lot.Item?.Name ?? lot.Title;
                LotRarity = lot.Item?.Rarity.ToDisplayName() ?? "RARE";
                LotToneHex = lot.Item?.GetRarityHex() ?? "#8A8F99";
                LotImagePath = lot.Item?.ImagePath ?? "Assets/card_trojan.png";
                _currentBidValue = lot.CurrentBid;
                _bidCount = lot.BidCount;
                TopBidder = lot.CurrentBidderName ?? "No bids";
                OnPropertyChanged(nameof(CurrentBid));
                OnPropertyChanged(nameof(PlaceBidLabel));
                OnPropertyChanged(nameof(BidProgress));
            }
        });

        SelectInventoryForAuctionCommand = new RelayCommand(p =>
        {
            if (p is InventoryItem item)
                SelectedInventoryItem = item;
        });

        SwitchTabCommand = new RelayCommand(p =>
        {
            if (p is string tab)
                ActiveTab = tab;
        });

        // Countdown ticks once per second
        _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _countdownTimer.Tick += (_, _) =>
        {
            if (_remaining <= TimeSpan.Zero) return;
            _remaining = _remaining.Subtract(TimeSpan.FromSeconds(1));
            OnPropertyChanged(nameof(Countdown));
        };
        _countdownTimer.Start();

        _ = LoadAuctionsAsync();
    }

    private int _userId;
    public int UserId
    {
        get => _userId;
        set
        {
            if (SetProperty(ref _userId, value))
            {
                _ = LoadAuctionsAsync();
                _ = LoadMyLotsAsync();
                _ = LoadTradableInventoryAsync();
            }
        }
    }

    // ----- Rarity filters -----
    public ObservableCollection<FilterChip> RarityFilters { get; }
    public RelayCommand SelectRarityCommand { get; }

    private string _selectedRarityName = "ALL LOTS";
    public string SelectedRarityName
    {
        get => _selectedRarityName;
        set => SetProperty(ref _selectedRarityName, value);
    }

    // ----- Collections -----
    public ObservableCollection<AuctionLot> AuctionLots { get; }
    public ObservableCollection<AuctionLot> MyLots { get; }
    public ObservableCollection<AuctionLot> MyBids { get; }
    public ObservableCollection<InventoryItem> TradableInventory { get; }

    private AuctionLot? _selectedLot;
    public AuctionLot? SelectedLot
    {
        get => _selectedLot;
        set => SetProperty(ref _selectedLot, value);
    }

    private InventoryItem? _selectedInventoryItem;
    public InventoryItem? SelectedInventoryItem
    {
        get => _selectedInventoryItem;
        set => SetProperty(ref _selectedInventoryItem, value);
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
                OnPropertyChanged(nameof(IsMyLotsTab));
                OnPropertyChanged(nameof(IsSellTab));
            }
        }
    }

    public bool IsBrowseTab => ActiveTab == "Browse";
    public bool IsMyLotsTab => ActiveTab == "MyLots";
    public bool IsSellTab => ActiveTab == "Sell";

    private long _startingPrice = 1000;
    public long StartingPrice
    {
        get => _startingPrice;
        set => SetProperty(ref _startingPrice, value);
    }

    private long _buyoutPrice = 5000;
    public long BuyoutPrice
    {
        get => _buyoutPrice;
        set => SetProperty(ref _buyoutPrice, value);
    }

    private CurrencyType _auctionCurrency = CurrencyType.Cps;
    public CurrencyType AuctionCurrency
    {
        get => _auctionCurrency;
        set => SetProperty(ref _auctionCurrency, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    // ----- Escrow / watchlist summary -----
    public string EscrowPoolLabel => "AUCTION POOL";
    public string EscrowPoolValue => "1.42M CPs";
    public string EscrowPoolMeta => "HELD ACROSS 148 ACTIVE LOTS";
    public string EscrowPoolDelta => "+84K TODAY";

    public string WatchlistLabel => "WATCHLIST";
    public string WatchlistValue => "12";
    public string WatchlistMeta => "03 LOTS CLOSE WITHIN 24H";
    public string WatchlistDelta => "02 WON THIS MONTH";

    public string LotsWonLabel => "LOTS WON";
    public string LotsWonValue => "07";
    public string LotsWonMeta => "96,300 CPs LIFETIME";
    public string LotsWonDelta => "TOP 4% OF BIDDERS";

    // ----- Live lot -----
    public string LotName { get; private set; }
    public string LotSerial { get; private set; }
    public string LotRarity { get; private set; }
    public string LotToneHex { get; private set; }
    public string LotImagePath { get; private set; }
    public string LotCertifiedLabel { get; }
    public string LotCertifiedId { get; }
    public string LotProvenance { get; }
    public string LotFloor { get; }

    /// <summary>hh:mm:ss until the hammer falls.</summary>
    public string Countdown => _remaining.ToString(@"hh\:mm\:ss");

    // ----- Bid panel -----
    public string CurrentBid => FormatCps(_currentBidValue);
    public string CurrentBidMeta => $"{_bidCount.ToString(CultureInfo.InvariantCulture)} BIDS · +{FormatCps(SelectedIncrementValue)} MINIMUM";
    public string TopBidder { get; private set; }
    public string TopBidderMeta { get; private set; }
    public string Buyout => FormatCps(BuyoutValue);
    public string BuyoutMeta => "INSTANT SETTLEMENT VIA TWIN CITY";
    public double BidProgress => Math.Clamp(_currentBidValue / BuyoutValue, 0, 1);
    public string BidProgressLabel => $"{BidProgress:P0} OF BUYOUT";

    public ObservableCollection<BidIncrement> Increments { get; }
    public RelayCommand SelectIncrementCommand { get; }
    public RelayCommand PlaceBidCommand { get; }
    public RelayCommand WatchCommand { get; }
    public RelayCommand BuyoutCommand { get; }
    public RelayCommand CreateAuctionCommand { get; }
    public RelayCommand CancelAuctionCommand { get; }
    public RelayCommand SelectLotCommand { get; }
    public RelayCommand SelectInventoryForAuctionCommand { get; }
    public RelayCommand SwitchTabCommand { get; }

    public double SelectedIncrementValue =>
        Increments.FirstOrDefault(i => i.IsSelected)?.Value ?? 500d;

    public string PlaceBidLabel => $"PLACE BID · {FormatCps(_currentBidValue + SelectedIncrementValue)}";

    private bool _isWatching;
    public bool IsWatching
    {
        get => _isWatching;
        set
        {
            if (SetProperty(ref _isWatching, value))
                OnPropertyChanged(nameof(WatchLabel));
        }
    }

    public string WatchLabel => IsWatching ? "ON WATCHLIST" : "ADD TO WATCHLIST";

    public async Task LoadAuctionsAsync()
    {
        IsLoading = true;
        try
        {
            ItemRarity? rarityFilter = null;
            if (SelectedRarityName != "ALL LOTS" && Enum.TryParse<ItemRarity>(SelectedRarityName, true, out var rarity))
                rarityFilter = rarity;

            var lots = await _auctionService.GetActiveLotsAsync(0, 20, rarityFilter);
            AuctionLots.Clear();
            foreach (var lot in lots)
                AuctionLots.Add(lot);

            if (lots.Any() && SelectedLot == null)
            {
                SelectedLot = lots.First();
                LotName = SelectedLot.Item?.Name ?? SelectedLot.Title;
                LotRarity = SelectedLot.Item?.Rarity.ToDisplayName() ?? "RARE";
                LotToneHex = SelectedLot.Item?.GetRarityHex() ?? "#8A8F99";
                LotImagePath = SelectedLot.Item?.ImagePath ?? "Assets/card_trojan.png";
                _currentBidValue = SelectedLot.CurrentBid;
                _bidCount = SelectedLot.BidCount;
                TopBidder = SelectedLot.CurrentBidderName ?? "No bids";
                OnPropertyChanged(nameof(CurrentBid));
                OnPropertyChanged(nameof(PlaceBidLabel));
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

    public async Task LoadMyLotsAsync()
    {
        if (UserId == 0) return;
        try
        {
            var lots = await _auctionService.GetUserLotsAsync(UserId);
            var bids = await _auctionService.GetUserBidsAsync(UserId);

            MyLots.Clear();
            foreach (var l in lots) MyLots.Add(l);

            MyBids.Clear();
            foreach (var b in bids) MyBids.Add(b);
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
            foreach (var item in items) TradableInventory.Add(item);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task PlaceBidOnSelectedAsync()
    {
        if (SelectedLot == null || UserId == 0) return;
        IsLoading = true;
        try
        {
            var newBid = _currentBidValue + SelectedIncrementValue;
            var bid = await _auctionService.PlaceBidAsync(UserId, SelectedLot.Id, (long)newBid);
            _currentBidValue = bid.Amount;
            _bidCount++;
            TopBidder = bid.BidderName;
            TopBidderMeta = "YOU · BID PLACED";

            OnPropertyChanged(nameof(CurrentBid));
            OnPropertyChanged(nameof(CurrentBidMeta));
            OnPropertyChanged(nameof(TopBidder));
            OnPropertyChanged(nameof(TopBidderMeta));
            OnPropertyChanged(nameof(BidProgress));
            OnPropertyChanged(nameof(BidProgressLabel));
            OnPropertyChanged(nameof(PlaceBidLabel));

            StatusMessage = $"Bid placed: {FormatCps(bid.Amount)}";
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

    private async Task BuyoutSelectedAsync()
    {
        if (SelectedLot == null || UserId == 0) return;
        IsLoading = true;
        try
        {
            var success = await _auctionService.BuyoutAsync(UserId, SelectedLot.Id);
            if (success)
            {
                StatusMessage = $"Bought {SelectedLot.Title} for {FormatCps(SelectedLot.BuyoutPrice)}!";
                AuctionLots.Remove(SelectedLot);
                SelectedLot = AuctionLots.FirstOrDefault();
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

    private async Task CreateAuctionAsync()
    {
        if (SelectedInventoryItem == null || UserId == 0) return;
        IsLoading = true;
        try
        {
            var lot = await _auctionService.CreateLotAsync(UserId, SelectedInventoryItem.Id, StartingPrice, BuyoutPrice, AuctionCurrency, TimeSpan.FromDays(3));
            StatusMessage = $"Created auction for {lot.Title}";
            MyLots.Insert(0, lot);
            TradableInventory.Remove(SelectedInventoryItem);
            SelectedInventoryItem = null;
            ActiveTab = "MyLots";
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

    private async Task CancelAuctionAsync(AuctionLot lot)
    {
        if (UserId == 0) return;
        IsLoading = true;
        try
        {
            var success = await _auctionService.CancelLotAsync(UserId, lot.Id);
            if (success)
            {
                MyLots.Remove(lot);
                StatusMessage = $"Cancelled auction for {lot.Title}";
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

    // ----- Tables / feeds / carousel -----
    public ObservableCollection<AttributeRow> Attributes { get; }
    public ObservableCollection<HammerPrice> HammerPrices { get; }
    public ObservableCollection<VaultDrop> VaultDrops { get; }

    public string ProvenanceTitle => "ITEM ATTRIBUTES";
    public string HammerTitle => "RECENT HAMMER PRICES";
    public string VaultDropsTitle => "ACTIVE AUCTION DROPS";
    public string HammerFootnote => "MARKET UPDATED EVERY 60S";

    private static string FormatCps(double value)
        => value.ToString("N0", CultureInfo.InvariantCulture) + " CPs";
}
