using NauraLauncher.Server.Api;
using NauraLauncher.Server.Infrastructure;
using NauraLauncher.Server.Realtime;

Console.WriteLine("==============================================");
Console.WriteLine("  NauraLauncher Server - Conquer Online");
Console.WriteLine("  Version 2.4.0 - Twin City Market Server");
Console.WriteLine("==============================================");
Console.WriteLine();

// Check for MySQL compatibility issues and report
Console.WriteLine("[INFO] Checking MySQL 5.6 compatibility...");
MySqlSchemaValidator.Validate();
Console.WriteLine("[OK] MySQL 5.6 compatibility check passed\n");

// Initialize database
var dbConfig = new DatabaseConfig
{
    ConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION") ?? "Server=localhost;Port=3306;Database=nauralauncher;Uid=root;Pwd=;",
    UseInMemory = true // For demo - set false to use real MySQL
};

var database = new DatabaseService(dbConfig);
await database.InitializeAsync();

// Start voice signaling server
var voiceServer = new VoiceSignalingServer(port: 8081);
voiceServer.Start();

Console.WriteLine($"[Voice] Signaling server started on ws://0.0.0.0:8081");
Console.WriteLine($"[Voice] Handles: call initiation, WebRTC SDP exchange, ICE candidates, channel management");
Console.WriteLine($"[Voice] Protocol: JSON over WebSocket - compatible with Conquer Online voice chat");
Console.WriteLine();

// Start API server (simple HTTP)
var apiServer = new ApiServer(port: 8080, database: database, voiceServer: voiceServer);
apiServer.Start();

Console.WriteLine($"[API] REST API server started on http://0.0.0.0:8080");
Console.WriteLine($"[API] Endpoints:");
Console.WriteLine($"      POST /api/auth/login - Login with username/password");
Console.WriteLine($"      POST /api/auth/register - Register new hero");
Console.WriteLine($"      GET  /api/marketplace/listings - Browse Twin City Market");
Console.WriteLine($"      POST /api/marketplace/list - List item from inventory");
Console.WriteLine($"      POST /api/marketplace/buy - Buy item (CPs/Gold flow)");
Console.WriteLine($"      GET  /api/auction/lots - Browse auction house");
Console.WriteLine($"      POST /api/auction/bid - Place bid");
Console.WriteLine($"      POST /api/auction/buyout - Instant buyout");
Console.WriteLine($"      GET  /api/friends - Friend list");
Console.WriteLine($"      POST /api/friends/add - Add friend");
Console.WriteLine($"      POST /api/friends/block - Block/unblock");
Console.WriteLine($"      GET  /api/inventory - Player inventory");
Console.WriteLine($"      GET  /api/wallet - Wallet (CPs, Gold, Silver)");
Console.WriteLine($"      WS   /ws/voice - Voice signaling WebSocket");
Console.WriteLine();

Console.WriteLine("[INFO] Server running. Press Ctrl+C to stop.");
Console.WriteLine("[INFO] For Conquer Online integration:");
Console.WriteLine("       - Launcher connects to this server for market/auction data");
Console.WriteLine("       - Voice calls use WebSocket signaling + WebRTC for P2P audio");
Console.WriteLine("       - Friends system with add/remove/block handled via REST");
Console.WriteLine();

// Keep running
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("\n[INFO] Shutting down servers...");
    voiceServer.Stop();
    apiServer.Stop();
    Console.WriteLine("[OK] Servers stopped");
};

try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException) { }

Console.WriteLine("[INFO] Server shutdown complete");
