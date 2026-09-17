using System.Collections.ObjectModel;
using System.Linq;
using NauraLauncher.Common;
using NauraLauncher.Models;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Backing data for the APEX marketplace shell. All content mirrors the
/// supplied design and is fully data-bound so it can be swapped for a live
/// service later.
/// </summary>
public class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        // ----- Top nav -----
        NavItems = new ObservableCollection<string> { "Home", "Marketplace", "Auction", "Settings" };
        SelectedNav = "Marketplace";

        // ----- Category filter chips -----
        Categories = new ObservableCollection<CategoryTab>
        {
            new() { Name = "All Entries", IsSelected = true },
            new() { Name = "Tactical RPG" },
            new() { Name = "Dark Fantasy" },
            new() { Name = "Sci-Fi Sim" },
            new() { Name = "Indie Spotlight" },
        };

        SelectCategoryCommand = new RelayCommand(p =>
        {
            if (p is not CategoryTab tab) return;
            foreach (var c in Categories) c.IsSelected = ReferenceEquals(c, tab);
        });

        // ----- Curated archives grid -----
        Archives = new ObservableCollection<GameEntry>
        {
            new()
            {
                Title = "Monolith: Descent", Studio = "SOVEREIGN ARCH", ReleaseTag = "NOV 2024",
                Ribbon = "96% POSITIVE", RibbonIsAccent = true, Price = "$34.00",
                ImagePath = "Assets/card_monolith.png",
            },
            new()
            {
                Title = "SYNTHESIS // ZERO", Studio = "AETHER LABS", ReleaseTag = "DEC 2024",
                Ribbon = "OVERWHELMING", Price = "$44.99", Discount = "-15%",
                ImagePath = "Assets/card_synthesis.png",
            },
            new()
            {
                Title = "Grey Perimeter", Studio = "KINESIS CORE", ReleaseTag = "NEW RELEASE",
                Ribbon = "TACTICAL SANDBOX", Price = "$29.90",
                ImagePath = "Assets/card_grey.png",
            },
            new()
            {
                Title = "Oscillation IV: Remaster", Studio = "VALENCE SOUND", ReleaseTag = "EXPANSION",
                Ribbon = "SOUNDTRACK INCLUDED", Price = "$18.50",
                ImagePath = "Assets/card_oscillation.png",
            },
        };

        // ----- System / pipeline status tiles -----
        StatusTiles = new ObservableCollection<SystemStatus>
        {
            new() { IconKey = "Icon.Gauge",     Category = "DIRECTSTORAGE 2.0", Detail = "Asset Stream: 6.4 GB/s",        State = "OPTIMIZED"   },
            new() { IconKey = "Icon.Chip",      Category = "SHADER CACHE",      Detail = "Pre-compiled for RTX/RDNA",     State = "READY"       },
            new() { IconKey = "Icon.Waveform",  Category = "SPATIAL PIPELINE",  Detail = "Binaural Raytraced Audio",      State = "CALIBRATED"  },
        };

        // ----- Feature (hero) spotlight -----
        Feature = new FeatureViewModel();

        // ----- Commands -----
        PreOrderCommand = new RelayCommand(() => Feature.IsPreOrdered = !Feature.IsPreOrdered);
        SelectNavCommand = new RelayCommand(p => { if (p is string s) SelectedNav = s; });
    }

    public ObservableCollection<string> NavItems { get; }

    private string _selectedNav = "Marketplace";
    public string SelectedNav
    {
        get => _selectedNav;
        set => SetProperty(ref _selectedNav, value);
    }

    public ObservableCollection<CategoryTab> Categories { get; }
    public ObservableCollection<GameEntry> Archives { get; }
    public ObservableCollection<SystemStatus> StatusTiles { get; }
    public FeatureViewModel Feature { get; }

    // Header meta
    public string AppVersion => "v2.4.0";
    public string Ping => "18ms";
    public string Credits => "$148.50";
    public string UserName => "VALKYRIE";
    public string UserState => "ONLINE";
    public string ArchivesShowing => $"SHOWING {Archives.Count} OF 38";

    // Status bar
    public string SystemState => "SYSTEM READY";
    public string PatchLabel => "PATCH 2.4.1";
    public double PatchProgress => 0.75;
    public string PatchPercent => "75%";

    public RelayCommand SelectCategoryCommand { get; }
    public RelayCommand PreOrderCommand { get; }
    public RelayCommand SelectNavCommand { get; }
}
