using System;
using System.Collections.ObjectModel;
using NauraLauncher.Common;
using NauraLauncher.Models;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Home page: operator greeting, quick stats, the "Continue Playing" hero,
/// the "Latest Intel" news list and the installed-library grid.
/// </summary>
public class HomeViewModel : ObservableObject
{
    public HomeViewModel()
    {
        Greeting = "GOOD EVENING, VALKYRIE";
        GreetingSub = "TWO SQUADMATES ONLINE · PATCH 2.4.1 STAGED FOR 03:00 UTC";
        DateLine = "THU 17 SEP · SECTOR NEO-BRNO";

        StatCards = new ObservableCollection<StatCard>
        {
            new() { IconKey = "Icon.Clock",     Label = "PLAYTIME THIS WEEK", Value = "12H 40M", Delta = "+3H 10M VS LAST WEEK", DeltaIsPositive = true },
            new() { IconKey = "Icon.Trophy",    Label = "ACHIEVEMENTS",       Value = "214 / 380", Delta = "56% COMPLETION",     DeltaIsPositive = true },
            new() { IconKey = "Icon.Users",     Label = "SQUAD ONLINE",       Value = "07",      Delta = "2 IN LOBBY",          DeltaIsPositive = true },
            new() { IconKey = "Icon.HardDrive", Label = "VAULT STORAGE",      Value = "412 GB",  Delta = "68% OF 600 GB USED",  DeltaIsPositive = false },
        };

        ContinueTitle = "PROTOCOL 9: ECLIPSE";
        ContinueStudio = "NEXUS ENTERTAINMENT";
        ContinueMission = "MISSION 07 // SILENT CARTOGRAPHER";
        ContinueProgressLabel = "62% COMPLETE";
        ContinuePlaytime = "18H 22M PLAYED";
        ContinueImagePath = "Assets/hero_protocol9.png";
        ContinueProgress = 0.62;

        NewsItems = new ObservableCollection<NewsItem>
        {
            new() { Category = "LIVE",         Title = "OBSIDIAN KATANA LOT #0042 CROSSES $52K", Meta = "2M AGO",  ToneHex = "#34D399", IsLive = true },
            new() { Category = "PATCH NOTES",  Title = "2.4.1 — DIRECTSTORAGE STREAMING REBALANCE",  Meta = "1H AGO",  ToneHex = "#8A8F99" },
            new() { Category = "VAULT DROP",   Title = "SERIAL EDITION ALLOCATION OPENS FRIDAY",     Meta = "4H AGO",  ToneHex = "#F5A524" },
            new() { Category = "ESPORTS",      Title = "APEX CIRCUIT QUALIFIERS — REGIONAL BRACKET", Meta = "9H AGO",  ToneHex = "#60A5FA" },
            new() { Category = "COMMUNITY",    Title = "GREY PERIMETER MOD TOOLKIT 1.2 SHIPPED",     Meta = "1D AGO",  ToneHex = "#C084FC" },
        };

        Library = new ObservableCollection<LibraryItem>
        {
            new()
            {
                Title = "PROTOCOL 9: ECLIPSE", Studio = "NEXUS ENTERTAINMENT", Status = "READY",
                Playtime = "18H 22M PLAYED", Progress = 0.62, StatusIsAccent = true,
                ImagePath = "Assets/hero_protocol9.png",
            },
            new()
            {
                Title = "MONOLITH: DESCENT", Studio = "SOVEREIGN ARCH", Status = "UPDATE 1.2 GB",
                Playtime = "41H 08M PLAYED", Progress = 1.0,
                ImagePath = "Assets/card_monolith.png",
            },
            new()
            {
                Title = "SYNTHESIS // ZERO", Studio = "AETHER LABS", Status = "READY",
                Playtime = "07H 55M PLAYED", Progress = 0.28, StatusIsAccent = true,
                ImagePath = "Assets/card_synthesis.png",
            },
            new()
            {
                Title = "GREY PERIMETER", Studio = "KINESIS CORE", Status = "VERIFYING",
                Playtime = "02H 11M PLAYED", Progress = 0.91,
                ImagePath = "Assets/card_grey.png",
            },
        };

        ContinueCommand = new RelayCommand(() => IsLaunching = !IsLaunching);
        PlayCommand = new RelayCommand(p =>
        {
            if (p is LibraryItem item)
                SelectedLibraryTitle = item.Title;
        });
    }

    // ----- Greeting block -----
    public string Greeting { get; }
    public string GreetingSub { get; }
    public string DateLine { get; }

    public ObservableCollection<StatCard> StatCards { get; }

    // ----- Continue Playing hero -----
    public string ContinueTitle { get; }
    public string ContinueStudio { get; }
    public string ContinueMission { get; }
    public string ContinueProgressLabel { get; }
    public string ContinuePlaytime { get; }
    public string ContinueImagePath { get; }
    public double ContinueProgress { get; }

    private bool _isLaunching;
    public bool IsLaunching
    {
        get => _isLaunching;
        set
        {
            if (SetProperty(ref _isLaunching, value))
                OnPropertyChanged(nameof(ContinueCta));
        }
    }

    /// <summary>Button label flips while a launch is being staged.</summary>
    public string ContinueCta => IsLaunching ? "LAUNCHING…" : "CONTINUE PLAYING";

    public ObservableCollection<NewsItem> NewsItems { get; }
    public ObservableCollection<LibraryItem> Library { get; }

    private string? _selectedLibraryTitle;
    public string? SelectedLibraryTitle
    {
        get => _selectedLibraryTitle;
        set => SetProperty(ref _selectedLibraryTitle, value);
    }

    public RelayCommand ContinueCommand { get; }
    public RelayCommand PlayCommand { get; }
}
