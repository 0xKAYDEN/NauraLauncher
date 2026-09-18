using NauraLauncher.Domain.Entities;

namespace NauraLauncher.Application.Interfaces;

public interface IVoiceService
{
    // Channel management
    Task<VoiceChannel> CreateChannelAsync(int creatorId, VoiceChannelType type, string name, List<int> participantIds);
    Task<VoiceChannel?> GetChannelAsync(string channelId);
    Task<List<VoiceChannel>> GetUserChannelsAsync(int userId);
    Task<bool> JoinChannelAsync(string channelId, int userId);
    Task<bool> LeaveChannelAsync(string channelId, int userId);
    Task<bool> CloseChannelAsync(string channelId, int userId);

    // Call management
    Task<VoiceCall> InitiateCallAsync(int callerId, int calleeId);
    Task<VoiceCall?> GetCallAsync(string callId);
    Task<bool> AcceptCallAsync(string callId, int userId);
    Task<bool> DeclineCallAsync(string callId, int userId);
    Task<bool> EndCallAsync(string callId, int userId, string reason = "");

    // Signaling
    event EventHandler<VoiceSignalingMessage>? OnSignalingMessage;
    Task SendSignalingMessageAsync(VoiceSignalingMessage message);
}
