using System.Collections.ObjectModel;
using NauraLauncher.Common;
using NauraLauncher.Models;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Backing data for the Marketplace page. Everything here was previously owned
/// by <see cref="MainViewModel"/>; the shell now owns the chrome and this VM
/// owns the gallery (category chips, spotlight hero, curated archives, pipeline
/// status tiles).
/// </summary>
public class MarketplaceViewModel : ObservableObject
{
    public MarketplaceViewModel()
    {
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
            new() { IconKey = "Icon.Gauge",     Category = "DIRECTSTORAGE 2.0", Detail = "Asset Stream: 6.4 GB/s",    State = "OPTIMIZED"  },
            new() { IconKey = "Icon.Chip",      Category = "SHADER CACHE",      Detail = "Pre-compiled for RTX/RDNA", State = "READY"      },
            new() { IconKey = "Icon.Waveform",  Category = "SPATIAL PIPELINE",  Detail = "Binaural Raytraced Audio",  State = "CALIBRATED" },
        };

        // ----- Feature (hero) spotlight -----
        Feature = new FeatureViewModel();

        PreOrderCommand = new RelayCommand(() => Feature.IsPreOrdered = !Feature.IsPreOrdered);
    }

    public ObservableCollection<CategoryTab> Categories { get; }
    public ObservableCollection<GameEntry> Archives { get; }
    public ObservableCollection<SystemStatus> StatusTiles { get; }
    public FeatureViewModel Feature { get; }

    public string SortLabel => "Top Rated";
    public string ArchivesShowing => $"SHOWING {Archives.Count} OF 38";

    public RelayCommand SelectCategoryCommand { get; }
    public RelayCommand PreOrderCommand { get; }
}
