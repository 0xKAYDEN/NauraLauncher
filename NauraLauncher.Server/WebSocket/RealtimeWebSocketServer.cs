using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NauraLauncher.Server.Database;

namespace NauraLauncher.Server.WebSocket;

/// <summary>
/// Production C# WebSocket Server supporting real-time event broadcasting,
/// ping-pong latency calculation, topic subscriptions, and auction floor updates.
/// </summary>
public class RealtimeWebSocketServer
{
    private readonly TopicBroker _broker = new();
    private readonly ConcurrentDictionary<string, WebSocketClientConnection> _clients = new();
    private readonly MySqlDatabase _db;

    public int ConnectedClientsCount => _clients.Count;

    public RealtimeWebSocketServer(MySqlDatabase db)
    {
        _db = db;
    }

    public async Task HandleWebSocketContextAsync(HttpListenerWebSocketContext context, CancellationToken ct)
    {
        var client = new WebSocketClientConnection(context.WebSocket);
        _clients[client.Id] = client;
        _broker.Subscribe("global", client);

        client.MessageReceived += async (s, msg) => await HandleClientMessageAsync(client, msg);
        client.Closed += (s, e) =>
        {
            _clients.TryRemove(client.Id, out _);
            _broker.UnsubscribeAll(client);
        };

        await client.ListenAsync(ct);
    }

    private async Task HandleClientMessageAsync(WebSocketClientConnection client, string messageJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(messageJson);
            var root = doc.RootElement;

            string type = root.TryGetProperty("type", out var typeProp) ? typeProp.GetString() ?? "" : "";
            string action = root.TryGetProperty("action", out var actProp) ? actProp.GetString() ?? "" : "";

            // Heartbeat ping-pong
            if (type.Equals("PING", StringComparison.OrdinalIgnoreCase) || action.Equals("ping", StringComparison.OrdinalIgnoreCase))
            {
                long ts = root.TryGetProperty("timestamp", out var tsProp) ? tsProp.GetInt64() : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                await client.SendAsync(new
                {
                    type = "PONG",
                    timestamp = ts,
                    serverTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
                return;
            }

            // Topic subscription
            if (type.Equals("SUBSCRIBE", StringComparison.OrdinalIgnoreCase) || action.Equals("subscribe", StringComparison.OrdinalIgnoreCase))
            {
                string topic = root.TryGetProperty("topic", out var topProp) ? topProp.GetString() ?? "" : "";
                if (!string.IsNullOrEmpty(topic))
                {
                    _broker.Subscribe(topic, client);
                    await client.SendAsync(new { type = "SUBSCRIBED", topic, status = "OK" });
                }
                return;
            }

            // Topic unsubscription
            if (type.Equals("UNSUBSCRIBE", StringComparison.OrdinalIgnoreCase) || action.Equals("unsubscribe", StringComparison.OrdinalIgnoreCase))
            {
                string topic = root.TryGetProperty("topic", out var topProp) ? topProp.GetString() ?? "" : "";
                if (!string.IsNullOrEmpty(topic))
                {
                    _broker.Unsubscribe(topic, client);
                    await client.SendAsync(new { type = "UNSUBSCRIBED", topic, status = "OK" });
                }
                return;
            }
        }
        catch (Exception ex)
        {
            await client.SendAsync(new { type = "ERROR", message = ex.Message });
        }
    }

    public async Task BroadcastAsync<T>(string topic, T message)
    {
        var subs = _broker.GetSubscribers(topic);
        foreach (var sub in subs)
        {
            await sub.SendAsync(message);
        }
    }

    public async Task BroadcastGlobalAsync<T>(T message)
    {
        foreach (var client in _clients.Values)
        {
            await client.SendAsync(message);
        }
    }
}
