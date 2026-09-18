using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace NauraLauncher.Server.Realtime;

/// <summary>
/// Voice signaling server for Conquer Online launcher
/// Handles WebRTC signaling: SDP offers/answers, ICE candidates, call management
/// Clean architecture: Infrastructure layer, real-time communication
/// 
/// Protocol:
/// Client -> Server: JSON { type, channelId, callId, fromUserId, toUserId, payload }
/// Server -> Client: Same JSON forwarded to target
/// 
/// Types: call-incoming, call-accepted, call-declined, call-ended, offer, answer, ice-candidate, user-joined, user-left, mute, unmute
/// </summary>
public class VoiceSignalingServer
{
    private readonly int _port;
    private HttpListener? _listener;
    private readonly ConcurrentDictionary<int, WebSocket> _userSockets = new();
    private readonly ConcurrentDictionary<string, VoiceChannel> _channels = new();
    private readonly ConcurrentDictionary<string, VoiceCall> _calls = new();
    private CancellationTokenSource? _cts;

    public VoiceSignalingServer(int port = 8081)
    {
        _port = port;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://0.0.0.0:{_port}/");
        _listener.Start();

        _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        Console.WriteLine($"[Voice] Listening on port {_port}");
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        foreach (var ws in _userSockets.Values)
        {
            try { ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutdown", CancellationToken.None).Wait(); } catch { }
        }
        _userSockets.Clear();
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var context = await _listener!.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    _ = Task.Run(() => HandleWebSocketAsync(context, ct), ct);
                }
                else
                {
                    // Simple HTTP response for health check
                    context.Response.StatusCode = 200;
                    var buffer = Encoding.UTF8.GetBytes("NauraLauncher Voice Signaling Server - Conquer Online\nUse WebSocket to connect");
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.Close();
                }
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                Console.WriteLine($"[Voice] Accept error: {ex.Message}");
                await Task.Delay(1000, ct);
            }
        }
    }

    private async Task HandleWebSocketAsync(HttpListenerContext context, CancellationToken ct)
    {
        WebSocketContext? wsContext = null;
        try
        {
            wsContext = await context.AcceptWebSocketAsync(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Voice] WebSocket accept failed: {ex.Message}");
            context.Response.StatusCode = 500;
            context.Response.Close();
            return;
        }

        var webSocket = wsContext.WebSocket;
        int userId = 0;

        // Expect first message to be auth: { type: "auth", userId: 123, token: "..." }
        var buffer = new byte[4096];
        try
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "auth")
                {
                    if (doc.RootElement.TryGetProperty("userId", out var userIdProp))
                    {
                        userId = userIdProp.GetInt32();
                        _userSockets[userId] = webSocket;
                        Console.WriteLine($"[Voice] User {userId} connected for voice");

                        // Send ack
                        var ack = JsonSerializer.Serialize(new { type = "auth-ok", userId, message = "Voice connected - Twin City Voice Server" });
                        await webSocket.SendAsync(Encoding.UTF8.GetBytes(ack), WebSocketMessageType.Text, true, ct);
                    }
                }
            }
        }
        catch
        {
            userId = 0;
        }

        if (userId == 0)
        {
            // Allow anonymous for demo
            userId = new Random().Next(1000, 9999);
            _userSockets[userId] = webSocket;
        }

        // Message loop
        var msgBuffer = new byte[8192];
        var messageBuilder = new StringBuilder();

        while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(msgBuffer), ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", ct);
                    break;
                }

                messageBuilder.Append(Encoding.UTF8.GetString(msgBuffer, 0, result.Count));

                if (result.EndOfMessage)
                {
                    var messageJson = messageBuilder.ToString();
                    messageBuilder.Clear();

                    await ProcessSignalingMessageAsync(messageJson, userId, ct);
                }
            }
            catch (WebSocketException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Voice] Error for user {userId}: {ex.Message}");
                break;
            }
        }

        _userSockets.TryRemove(userId, out _);
        Console.WriteLine($"[Voice] User {userId} disconnected");

        // Clean up channels where user was participant
        foreach (var channel in _channels.Values.Where(c => c.ParticipantIds.Contains(userId)).ToList())
        {
            channel.ParticipantIds.Remove(userId);
            if (channel.ParticipantIds.Count == 0)
                _channels.TryRemove(channel.ChannelId, out _);
        }
    }

    private async Task ProcessSignalingMessageAsync(string json, int fromUserId, CancellationToken ct)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeProp)) return;
            var type = typeProp.GetString() ?? "";

            Console.WriteLine($"[Voice] {type} from {fromUserId}");

            // Parse common fields
            string channelId = root.TryGetProperty("channelId", out var chProp) ? chProp.GetString() ?? "" : "";
            string callId = root.TryGetProperty("callId", out var callProp) ? callProp.GetString() ?? "" : "";
            int toUserId = root.TryGetProperty("toUserId", out var toProp) ? toProp.GetInt32() : 0;
            string payload = root.TryGetProperty("payload", out var payloadProp) ? payloadProp.GetString() ?? "" : "";

            var message = new VoiceSignalingMessage
            {
                Type = type,
                ChannelId = channelId,
                CallId = callId,
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Payload = payload,
                Timestamp = DateTime.UtcNow
            };

            // Handle server-side logic
            switch (type)
            {
                case "create-channel":
                    var newChannel = new VoiceChannel
                    {
                        ChannelId = Guid.NewGuid().ToString(),
                        Type = VoiceChannelType.Party,
                        Name = payload,
                        CreatedBy = fromUserId,
                        ParticipantIds = new List<int> { fromUserId },
                        CreatedAt = DateTime.UtcNow
                    };
                    _channels[newChannel.ChannelId] = newChannel;
                    await SendToUserAsync(fromUserId, JsonSerializer.Serialize(new
                    {
                        type = "channel-created",
                        channelId = newChannel.ChannelId,
                        channel = newChannel
                    }), ct);
                    break;

                case "join-channel":
                    if (_channels.TryGetValue(channelId, out var channelToJoin))
                    {
                        if (!channelToJoin.ParticipantIds.Contains(fromUserId))
                            channelToJoin.ParticipantIds.Add(fromUserId);

                        // Notify all participants
                        await BroadcastToChannelAsync(channelId, JsonSerializer.Serialize(new
                        {
                            type = "user-joined",
                            channelId,
                            userId = fromUserId
                        }), ct, excludeUserId: fromUserId);
                    }
                    break;

                case "leave-channel":
                    if (_channels.TryGetValue(channelId, out var channelToLeave))
                    {
                        channelToLeave.ParticipantIds.Remove(fromUserId);
                        await BroadcastToChannelAsync(channelId, JsonSerializer.Serialize(new
                        {
                            type = "user-left",
                            channelId,
                            userId = fromUserId
                        }), ct);
                    }
                    break;

                case "initiate-call":
                    var newCall = new VoiceCall
                    {
                        CallId = Guid.NewGuid().ToString(),
                        ChannelId = Guid.NewGuid().ToString(),
                        CallerId = fromUserId,
                        CalleeId = toUserId,
                        Status = VoiceCallStatus.Ringing,
                        StartedAt = DateTime.UtcNow
                    };
                    _calls[newCall.CallId] = newCall;

                    var callChannel = new VoiceChannel
                    {
                        ChannelId = newCall.ChannelId,
                        Type = VoiceChannelType.Direct,
                        Name = $"Call {fromUserId} -> {toUserId}",
                        CreatedBy = fromUserId,
                        ParticipantIds = new List<int> { fromUserId }
                    };
                    _channels[callChannel.ChannelId] = callChannel;

                    await SendToUserAsync(toUserId, JsonSerializer.Serialize(new
                    {
                        type = "call-incoming",
                        callId = newCall.CallId,
                        channelId = newCall.ChannelId,
                        fromUserId,
                        payload = $"{{\"callerId\":{fromUserId}}}"
                    }), ct);

                    await SendToUserAsync(fromUserId, JsonSerializer.Serialize(new
                    {
                        type = "call-ringing",
                        callId = newCall.CallId,
                        channelId = newCall.ChannelId,
                        toUserId
                    }), ct);
                    break;

                case "accept-call":
                    if (_calls.TryGetValue(callId, out var callToAccept))
                    {
                        callToAccept.Status = VoiceCallStatus.Active;
                        callToAccept.AnsweredAt = DateTime.UtcNow;
                        if (_channels.TryGetValue(callToAccept.ChannelId, out var callCh))
                        {
                            if (!callCh.ParticipantIds.Contains(fromUserId))
                                callCh.ParticipantIds.Add(fromUserId);
                        }

                        await SendToUserAsync(callToAccept.CallerId, JsonSerializer.Serialize(new
                        {
                            type = "call-accepted",
                            callId,
                            channelId = callToAccept.ChannelId,
                            fromUserId
                        }), ct);
                    }
                    break;

                case "decline-call":
                case "end-call":
                    if (_calls.TryGetValue(callId, out var callToEnd))
                    {
                        callToEnd.Status = type == "decline-call" ? VoiceCallStatus.Declined : VoiceCallStatus.Ended;
                        callToEnd.EndedAt = DateTime.UtcNow;

                        var otherId = callToEnd.CallerId == fromUserId ? callToEnd.CalleeId : callToEnd.CallerId;
                        await SendToUserAsync(otherId, JsonSerializer.Serialize(new
                        {
                            type = type == "decline-call" ? "call-declined" : "call-ended",
                            callId,
                            channelId = callToEnd.ChannelId,
                            fromUserId
                        }), ct);

                        _channels.TryRemove(callToEnd.ChannelId, out _);
                        _calls.TryRemove(callId, out _);
                    }
                    break;

                case "offer":
                case "answer":
                case "ice-candidate":
                case "mute":
                case "unmute":
                    // Forward to target user
                    if (toUserId != 0)
                    {
                        await SendToUserAsync(toUserId, json, ct);
                    }
                    else if (!string.IsNullOrEmpty(channelId))
                    {
                        await BroadcastToChannelAsync(channelId, json, ct, excludeUserId: fromUserId);
                    }
                    break;

                default:
                    // Forward unknown types to target if specified
                    if (toUserId != 0)
                        await SendToUserAsync(toUserId, json, ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Voice] Failed to process message: {ex.Message}");
        }
    }

    private async Task SendToUserAsync(int userId, string json, CancellationToken ct)
    {
        if (_userSockets.TryGetValue(userId, out var socket) && socket.State == WebSocketState.Open)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Voice] Failed to send to {userId}: {ex.Message}");
            }
        }
    }

    private async Task BroadcastToChannelAsync(string channelId, string json, CancellationToken ct, int excludeUserId = 0)
    {
        if (!_channels.TryGetValue(channelId, out var channel)) return;

        foreach (var participantId in channel.ParticipantIds)
        {
            if (participantId == excludeUserId) continue;
            await SendToUserAsync(participantId, json, ct);
        }
    }
}

public class VoiceChannel
{
    public string ChannelId { get; set; } = string.Empty;
    public VoiceChannelType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<int> ParticipantIds { get; set; } = new();
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class VoiceCall
{
    public string CallId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public int CallerId { get; set; }
    public int CalleeId { get; set; }
    public VoiceCallStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public DateTime? EndedAt { get; set; }
}

public class VoiceSignalingMessage
{
    public string Type { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string CallId { get; set; } = string.Empty;
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public enum VoiceCallStatus
{
    Initiating = 0,
    Ringing = 1,
    Active = 2,
    Ended = 3,
    Declined = 4,
    Failed = 5
}

public enum VoiceChannelType
{
    Direct = 0,
    Party = 1,
    Guild = 2,
    Team = 3
}
