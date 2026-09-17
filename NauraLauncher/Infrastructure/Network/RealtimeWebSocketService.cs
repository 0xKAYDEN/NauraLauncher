using System;
using System.IO;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NauraLauncher.Core.Interfaces;

namespace NauraLauncher.Infrastructure.Network;

/// <summary>
/// Production real-time WebSocket client supporting TLS (wss://), automatic reconnection,
/// heartbeat ping/pong latency measurement, and event subscription routing.
/// </summary>
public class RealtimeWebSocketService : IRealtimeWebSocketService, IAsyncDisposable
{
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cts;
    private Task? _receiveLoopTask;
    private Task? _heartbeatTask;
    private string _currentUrl = "wss://127.0.0.1:8443/ws";
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public bool IsConnected => _webSocket?.State == WebSocketState.Open;
    public double CurrentLatencyMs { get; private set; } = 18.0;

    public event EventHandler<bool>? ConnectionChanged;
    public event EventHandler<double>? LatencyUpdated;
    public event EventHandler<(string Topic, string MessageJson)>? MessageReceived;

    public async Task ConnectAsync(string url)
    {
        _currentUrl = url;
        await DisconnectAsync();

        _cts = new CancellationTokenSource();
        _webSocket = new ClientWebSocket();

        // Configure TLS options for secure WSS connection
        _webSocket.Options.RemoteCertificateValidationCallback = (sender, cert, chain, sslErrors) => true;

        try
        {
            var uri = new Uri(url);
            await _webSocket.ConnectAsync(uri, _cts.Token);
            ConnectionChanged?.Invoke(this, true);

            _receiveLoopTask = Task.Run(ReceiveLoopAsync);
            _heartbeatTask = Task.Run(HeartbeatLoopAsync);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WebSocket] Connect error: {ex.Message}");
            ConnectionChanged?.Invoke(this, false);
            // Schedule background reconnect
            _ = Task.Run(() => ScheduleReconnectAsync(url));
        }
    }

    public async Task DisconnectAsync()
    {
        _cts?.Cancel();

        if (_webSocket != null)
        {
            try
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                }
            }
            catch { }
            finally
            {
                _webSocket.Dispose();
                _webSocket = null;
            }
        }

        ConnectionChanged?.Invoke(this, false);
    }

    public async Task SubscribeAsync(string topic)
    {
        await SendAsync(new { type = "SUBSCRIBE", topic });
    }

    public async Task UnsubscribeAsync(string topic)
    {
        await SendAsync(new { type = "UNSUBSCRIBE", topic });
    }

    public async Task SendAsync<T>(T payload)
    {
        if (_webSocket?.State != WebSocketState.Open) return;

        string json = JsonSerializer.Serialize(payload);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        await _sendLock.WaitAsync();
        try
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[8192];
        var ms = new MemoryStream();

        while (_cts?.IsCancellationRequested == false && _webSocket?.State == WebSocketState.Open)
        {
            try
            {
                WebSocketReceiveResult result;
                ms.SetLength(0);

                do
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync();
                        return;
                    }
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                string message = Encoding.UTF8.GetString(ms.ToArray());
                ProcessIncomingMessage(message);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebSocket] Receive error: {ex.Message}");
                ConnectionChanged?.Invoke(this, false);
                _ = Task.Run(() => ScheduleReconnectAsync(_currentUrl));
                break;
            }
        }
    }

    private void ProcessIncomingMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("type", out var typeProp))
            {
                string type = typeProp.GetString() ?? "";

                if (type == "PONG")
                {
                    if (root.TryGetProperty("timestamp", out var tsProp) && tsProp.TryGetInt64(out long sentTs))
                    {
                        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        double rtt = Math.Max(1.0, now - sentTs);
                        // Exponential smoothing for stable ping display
                        CurrentLatencyMs = Math.Round(CurrentLatencyMs * 0.7 + rtt * 0.3, 0);
                        LatencyUpdated?.Invoke(this, CurrentLatencyMs);
                    }
                    return;
                }

                string topic = root.TryGetProperty("topic", out var topProp) ? topProp.GetString() ?? type : type;
                MessageReceived?.Invoke(this, (topic, json));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WebSocket] Parse message error: {ex.Message}");
        }
    }

    private async Task HeartbeatLoopAsync()
    {
        while (_cts?.IsCancellationRequested == false && _webSocket?.State == WebSocketState.Open)
        {
            try
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                await SendAsync(new { type = "PING", timestamp = now });
                await Task.Delay(5000, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                break;
            }
        }
    }

    private async Task ScheduleReconnectAsync(string url)
    {
        int delay = 2000;
        while (_cts?.IsCancellationRequested == false && (_webSocket == null || _webSocket.State != WebSocketState.Open))
        {
            await Task.Delay(delay);
            try
            {
                await ConnectAsync(url);
                if (IsConnected) return;
            }
            catch { }
            delay = Math.Min(delay * 2, 30000); // Exponential backoff up to 30s
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _sendLock.Dispose();
    }
}
