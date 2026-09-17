using System;
using System.Threading.Tasks;

namespace NauraLauncher.Core.Interfaces;

public interface IRealtimeWebSocketService
{
    bool IsConnected { get; }
    double CurrentLatencyMs { get; }

    Task ConnectAsync(string url);
    Task DisconnectAsync();
    Task SubscribeAsync(string topic);
    Task UnsubscribeAsync(string topic);
    Task SendAsync<T>(T payload);

    event EventHandler<bool>? ConnectionChanged;
    event EventHandler<double>? LatencyUpdated;
    event EventHandler<(string Topic, string MessageJson)>? MessageReceived;
}
