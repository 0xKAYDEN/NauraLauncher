using System.Net.Http;
using System.Text;
using System.Text.Json;
using NauraLauncher.Domain.Entities;

namespace NauraLauncher.Services;

/// <summary>
/// HTTP client for NauraLauncher Server - Conquer Online backend
/// Clean architecture: Infrastructure service, abstracts HTTP calls
/// Handles auth, marketplace, auction, friends, inventory, wallet
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string? _authToken;

    public ApiClient(string baseUrl = "http://localhost:8080")
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "NauraLauncher/2.4.0 ConquerOnline");
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    // Auth
    public async Task<(bool success, User? user, string message)> LoginAsync(string username, string password)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { username, password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/auth/login", content);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var success = doc.RootElement.GetProperty("success").GetBoolean();
            var message = doc.RootElement.TryGetProperty("message", out var msgProp) ? msgProp.GetString() ?? "" : "";

            // In real implementation, parse user from JSON
            // For demo, return mock user
            return (success, null, message);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool success, User? user, string message)> RegisterAsync(string username, string email, string displayName, string password)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { username, email, displayName, password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/auth/register", content);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var success = doc.RootElement.GetProperty("success").GetBoolean();
            var message = doc.RootElement.TryGetProperty("message", out var msgProp) ? msgProp.GetString() ?? "" : "";

            return (success, null, message);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    // Marketplace
    public async Task<string> GetMarketplaceListingsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/marketplace/listings");
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return $"{{\"error\":\"{ex.Message}\"}}";
        }
    }

    // Wallet
    public async Task<string> GetWalletAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/wallet?userId={userId}");
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return $"{{\"error\":\"{ex.Message}\"}}";
        }
    }

    // Health check
    public async Task<bool> IsServerOnlineAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Voice client - handles WebSocket connection to voice signaling server
/// For Conquer Online voice chat between friends, party, guild
/// </summary>
public class VoiceClient
{
    private readonly string _voiceServerUrl;
    private System.Net.WebSockets.ClientWebSocket? _webSocket;
    private int _userId;

    public event EventHandler<string>? OnMessageReceived;
    public event EventHandler? OnConnected;
    public event EventHandler? OnDisconnected;

    public VoiceClient(string voiceServerUrl = "ws://localhost:8081")
    {
        _voiceServerUrl = voiceServerUrl;
    }

    public async Task ConnectAsync(int userId, string token = "")
    {
        _userId = userId;
        _webSocket = new System.Net.WebSockets.ClientWebSocket();
        await _webSocket.ConnectAsync(new Uri(_voiceServerUrl), CancellationToken.None);

        // Send auth
        var authJson = JsonSerializer.Serialize(new { type = "auth", userId, token });
        var bytes = Encoding.UTF8.GetBytes(authJson);
        await _webSocket.SendAsync(bytes, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);

        OnConnected?.Invoke(this, EventArgs.Empty);

        // Start receive loop
        _ = Task.Run(ReceiveLoopAsync);
    }

    public async Task SendSignalingAsync(string type, string channelId, string callId, int toUserId, string payload)
    {
        if (_webSocket == null || _webSocket.State != System.Net.WebSockets.WebSocketState.Open) return;

        var message = JsonSerializer.Serialize(new
        {
            type,
            channelId,
            callId,
            fromUserId = _userId,
            toUserId,
            payload
        });

        var bytes = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(bytes, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task InitiateCallAsync(int calleeId)
    {
        await SendSignalingAsync("initiate-call", "", "", calleeId, "");
    }

    public async Task AcceptCallAsync(string callId)
    {
        await SendSignalingAsync("accept-call", "", callId, 0, "");
    }

    public async Task EndCallAsync(string callId)
    {
        await SendSignalingAsync("end-call", "", callId, 0, "");
    }

    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[8192];
        try
        {
            while (_webSocket != null && _webSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                    break;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                OnMessageReceived?.Invoke(this, json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VoiceClient] Receive error: {ex.Message}");
        }
        finally
        {
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task DisconnectAsync()
    {
        if (_webSocket != null)
        {
            await _webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
            _webSocket.Dispose();
            _webSocket = null;
        }
    }
}
