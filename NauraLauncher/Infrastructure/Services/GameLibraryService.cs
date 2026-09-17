using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NauraLauncher.Core.Entities;
using NauraLauncher.Core.Interfaces;
using NauraLauncher.Models;

namespace NauraLauncher.Infrastructure.Services;

/// <summary>
/// Service coordinating user entitlements, game catalog browsing, purchases, and playtime tracking.
/// </summary>
public class GameLibraryService : IGameLibraryService
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;

    // Resilient local memory cache
    private readonly List<LibraryItem> _cachedLibrary = new()
    {
        new()
        {
            Title = "PROTOCOL 9: ECLIPSE",
            Studio = "NEXUS ENTERTAINMENT",
            Status = "READY",
            Playtime = "18H 22M PLAYED",
            Progress = 0.62,
            StatusIsAccent = true,
            ImagePath = "Assets/hero_protocol9.png"
        },
        new()
        {
            Title = "MONOLITH: DESCENT",
            Studio = "SOVEREIGN ARCH",
            Status = "UPDATE 1.2 GB",
            Playtime = "41H 08M PLAYED",
            Progress = 1.0,
            ImagePath = "Assets/card_monolith.png"
        },
        new()
        {
            Title = "SYNTHESIS // ZERO",
            Studio = "AETHER LABS",
            Status = "READY",
            Playtime = "07H 55M PLAYED",
            Progress = 0.28,
            StatusIsAccent = true,
            ImagePath = "Assets/card_synthesis.png"
        },
        new()
        {
            Title = "GREY PERIMETER",
            Studio = "KINESIS CORE",
            Status = "VERIFYING",
            Playtime = "02H 11M PLAYED",
            Progress = 0.91,
            ImagePath = "Assets/card_grey.png"
        }
    };

    private readonly List<GameEntry> _cachedCatalog = new()
    {
        new()
        {
            Title = "Monolith: Descent",
            Studio = "SOVEREIGN ARCH",
            ReleaseTag = "NOV 2024",
            Ribbon = "96% POSITIVE",
            RibbonIsAccent = true,
            Price = "$34.00",
            ImagePath = "Assets/card_monolith.png"
        },
        new()
        {
            Title = "SYNTHESIS // ZERO",
            Studio = "AETHER LABS",
            ReleaseTag = "DEC 2024",
            Ribbon = "OVERWHELMING",
            Price = "$44.99",
            Discount = "-15%",
            ImagePath = "Assets/card_synthesis.png"
        },
        new()
        {
            Title = "Grey Perimeter",
            Studio = "KINESIS CORE",
            ReleaseTag = "NEW RELEASE",
            Ribbon = "TACTICAL SANDBOX",
            Price = "$29.90",
            ImagePath = "Assets/card_grey.png"
        },
        new()
        {
            Title = "Oscillation IV: Remaster",
            Studio = "VALENCE SOUND",
            ReleaseTag = "EXPANSION",
            Ribbon = "SOUNDTRACK INCLUDED",
            Price = "$18.50",
            ImagePath = "Assets/card_oscillation.png"
        }
    };

    public GameLibraryService(IApiClient apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task<IReadOnlyList<LibraryItem>> GetUserLibraryAsync()
    {
        try
        {
            var res = await _apiClient.GetAsync<LibraryResponse>("/api/v1/library");
            if (res?.Library != null && res.Library.Count > 0)
            {
                var items = res.Library.Select(l => new LibraryItem
                {
                    Title = l.Title,
                    Studio = l.Studio,
                    Status = l.Status,
                    Playtime = FormatPlaytime(l.Playtime_Seconds),
                    Progress = l.Campaign_Progress,
                    StatusIsAccent = l.Status == "READY",
                    ImagePath = l.Image_Path
                }).ToList();

                _cachedLibrary.Clear();
                _cachedLibrary.AddRange(items);
                return _cachedLibrary;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GameLibrary] Fetch library error: {ex.Message}");
        }

        return _cachedLibrary;
    }

    public async Task<IReadOnlyList<GameEntry>> GetCatalogAsync(string? category = null)
    {
        try
        {
            string url = string.IsNullOrEmpty(category) || category == "All Entries"
                ? "/api/v1/games"
                : $"/api/v1/games?category={Uri.EscapeDataString(category)}";

            var res = await _apiClient.GetAsync<CatalogResponse>(url);
            if (res?.Games != null && res.Games.Count > 0)
            {
                var entries = res.Games.Select(g => new GameEntry
                {
                    Title = g.Title,
                    Studio = g.Studio,
                    ReleaseTag = g.Release_Tag,
                    Ribbon = g.Ribbon,
                    RibbonIsAccent = g.Ribbon_Is_Accent == 1,
                    Price = "$" + g.Price.ToString("F2"),
                    Discount = g.Discount_Pct > 0 ? $"-{g.Discount_Pct}%" : null,
                    ImagePath = g.Image_Path
                }).ToList();

                _cachedCatalog.Clear();
                _cachedCatalog.AddRange(entries);
                return _cachedCatalog;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GameLibrary] Fetch catalog error: {ex.Message}");
        }

        return _cachedCatalog;
    }

    public async Task<GameEntry?> GetFeaturedGameAsync()
    {
        var catalog = await GetCatalogAsync();
        return catalog.FirstOrDefault();
    }

    public async Task<bool> ClaimOrPurchaseGameAsync(string gameId)
    {
        try
        {
            var res = await _apiClient.PostAsync<object, ClaimResponse>("/api/v1/library/claim", new { gameId });
            if (res != null && res.Status == "CLAIMED")
            {
                await _authService.RefreshProfileAsync();
                await GetUserLibraryAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GameLibrary] Claim error: {ex.Message}");
        }
        return false;
    }

    public async Task RecordPlaytimeAsync(string gameId, int seconds)
    {
        try
        {
            await _apiClient.PostAsync<object, object>($"/api/v1/library/{gameId}/playtime", new { seconds });
        }
        catch { }
    }

    private static string FormatPlaytime(long seconds)
    {
        long hours = seconds / 3600;
        long minutes = (seconds % 3600) / 60;
        return $"{hours:D2}H {minutes:D2}M PLAYED";
    }

    private record LibraryResponse(List<LibraryDto> Library);
    private record LibraryDto(string Id, string Title, string Studio, string Status, long Playtime_Seconds, double Campaign_Progress, string Image_Path);
    private record CatalogResponse(List<GameDto> Games);
    private record GameDto(string Id, string Title, string Studio, string Release_Tag, string Ribbon, int Ribbon_Is_Accent, double Price, int Discount_Pct, string Image_Path);
    private record ClaimResponse(string Status, string LicenseKey, double RemainingCredits);
}
