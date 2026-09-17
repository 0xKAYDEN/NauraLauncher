using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NauraLauncher.Server.WebSocket;

/// <summary>
/// Pub/Sub topic subscription broker managing client channel distribution.
/// </summary>
public class TopicBroker
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocketClientConnection>> _subscriptions = new();

    public void Subscribe(string topic, WebSocketClientConnection client)
    {
        var set = _subscriptions.GetOrAdd(topic, _ => new ConcurrentDictionary<string, WebSocketClientConnection>());
        set[client.Id] = client;
    }

    public void Unsubscribe(string topic, WebSocketClientConnection client)
    {
        if (_subscriptions.TryGetValue(topic, out var set))
        {
            set.TryRemove(client.Id, out _);
        }
    }

    public void UnsubscribeAll(WebSocketClientConnection client)
    {
        foreach (var set in _subscriptions.Values)
        {
            set.TryRemove(client.Id, out _);
        }
    }

    public IEnumerable<WebSocketClientConnection> GetSubscribers(string topic)
    {
        if (_subscriptions.TryGetValue(topic, out var set))
        {
            return set.Values;
        }
        return Array.Empty<WebSocketClientConnection>();
    }
}
