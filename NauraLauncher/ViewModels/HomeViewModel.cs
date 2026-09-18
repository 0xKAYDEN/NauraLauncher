using System.Collections.ObjectModel;
using NauraLauncher.Common;
using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;
using NauraLauncher.Models;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Home page: Conquer Online themed - player greeting, quick stats, continue journey, latest news, character showcase
/// </summary>
public class HomeViewModel : ObservableObject
{
    public HomeViewModel()
    {
        Greeting = "WELCOME BACK, HERO";
        GreetingSub = "TWIN CITY MARKET OPEN · 3 FRIENDS ONLINE · SERVER: ETERNITY";
        DateLine = "THU 17 SEP · TWIN CITY (178,182)";

        StatCards = new ObservableCollection<StatCard>
        {
            new() { IconKey = "Icon.Clock",     Label = "PLAYTIME THIS WEEK", Value = "23H 40M", Delta = "+5H 10M VS LAST WEEK", DeltaIsPositive = true },
            new() { IconKey = "Icon.Trophy",    Label = "BATTLE POWER",       Value = "4,850", Delta = "+120 THIS WEEK",     DeltaIsPositive = true },
            new() { IconKey = "Icon.Users",     Label = "FRIENDS ONLINE",       Value = "07",      Delta = "2 IN TWIN CITY",          DeltaIsPositive = true },
            new() { IconKey = "Icon.Gold", Label = "WEALTH",      Value = "1.2M Gold",  Delta = "+50K TODAY",  DeltaIsPositive = true },
        };

        ContinueTitle = "CONQUER ONLINE";
        ContinueStudio = "TQ DIGITAL";
        ContinueMission = "QUEST: DRAGON BALL HUNT // MARKET CENTER";
        ContinueProgressLabel = "LEVEL 130 - 2ND REBORN TROJAN";
        ContinuePlaytime = "1,240H TOTAL PLAYED";
        ContinueImagePath = "Assets/hero_conquer.png";
        ContinueProgress = 0.85;

        NewsItems = new ObservableCollection<NewsItem>
        {
            new() { Category = "LIVE",         Title = "SUPER DRAGON GEM AUCTION CROSSES 15K CPS", Meta = "2M AGO",  ToneHex = "#34D399", IsLive = true },
            new() { Category = "PATCH NOTES",  Title = "PATCH 7009 — NEW NINJA SKILLS & BALANCE",  Meta = "1H AGO",  ToneHex = "#8A8F99" },
            new() { Category = "MARKET",   Title = "MARKETPLACE BOOM: +200% TRADE VOLUME TODAY",     Meta = "4H AGO",  ToneHex = "#F5A524" },
            new() { Category = "EVENT",      Title = "GUILD WAR QUALIFIERS — REGISTRATION OPEN", Meta = "9H AGO",  ToneHex = "#60A5FA" },
            new() { Category = "COMMUNITY",    Title = "DRAGON BALL EXCHANGE RATE HITS RECORD HIGH",     Meta = "1D AGO",  ToneHex = "#C084FC" },
        };

        Library = new ObservableCollection<LibraryItem>
        {
            new()
            {
                Title = "TROJAN HERO", Studio = "MAIN CHARACTER", Status = "2ND REBORN - LVL 130",
                Playtime = "TWIN CITY", Progress = 1.0, StatusIsAccent = true,
                ImagePath = "Assets/card_trojan.png",
            },
            new()
            {
                Title = "NINJA ASSASSIN", Studio = "SECONDARY", Status = "1ST REBORN - LVL 120",
                Playtime = "PHOENIX CASTLE", Progress = 0.85,
                ImagePath = "Assets/card_ninja.png",
            },
            new()
            {
                Title = "FIRE TAOIST", Studio = "SUPPORT", Status = "LVL 110",
                Playtime = "MARKET", Progress = 0.62, StatusIsAccent = false,
                ImagePath = "Assets/card_taoist.png",
            },
            new()
            {
                Title = "WARRIOR TANK", Studio = "ALT CHARACTER", Status = "LVL 95",
                Playtime = "DESERT CITY", Progress = 0.45,
                ImagePath = "Assets/card_warrior.png",
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
    public string ContinueCta => IsLaunching ? "LAUNCHING CONQUER..." : "ENTER TWIN CITY";

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
