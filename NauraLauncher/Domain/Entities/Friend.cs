namespace NauraLauncher.Domain.Entities;

public enum FriendStatus
{
    Pending = 0,
    Accepted = 1,
    Blocked = 2,
    Declined = 3
}

public class Friendship
{
    public int Id { get; set; }
    public int UserId { get; set; }          // Requester
    public int FriendId { get; set; }        // Target
    public FriendStatus Status { get; set; } = FriendStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }

    // For display - denormalized
    public string FriendName { get; set; } = string.Empty;
    public string FriendAvatar { get; set; } = string.Empty;
    public bool FriendIsOnline { get; set; }
    public int FriendLevel { get; set; }
    public string FriendClass { get; set; } = string.Empty;
}

public class BlockedUser
{
    public int Id { get; set; }
    public int UserId { get; set; }          // Who blocked
    public int BlockedId { get; set; }       // Who is blocked
    public string BlockedName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
}

public class FriendRequest
{
    public int Id { get; set; }
    public int FromUserId { get; set; }
    public string FromUsername { get; set; } = string.Empty;
    public int ToUserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
