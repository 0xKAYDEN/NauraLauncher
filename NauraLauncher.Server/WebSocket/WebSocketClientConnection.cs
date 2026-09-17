using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NauraLauncher.Server.WebSocket;

/// <summary>
/// Encapsulates a connected WebSocket client session with thread-safe sending
/// and frame processing.
/// </summary>
public class WebSocketClientConnection
{
    private readonly System.Net.WebSockets.WebSocket _webSocket;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public bool IsAlive { get; set; } = true;
    public string? UserId { get; set; }

    public event EventHandler<string>? MessageReceived;
    public event EventHandler? Closed;

    public WebSocketClientConnection(System.Net.WebSockets.WebSocket webSocket)
    {
        _webSocket = webSocket;
    }

    public async Task ListenAsync(CancellationToken ct)
    {
        var buffer = new byte[8192];
        using var ms = new MemoryStream();

        try
        {
            while (!ct.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                ms.SetLength(0);

                do
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing");
                        return;
                    }
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                IsAlive = true;
                string text = Encoding.UTF8.GetString(ms.ToArray());
                MessageReceived?.Invoke(this, text);
            }
        }
        catch (OperationCanceledException) { }
        catch { }
        finally
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task SendAsync<T>(T payload)
    {
        if (_webSocket.State != WebSocketState.Open) return;

        string json = JsonSerializer.Serialize(payload);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        await _sendLock.WaitAsync();
        try
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        catch { }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task CloseAsync(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure, string reason = "")
    {
        try
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(status, reason, CancellationToken.None);
            }
        }
        catch { }
    }
}
