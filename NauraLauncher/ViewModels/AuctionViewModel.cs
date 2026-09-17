using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Threading;
using NauraLauncher.Common;
using NauraLauncher.Models;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Auction page: rarity filters, escrow / watchlist summary, the live
/// "Obsidian Katana // Serial #0042" lot with countdown, the bid panel,
/// provenance table, hammer-price feed and vault-drop carousel.
/// </summary>
public class AuctionViewModel : ObservableObject
{
    private readonly DispatcherTimer _countdownTimer;
    private TimeSpan _remaining = new(0, 2, 41, 17);

    private const double BuyoutValue = 78_000d;
    private double _currentBidValue = 52_400d;
    private int _bidCount = 137;

    public AuctionViewModel()
    {
        RarityFilters = new ObservableCollection<FilterChip>
        {
            new() { Name = "ALL LOTS",   Count = "148", ToneHex = "#F4F5F7", IsSelected = true },
            new() { Name = "COMMON",     Count = "62",  ToneHex = "#8A8F99" },
            new() { Name = "RARE",       Count = "38",  ToneHex = "#60A5FA" },
            new() { Name = "EPIC",       Count = "24",  ToneHex = "#C084FC" },
            new() { Name = "LEGENDARY",  Count = "15",  ToneHex = "#F5A524" },
            new() { Name = "MYTHIC",     Count = "06",  ToneHex = "#F87171" },
            new() { Name = "OBSIDIAN",   Count = "03",  ToneHex = "#34D399" },
        };

        SelectRarityCommand = new RelayCommand(p =>
        {
            if (p is not FilterChip chip) return;
            foreach (var c in RarityFilters) c.IsSelected = ReferenceEquals(c, chip);
            SelectedRarityName = chip.Name;
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

            // The bid read-outs are derived from the selected increment.
            OnPropertyChanged(nameof(SelectedIncrementValue));
            OnPropertyChanged(nameof(CurrentBidMeta));
            OnPropertyChanged(nameof(PlaceBidLabel));
        });

        Attributes = new ObservableCollection<AttributeRow>
        {
            new() { Label = "ORIGIN",          Value = "FORGE SECTOR 09 — NEO-BRNO" },
            new() { Label = "FORGE DATE",      Value = "14 · 09 · 2089", MonoValue = "14·09·2089" },
            new() { Label = "ALLOY",           Value = "CARBON-LATTICE OBSIDIAN" },
            new() { Label = "EDGE GEOMETRY",   Value = "SINGLE BEVEL · 11.4°" },
            new() { Label = "MASS",            Value = "1.18 KG" },
            new() { Label = "OWNER HISTORY",   Value = "3 REGISTERED CUSTODIANS" },
            new() { Label = "CERTIFICATION",   Value = "AV-9F2C-0042", MonoValue = "AV-9F2C-0042", IsAccent = true },
            new() { Label = "VAULT STATUS",    Value = "ESCROWED · TRANSFER LOCKED", IsAccent = true },
        };

        HammerPrices = new ObservableCollection<HammerPrice>
        {
            new() { Item = "OBSIDIAN KATANA", Serial = "#0017", Price = "$48,200", Delta = "+12.4%", DeltaIsPositive = true,  When = "6M AGO",  ToneHex = "#34D399" },
            new() { Item = "ECLIPSE VISOR",   Serial = "#0231", Price = "$21,900", Delta = "+4.1%",  DeltaIsPositive = true,  When = "22M AGO", ToneHex = "#F87171" },
            new() { Item = "VOID LANCE",      Serial = "#0008", Price = "$63,750", Delta = "+18.9%", DeltaIsPositive = true,  When = "48M AGO", ToneHex = "#C084FC" },
            new() { Item = "GREY MANTLE",     Serial = "#0444", Price = "$7,400",  Delta = "-2.6%",  DeltaIsPositive = false, When = "1H AGO",  ToneHex = "#8A8F99" },
            new() { Item = "SOLARIS EDGE",    Serial = "#0102", Price = "$15,150", Delta = "+0.8%",  DeltaIsPositive = true,  When = "2H AGO",  ToneHex = "#F5A524" },
        };

        VaultDrops = new ObservableCollection<VaultDrop>
        {
            new()
            {
                Name = "ECLIPSE VISOR", Edition = "SERIAL EDITION · 1 OF 120", Rarity = "MYTHIC",
                ToneHex = "#F87171", DropWindow = "DROPS IN 04:12:09", Progress = 0.72,
                ImagePath = "Assets/card_synthesis.png",
            },
            new()
            {
                Name = "VOID LANCE", Edition = "FORGE RUN · 1 OF 40", Rarity = "LEGENDARY",
                ToneHex = "#C084FC", DropWindow = "DROPS IN 11:40:55", Progress = 0.35,
                ImagePath = "Assets/card_oscillation.png",
            },
            new()
            {
                Name = "GREY MANTLE", Edition = "COMBAT PROVENANCE · 1 OF 300", Rarity = "RARE",
                ToneHex = "#60A5FA", DropWindow = "DROPS IN 1D 02:15", Progress = 0.91,
                ImagePath = "Assets/card_grey.png",
            },
            new()
            {
                Name = "MONOLITH SHARD", Edition = "ARCHIVE CAST · 1 OF 24", Rarity = "OBSIDIAN",
                ToneHex = "#34D399", DropWindow = "DROPS IN 2D 06:30", Progress = 0.18,
                ImagePath = "Assets/card_monolith.png",
            },
        };

        // ----- Live lot -----
        LotName = "OBSIDIAN KATANA";
        LotSerial = "SERIAL #0042";
        LotRarity = "OBSIDIAN";
        LotToneHex = "#34D399";
        LotImagePath = "Assets/card_monolith.png";
        LotCertifiedLabel = "CERTIFIED";
        LotCertifiedId = "AV-9F2C-0042";
        LotProvenance = "FORGED SECTOR 09 · 3 CUSTODIANS · ESCROW HELD";
        LotFloor = "FLOOR $38,000";
        TopBidder = "SHOGUN_07";
        TopBidderMeta = "LEVEL 42 · VERIFIED COLLECTOR";

        PlaceBidCommand = new RelayCommand(PlaceBid);
        WatchCommand = new RelayCommand(() => IsWatching = !IsWatching);

        // Countdown ticks once per second, like the live lot timer in the design.
        _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _countdownTimer.Tick += (_, _) =>
        {
            if (_remaining <= TimeSpan.Zero) return;
            _remaining = _remaining.Subtract(TimeSpan.FromSeconds(1));
            OnPropertyChanged(nameof(Countdown));
        };
        _countdownTimer.Start();
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

    // ----- Escrow / watchlist summary -----
    public string EscrowPoolLabel => "ESCROW POOL";
    public string EscrowPoolValue => "$1.42M";
    public string EscrowPoolMeta => "HELD ACROSS 148 ACTIVE LOTS";
    public string EscrowPoolDelta => "+$84K TODAY";

    public string WatchlistLabel => "WATCHLIST";
    public string WatchlistValue => "12";
    public string WatchlistMeta => "03 LOTS CLOSE WITHIN 24H";
    public string WatchlistDelta => "02 WON THIS MONTH";

    public string LotsWonLabel => "LOTS WON";
    public string LotsWonValue => "07";
    public string LotsWonMeta => "$96,300 LIFETIME HAMMER";
    public string LotsWonDelta => "TOP 4% OF BIDDERS";

    // ----- Live lot -----
    public string LotName { get; }
    public string LotSerial { get; }
    public string LotRarity { get; }
    public string LotToneHex { get; }
    public string LotImagePath { get; }
    public string LotCertifiedLabel { get; }
    public string LotCertifiedId { get; }
    public string LotProvenance { get; }
    public string LotFloor { get; }

    /// <summary>hh:mm:ss until the hammer falls.</summary>
    public string Countdown => _remaining.ToString(@"hh\:mm\:ss");

    // ----- Bid panel -----
    public string CurrentBid => FormatCredits(_currentBidValue);
    public string CurrentBidMeta => $"{_bidCount.ToString(CultureInfo.InvariantCulture)} BIDS · +{FormatCredits(SelectedIncrementValue)} MINIMUM";
    public string TopBidder { get; private set; }
    public string TopBidderMeta { get; private set; }
    public string Buyout => FormatCredits(BuyoutValue);
    public string BuyoutMeta => "INSTANT SETTLEMENT VIA ESCROW";
    public double BidProgress => Math.Clamp(_currentBidValue / BuyoutValue, 0, 1);
    public string BidProgressLabel => $"{BidProgress:P0} OF BUYOUT";

    public ObservableCollection<BidIncrement> Increments { get; }
    public RelayCommand SelectIncrementCommand { get; }
    public RelayCommand PlaceBidCommand { get; }
    public RelayCommand WatchCommand { get; }

    public double SelectedIncrementValue =>
        Increments.FirstOrDefault(i => i.IsSelected)?.Value ?? 500d;

    public string PlaceBidLabel => $"PLACE BID · {FormatCredits(_currentBidValue + SelectedIncrementValue)}";

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

    private void PlaceBid()
    {
        _currentBidValue += SelectedIncrementValue;
        _bidCount++;
        TopBidder = "VALKYRIE (YOU)";
        TopBidderMeta = "LEVEL 38 · ESCROW CLEARED";

        OnPropertyChanged(nameof(CurrentBid));
        OnPropertyChanged(nameof(CurrentBidMeta));
        OnPropertyChanged(nameof(TopBidder));
        OnPropertyChanged(nameof(TopBidderMeta));
        OnPropertyChanged(nameof(BidProgress));
        OnPropertyChanged(nameof(BidProgressLabel));
        OnPropertyChanged(nameof(PlaceBidLabel));
    }

    // ----- Tables / feeds / carousel -----
    public ObservableCollection<AttributeRow> Attributes { get; }
    public ObservableCollection<HammerPrice> HammerPrices { get; }
    public ObservableCollection<VaultDrop> VaultDrops { get; }

    public string ProvenanceTitle => "PROVENANCE & ATTRIBUTES";
    public string HammerTitle => "RECENT HAMMER PRICES";
    public string VaultDropsTitle => "ACTIVE VAULT DROPS";
    public string HammerFootnote => "INDEX UPDATED EVERY 60S";

    private static string FormatCredits(double value)
        => "$" + value.ToString("N0", CultureInfo.InvariantCulture);
}
