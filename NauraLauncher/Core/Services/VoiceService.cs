using NauraLauncher.Core.Interfaces;
using NauraLauncher.Domain.Entities;

namespace NauraLauncher.Core.Services;

/// <summary>
/// Voice service - handles voice call signaling and channel management
/// Clean architecture: Application service, can be backed by WebSocket server
/// </summary>
public class VoiceService : IVoiceService
{
    private static readonly Dictionary<string, VoiceChannel> _channels = new();
    private static readonly Dictionary<string, VoiceCall> _calls = new();

    public event EventHandler<VoiceSignalingMessage>? OnSignalingMessage;

    public Task<VoiceChannel> CreateChannelAsync(int creatorId, VoiceChannelType type, string name, List<int> participantIds)
    {
        var channel = new VoiceChannel
        {
            ChannelId = Guid.NewGuid().ToString(),
            Type = type,
            Name = name,
            CreatedBy = creatorId,
            ParticipantIds = new List<int>(participantIds) { creatorId }.Distinct().ToList(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _channels[channel.ChannelId] = channel;
        return Task.FromResult(channel);
    }

    public Task<VoiceChannel?> GetChannelAsync(string channelId)
    {
        _channels.TryGetValue(channelId, out var channel);
        return Task.FromResult(channel);
    }

    public Task<List<VoiceChannel>> GetUserChannelsAsync(int userId)
    {
        var result = _channels.Values.Where(c => c.ParticipantIds.Contains(userId) && c.IsActive).ToList();
        return Task.FromResult(result);
    }

    public Task<bool> JoinChannelAsync(string channelId, int userId)
    {
        if (!_channels.TryGetValue(channelId, out var channel)) return Task.FromResult(false);
        if (!channel.ParticipantIds.Contains(userId))
            channel.ParticipantIds.Add(userId);

        // Notify others
        OnSignalingMessage?.Invoke(this, new VoiceSignalingMessage
        {
            Type = "user-joined",
            ChannelId = channelId,
            FromUserId = userId,
            Payload = $"{{\"userId\":{userId},\"action\":\"joined\"}}"
        });

        return Task.FromResult(true);
    }

    public Task<bool> LeaveChannelAsync(string channelId, int userId)
    {
        if (!_channels.TryGetValue(channelId, out var channel)) return Task.FromResult(false);
        channel.ParticipantIds.Remove(userId);

        OnSignalingMessage?.Invoke(this, new VoiceSignalingMessage
        {
            Type = "user-left",
            ChannelId = channelId,
            FromUserId = userId,
            Payload = $"{{\"userId\":{userId},\"action\":\"left\"}}"
        });

        if (channel.ParticipantIds.Count == 0)
            channel.IsActive = false;

        return Task.FromResult(true);
    }

    public Task<bool> CloseChannelAsync(string channelId, int userId)
    {
        if (!_channels.TryGetValue(channelId, out var channel)) return Task.FromResult(false);
        if (channel.CreatedBy != userId) return Task.FromResult(false);

        channel.IsActive = false;
        _channels.Remove(channelId);
        return Task.FromResult(true);
    }

    public Task<VoiceCall> InitiateCallAsync(int callerId, int calleeId)
    {
        var caller = AuthService.GetAllUsers().FirstOrDefault(u => u.Id == callerId);
        var callee = AuthService.GetAllUsers().FirstOrDefault(u => u.Id == calleeId);

        if (caller == null || callee == null)
            throw new InvalidOperationException("User not found");

        // Check if callee is blocked
        var friendsService = new FriendsService();
        // In real implementation, check block status async

        var call = new VoiceCall
        {
            CallId = Guid.NewGuid().ToString(),
            ChannelId = Guid.NewGuid().ToString(),
            CallerId = callerId,
            CallerName = caller.DisplayName,
            CalleeId = calleeId,
            CalleeName = callee.DisplayName,
            Status = VoiceCallStatus.Ringing,
            StartedAt = DateTime.UtcNow
        };

        _calls[call.CallId] = call;

        // Create a direct channel for the call
        var channel = new VoiceChannel
        {
            ChannelId = call.ChannelId,
            Type = VoiceChannelType.Direct,
            Name = $"Call {caller.DisplayName} -> {callee.DisplayName}",
            CreatedBy = callerId,
            ParticipantIds = new List<int> { callerId },
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _channels[channel.ChannelId] = channel;

        OnSignalingMessage?.Invoke(this, new VoiceSignalingMessage
        {
            Type = "call-incoming",
            ChannelId = call.ChannelId,
            CallId = call.CallId,
            FromUserId = callerId,
            ToUserId = calleeId,
            Payload = $"{{\"caller\":\"{caller.DisplayName}\",\"callId\":\"{call.CallId}\"}}"
        });

        return Task.FromResult(call);
    }

    public Task<VoiceCall?> GetCallAsync(string callId)
    {
        _calls.TryGetValue(callId, out var call);
        return Task.FromResult(call);
    }

    public Task<bool> AcceptCallAsync(string callId, int userId)
    {
        if (!_calls.TryGetValue(callId, out var call)) return Task.FromResult(false);
        if (call.CalleeId != userId) return Task.FromResult(false);

        call.Status = VoiceCallStatus.Active;
        call.AnsweredAt = DateTime.UtcNow;

        // Add callee to channel
        if (_channels.TryGetValue(call.ChannelId, out var channel))
        {
            if (!channel.ParticipantIds.Contains(userId))
                channel.ParticipantIds.Add(userId);
        }

        OnSignalingMessage?.Invoke(this, new VoiceSignalingMessage
        {
            Type = "call-accepted",
            ChannelId = call.ChannelId,
            CallId = call.CallId,
            FromUserId = userId,
            ToUserId = call.CallerId,
            Payload = $"{{\"callId\":\"{call.CallId}\",\"accepted\":true}}"
        });

        return Task.FromResult(true);
    }

    public Task<bool> DeclineCallAsync(string callId, int userId)
    {
        if (!_calls.TryGetValue(callId, out var call)) return Task.FromResult(false);
        if (call.CalleeId != userId) return Task.FromResult(false);

        call.Status = VoiceCallStatus.Declined;
        call.EndedAt = DateTime.UtcNow;
        call.EndReason = "Declined";

        OnSignalingMessage?.Invoke(this, new VoiceSignalingMessage
        {
            Type = "call-declined",
            ChannelId = call.ChannelId,
            CallId = call.CallId,
            FromUserId = userId,
            ToUserId = call.CallerId,
            Payload = $"{{\"callId\":\"{call.CallId}\",\"declined\":true}}"
        });

        // Clean up channel
        _channels.Remove(call.ChannelId);

        return Task.FromResult(true);
    }

    public Task<bool> EndCallAsync(string callId, int userId, string reason = "")
    {
        if (!_calls.TryGetValue(callId, out var call)) return Task.FromResult(false);
        if (call.CallerId != userId && call.CalleeId != userId) return Task.FromResult(false);

        call.Status = VoiceCallStatus.Ended;
        call.EndedAt = DateTime.UtcNow;
        call.EndReason = reason;

        var otherId = call.CallerId == userId ? call.CalleeId : call.CallerId;

        OnSignalingMessage?.Invoke(this, new VoiceSignalingMessage
        {
            Type = "call-ended",
            ChannelId = call.ChannelId,
            CallId = call.CallId,
            FromUserId = userId,
            ToUserId = otherId,
            Payload = $"{{\"callId\":\"{call.CallId}\",\"reason\":\"{reason}\"}}"
        });

        _channels.Remove(call.ChannelId);

        return Task.FromResult(true);
    }

    public Task SendSignalingMessageAsync(VoiceSignalingMessage message)
    {
        // In real server, this would forward via WebSocket to target user
        // For demo, just raise event
        message.Timestamp = DateTime.UtcNow;
        OnSignalingMessage?.Invoke(this, message);
        return Task.CompletedTask;
    }
}
