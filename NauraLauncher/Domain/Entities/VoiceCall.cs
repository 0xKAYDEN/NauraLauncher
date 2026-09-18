namespace NauraLauncher.Domain.Entities;

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
    Direct = 0,      // 1-to-1 call
    Party = 1,       // Party voice
    Guild = 2,       // Guild voice
    Team = 3         // Team voice
}

public class VoiceChannel
{
    public string ChannelId { get; set; } = Guid.NewGuid().ToString();
    public VoiceChannelType Type { get; set; } = VoiceChannelType.Direct;
    public string Name { get; set; } = string.Empty;
    public List<int> ParticipantIds { get; set; } = new();
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

public class VoiceCall
{
    public string CallId { get; set; } = Guid.NewGuid().ToString();
    public string ChannelId { get; set; } = string.Empty;
    public int CallerId { get; set; }
    public string CallerName { get; set; } = string.Empty;
    public int CalleeId { get; set; }
    public string CalleeName { get; set; } = string.Empty;
    public VoiceCallStatus Status { get; set; } = VoiceCallStatus.Initiating;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AnsweredAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? EndReason { get; set; }
}

public class VoiceSignalingMessage
{
    public string Type { get; set; } = string.Empty; // offer, answer, ice-candidate, join, leave, mute, etc.
    public string ChannelId { get; set; } = string.Empty;
    public string CallId { get; set; } = string.Empty;
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public string Payload { get; set; } = string.Empty; // JSON SDP or ICE
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
