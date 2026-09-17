using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NauraLauncher.Common;
using NauraLauncher.Infrastructure.DI;
using NauraLauncher.Models;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Production Home page: operator greeting, system stats, live game execution supervisor,
/// news intel stream, and library management.
/// </summary>
public class HomeViewModel : ObservableObject
{
    private string _continueCta = "CONTINUE PLAYING";
    private bool _isLaunching;

    public HomeViewModel()
    {
        Greeting = "GOOD EVENING, VALKYRIE";
        GreetingSub = "TWO SQUADMATES ONLINE · PATCH 2.4.1 STAGED FOR 03:00 UTC";
        DateLine = DateTime.UtcNow.ToString("ddd dd MMM", System.Globalization.CultureInfo.InvariantCulture).ToUpperInvariant() + " · SECTOR NEO-BRNO";

        // Query real disk capacity for Vault Storage stat card
        string diskStat = "412 GB";
        string diskDelta = "68% OF 600 GB USED";
        try
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady);
            if (drive != null)
            {
                long totalGb = drive.TotalSize / (1024 * 1024 * 1024);
                long freeGb = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                long usedGb = totalGb - freeGb;
                diskStat = $"{usedGb} GB";
                double pct = totalGb > 0 ? (double)usedGb / totalGb * 100 : 68;
                diskDelta = $"{pct:F0}% OF {totalGb} GB USED";
            }
        }
        catch { }

        StatCards = new ObservableCollection<StatCard>
        {
            new() { IconKey = "Icon.Clock",     Label = "PLAYTIME THIS WEEK", Value = "18H 22M", Delta = "+3H 10M VS LAST WEEK", DeltaIsPositive = true },
            new() { IconKey = "Icon.Trophy",    Label = "ACHIEVEMENTS",       Value = "214 / 380", Delta = "56% COMPLETION",     DeltaIsPositive = true },
            new() { IconKey = "Icon.Users",     Label = "SQUAD ONLINE",       Value = "07",      Delta = "2 IN LOBBY",          DeltaIsPositive = true },
            new() { IconKey = "Icon.HardDrive", Label = "VAULT STORAGE",      Value = diskStat,  Delta = diskDelta,             DeltaIsPositive = false },
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

        // Real Production Launch Commands
        ContinueCommand = new RelayCommand(async () => await ExecuteLaunchContinueGameAsync());
        PlayCommand = new RelayCommand(async p =>
        {
            if (p is LibraryItem item)
            {
                SelectedLibraryTitle = item.Title;
                await ExecuteLaunchLibraryItemAsync(item);
            }
        });

        // Listen for process lifecycle events
        ServiceContainer.GameLauncher.ProcessStateChanged += (s, e) =>
        {
            if (e.GameTitle == ContinueTitle)
            {
                if (e.IsRunning)
                {
                    ContinueCta = $"RUNNING (PID: {e.Pid})";
                    IsLaunching = false;
                }
                else
                {
                    ContinueCta = "CONTINUE PLAYING";
                    IsLaunching = false;
                }
            }
        };

        // Asynchronously sync library from database
        _ = SyncLibraryAsync();
    }

    private async Task SyncLibraryAsync()
    {
        try
        {
            var items = await ServiceContainer.Library.GetUserLibraryAsync();
            if (items.Count > 0)
            {
                Library.Clear();
                foreach (var item in items) Library.Add(item);
            }
        }
        catch { }
    }

    private async Task ExecuteLaunchContinueGameAsync()
    {
        if (ServiceContainer.GameLauncher.IsRunning(ContinueTitle))
        {
            await ServiceContainer.GameLauncher.TerminateAsync(ContinueTitle);
            return;
        }

        IsLaunching = true;
        ContinueCta = "LAUNCHING…";

        bool ok = await ServiceContainer.GameLauncher.LaunchAsync(ContinueTitle, "Protocol9.exe");
        if (!ok)
        {
            ContinueCta = "FAILED TO START";
            await Task.Delay(2000);
            ContinueCta = "CONTINUE PLAYING";
            IsLaunching = false;
        }
    }

    private async Task ExecuteLaunchLibraryItemAsync(LibraryItem item)
    {
        if (item.Status.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
        {
            item.Status = "DOWNLOADING…";
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string targetFolder = Path.Combine(appData, "NauraLauncher", "Games", item.Title.Replace(":", "").Replace("/", "_"));
            await ServiceContainer.Downloader.StartDownloadOrPatchAsync(item.Title, "/api/v1/download/package.bin", targetFolder);
            item.Status = "READY";
            item.StatusIsAccent = true;
        }
        else
        {
            await ServiceContainer.GameLauncher.LaunchAsync(item.Title, "game.exe");
        }
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

    public bool IsLaunching
    {
        get => _isLaunching;
        set => SetProperty(ref _isLaunching, value);
    }

    public string ContinueCta
    {
        get => _continueCta;
        set => SetProperty(ref _continueCta, value);
    }

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
