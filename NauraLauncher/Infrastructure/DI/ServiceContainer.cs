using System;
using NauraLauncher.Core.Interfaces;
using NauraLauncher.Infrastructure.Database;
using NauraLauncher.Infrastructure.Network;
using NauraLauncher.Infrastructure.Security;
using NauraLauncher.Infrastructure.Services;

namespace NauraLauncher.Infrastructure.DI;

/// <summary>
/// Clean Architecture service composition root.
/// </summary>
public static class ServiceContainer
{
    private static readonly Lazy<ISecurityService> _security = new(() => new CryptoService());
    private static readonly Lazy<IApiClient> _apiClient = new(() => new SecureApiClient("https://127.0.0.1:8443"));
    private static readonly Lazy<IRealtimeWebSocketService> _webSocket = new(() => new RealtimeWebSocketService());
    private static readonly Lazy<IAuthService> _auth = new(() => new AuthService(ApiClient, Security));
    private static readonly Lazy<IGameLibraryService> _library = new(() => new GameLibraryService(ApiClient, Auth));
    private static readonly Lazy<IGameProcessLauncher> _gameLauncher = new(() => new GameProcessLauncher(ApiClient, Auth));
    private static readonly Lazy<IDownloadPatcherService> _downloader = new(() => new DownloadPatcherService(Security));
    private static readonly Lazy<IAuctionService> _auction = new(() => new AuctionService(ApiClient, WebSocket, Auth));
    private static readonly Lazy<ISettingsService> _settings = new(() => new SettingsService(ApiClient));
    private static readonly Lazy<IMySqlService> _mySql = new(() => new MySqlService());

    public static ISecurityService Security => _security.Value;
    public static IApiClient ApiClient => _apiClient.Value;
    public static IRealtimeWebSocketService WebSocket => _webSocket.Value;
    public static IAuthService Auth => _auth.Value;
    public static IGameLibraryService Library => _library.Value;
    public static IGameProcessLauncher GameLauncher => _gameLauncher.Value;
    public static IDownloadPatcherService Downloader => _downloader.Value;
    public static IAuctionService Auction => _auction.Value;
    public static ISettingsService Settings => _settings.Value;
    public static IMySqlService MySql => _mySql.Value;

    public static void Initialize()
    {
        // Kick off background connections
        _ = WebSocket.ConnectAsync("wss://127.0.0.1:8443/ws");
    }
}
