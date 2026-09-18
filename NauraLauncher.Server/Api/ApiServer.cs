using System.Net;
using System.Text;
using System.Text.Json;
using NauraLauncher.Server.Infrastructure;
using NauraLauncher.Server.Realtime;

namespace NauraLauncher.Server.Api;

/// <summary>
/// Simple HTTP API server for Conquer Online launcher
/// Handles auth, marketplace, auction, friends, inventory, wallet
/// Clean architecture: API layer, uses in-memory services for demo (can be swapped with MySQL repositories)
/// </summary>
public class ApiServer
{
    private readonly int _port;
    private readonly DatabaseService _database;
    private readonly VoiceSignalingServer _voiceServer;
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;

    // In-memory stores for demo - in production would use MySQL repositories
    private readonly Dictionary<string, object> _mockData = new();

    public ApiServer(int port, DatabaseService database, VoiceSignalingServer voiceServer)
    {
        _port = port;
        _database = database;
        _voiceServer = voiceServer;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://0.0.0.0:{_port}/");
        _listener.Start();

        _ = Task.Run(() => HandleRequestsAsync(_cts.Token));
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
    }

    private async Task HandleRequestsAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var context = await _listener!.GetContextAsync();
                _ = Task.Run(() => ProcessRequestAsync(context), ct);
            }
            catch when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Listener error: {ex.Message}");
                await Task.Delay(1000, ct);
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // CORS headers for launcher
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

        if (request.HttpMethod == "OPTIONS")
        {
            response.StatusCode = 200;
            response.Close();
            return;
        }

        var path = request.Url?.AbsolutePath ?? "/";
        var method = request.HttpMethod;

        Console.WriteLine($"[API] {method} {path}");

        try
        {
            string responseJson = "";
            int statusCode = 200;

            // Routing
            if (path.StartsWith("/api/auth/login") && method == "POST")
            {
                responseJson = await HandleLoginAsync(request);
            }
            else if (path.StartsWith("/api/auth/register") && method == "POST")
            {
                responseJson = await HandleRegisterAsync(request);
            }
            else if (path.StartsWith("/api/marketplace/listings") && method == "GET")
            {
                responseJson = HandleMarketplaceListings(request);
            }
            else if (path.StartsWith("/api/marketplace/list") && method == "POST")
            {
                responseJson = await HandleMarketplaceListAsync(request);
            }
            else if (path.StartsWith("/api/marketplace/buy") && method == "POST")
            {
                responseJson = await HandleMarketplaceBuyAsync(request);
            }
            else if (path.StartsWith("/api/auction/lots") && method == "GET")
            {
                responseJson = HandleAuctionLots(request);
            }
            else if (path.StartsWith("/api/auction/bid") && method == "POST")
            {
                responseJson = await HandleAuctionBidAsync(request);
            }
            else if (path.StartsWith("/api/auction/buyout") && method == "POST")
            {
                responseJson = await HandleAuctionBuyoutAsync(request);
            }
            else if (path.StartsWith("/api/friends") && method == "GET")
            {
                responseJson = HandleFriendsList(request);
            }
            else if (path.StartsWith("/api/friends/add") && method == "POST")
            {
                responseJson = await HandleFriendsAddAsync(request);
            }
            else if (path.StartsWith("/api/friends/block") && method == "POST")
            {
                responseJson = await HandleFriendsBlockAsync(request);
            }
            else if (path.StartsWith("/api/inventory") && method == "GET")
            {
                responseJson = HandleInventory(request);
            }
            else if (path.StartsWith("/api/wallet") && method == "GET")
            {
                responseJson = HandleWallet(request);
            }
            else if (path == "/" || path == "/health")
            {
                responseJson = JsonSerializer.Serialize(new
                {
                    status = "ok",
                    service = "NauraLauncher Server - Conquer Online",
                    version = "2.4.0",
                    timestamp = DateTime.UtcNow,
                    endpoints = new[]
                    {
                        "/api/auth/login",
                        "/api/auth/register",
                        "/api/marketplace/listings",
                        "/api/auction/lots",
                        "/api/friends",
                        "/api/inventory",
                        "/api/wallet",
                        "/ws/voice (WebSocket on port 8081)"
                    },
                    conquer = new
                    {
                        server = "Eternity",
                        patch = "7009",
                        marketLocation = "Twin City (178,182)",
                        currencies = new[] { "CPs (Conquer Points)", "Gold", "Silver", "Bound CPs" },
                        classes = new[] { "Trojan", "Warrior", "Archer", "Fire Taoist", "Water Taoist", "Ninja", "Monk", "Pirate", "Dragon Warrior" }
                    }
                });
            }
            else
            {
                statusCode = 404;
                responseJson = JsonSerializer.Serialize(new { error = "Not found", path });
            }

            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            var buffer = Encoding.UTF8.GetBytes(responseJson);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] Error handling {path}: {ex.Message}");
            try
            {
                response.StatusCode = 500;
                var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
                var buffer = Encoding.UTF8.GetBytes(errorJson);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
            catch { }
        }
    }

    private async Task<string> ReadBodyAsync(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        return await reader.ReadToEndAsync();
    }

    private async Task<string> HandleLoginAsync(HttpListenerRequest request)
    {
        var body = await ReadBodyAsync(request);
        Console.WriteLine($"[Auth] Login attempt: {body}");

        // Mock success for any login in demo mode
        var response = new
        {
            success = true,
            message = "Login successful - Welcome to Twin City",
            user = new
            {
                id = 100,
                username = "DragonLord",
                displayName = "DragonLord",
                level = 130,
                mainClass = "Trojan",
                reborn = "2nd Reborn",
                avatar = "Assets/avatar_conquer.png",
                wallet = new { cps = 50000, gold = 1000000, silver = 500000, boundCps = 1000 }
            },
            token = "mock-jwt-token-conquer-online"
        };

        return JsonSerializer.Serialize(response);
    }

    private async Task<string> HandleRegisterAsync(HttpListenerRequest request)
    {
        var body = await ReadBodyAsync(request);
        Console.WriteLine($"[Auth] Register: {body}");

        var response = new
        {
            success = true,
            message = "Hero created successfully - Welcome to Conquer Online",
            user = new
            {
                id = new Random().Next(1000, 9999),
                username = "NewHero",
                displayName = "NewHero",
                level = 1,
                mainClass = "Trojan",
                wallet = new { cps = 1000, gold = 100000, silver = 50000, boundCps = 100 }
            },
            token = "mock-jwt-token-new-hero"
        };

        return JsonSerializer.Serialize(response);
    }

    private string HandleMarketplaceListings(HttpListenerRequest request)
    {
        // Return mock marketplace data - Conquer Online themed
        var listings = new[]
        {
            new { id = 1, seller = "DragonLord", item = new { name = "Dragon Blade +12", rarity = "Legendary", type = "Weapon", plus = 12, sockets = 2 }, price = 52000, currency = "Cps", status = "Active" },
            new { id = 2, seller = "FireQueen", item = new { name = "Super Dragon Gem", rarity = "Super", type = "Gem", plus = 0, sockets = 0 }, price = 8500, currency = "Cps", status = "Active" },
            new { id = 3, seller = "ShadowNinja", item = new { name = "Dragon Ball", rarity = "Epic", type = "Consumable", plus = 0, sockets = 0, quantity = 10 }, price = 1500, currency = "Gold", status = "Active" },
            new { id = 4, seller = "HolyMonk", item = new { name = "Heaven Fan +9", rarity = "Epic", type = "Weapon", plus = 9, sockets = 1 }, price = 12000, currency = "Cps", status = "Active" },
        };

        return JsonSerializer.Serialize(new { listings, count = listings.Length, market = "Twin City (178,182)", currencies = new[] { "Cps", "Gold" } });
    }

    private async Task<string> HandleMarketplaceListAsync(HttpListenerRequest request)
    {
        var body = await ReadBodyAsync(request);
        Console.WriteLine($"[Market] List item: {body}");

        return JsonSerializer.Serialize(new { success = true, message = "Item listed in Twin City Market", listingId = new Random().Next(100, 999) });
    }

    private async Task<string> HandleMarketplaceBuyAsync(HttpListenerRequest request)
    {
        var body = await ReadBodyAsync(request);
        Console.WriteLine($"[Market] Buy: {body}");

        return JsonSerializer.Serialize(new { success = true, message = "Purchase successful - Item added to inventory", transactionId = new Random().Next(1000, 9999) });
    }

    private string HandleAuctionLots(HttpListenerRequest request)
    {
        var lots = new[]
        {
            new { id = 1, seller = "DragonLord", item = new { name = "Dragon Blade +12 Super 2-Socket", rarity = "Super" }, currentBid = 48000, buyout = 75000, currency = "Cps", bids = 24, endsIn = "02:41:17" },
            new { id = 2, seller = "PirateKing", item = new { name = "Pirate Rapier +8", rarity = "Elite" }, currentBid = 8500, buyout = 15000, currency = "Gold", bids = 12, endsIn = "05:22:10" },
            new { id = 3, seller = "FireQueen", item = new { name = "Super Dragon Gem Pack x5", rarity = "Super" }, currentBid = 32000, buyout = 50000, currency = "Cps", bids = 8, endsIn = "11:40:55" },
        };

        return JsonSerializer.Serialize(new { lots, count = lots.Length, auctionHouse = "Twin City Auction House" });
    }

    private async Task<string> HandleAuctionBidAsync(HttpListenerRequest request)
    {
        var body = await ReadBodyAsync(request);
        Console.WriteLine($"[Auction] Bid: {body}");

        return JsonSerializer.Serialize(new { success = true, message = "Bid placed successfully", newBid = 48500, topBidder = "You" });
    }

    private async Task<string> HandleAuctionBuyoutAsync(HttpListenerRequest request)
    {
        var body = await ReadBodyAsync(request);
        Console.WriteLine($"[Auction] Buyout: {body}");

        return JsonSerializer.Serialize(new { success = true, message = "Buyout successful - Item added to inventory" });
    }

    private string HandleFriendsList(HttpListenerRequest request)
    {
        var friends = new[]
        {
            new { id = 1, name = "DragonLord", level = 130, mainClass = "Trojan", isOnline = true, location = "Twin City" },
            new { id = 2, name = "FireQueen", level = 125, mainClass = "Fire Taoist", isOnline = true, location = "Market" },
            new { id = 3, name = "ShadowNinja", level = 120, mainClass = "Ninja", isOnline = false, location = "Offline" },
        };

        return JsonSerializer.Serialize(new { friends, onlineCount = 2, totalCount = 3, voiceServer = "ws://localhost:8081" });
    }

    private async Task<string> HandleFriendsAddAsync(HttpListenerRequest request)
    {
        var body = await ReadBodyAsync(request);
        Console.WriteLine($"[Friends] Add: {body}");

        return JsonSerializer.Serialize(new { success = true, message = "Friend request sent - or added if player is mock" });
    }

    private async Task<string> HandleFriendsBlockAsync(HttpListenerRequest request)
    {
        var body = await ReadBodyAsync(request);
        Console.WriteLine($"[Friends] Block: {body}");

        return JsonSerializer.Serialize(new { success = true, message = "Player blocked - voice and trade blocked" });
    }

    private string HandleInventory(HttpListenerRequest request)
    {
        var items = new[]
        {
            new { id = 1, name = "Dragon Blade +12", type = "Weapon", rarity = "Legendary", plus = 12, sockets = 2, tradable = true, bound = false },
            new { id = 2, name = "Super Dragon Gem", type = "Gem", rarity = "Super", quantity = 5, tradable = true },
            new { id = 3, name = "Dragon Ball", type = "Consumable", rarity = "Epic", quantity = 27, tradable = true },
        };

        return JsonSerializer.Serialize(new { items, count = items.Length, owner = "DragonLord" });
    }

    private string HandleWallet(HttpListenerRequest request)
    {
        return JsonSerializer.Serialize(new
        {
            cps = 50000,
            gold = 1000000,
            silver = 500000,
            boundCps = 1000,
            currencies = new[]
            {
                new { type = "Cps", name = "Conquer Points", amount = 50000, icon = "Gem", description = "Premium currency for rare items, garments, mounts, lottery" },
                new { type = "Gold", name = "Gold", amount = 1000000, icon = "Gold", description = "Trade currency for marketplace and player vending" },
                new { type = "Silver", name = "Silver", amount = 500000, icon = "Silver", description = "Basic currency for potions, repairs" },
                new { type = "BoundCps", name = "Bound CPs", amount = 1000, icon = "Gem", description = "Non-tradable CPs from events" }
            }
        });
    }
}
