using NauraLauncher.Core.Interfaces;
using NauraLauncher.Domain.Entities;

namespace NauraLauncher.Core.Services;

public class FriendsService : IFriendsService
{
    private static readonly List<Friendship> _friendships = new();
    private static readonly List<BlockedUser> _blocked = new();
    private static readonly List<FriendRequest> _requests = new();
    private static int _nextFriendId = 1;
    private static int _nextBlockId = 1;
    private static int _nextRequestId = 1;

    static FriendsService()
    {
        var users = AuthService.GetAllUsers();
        // Seed friendships for first user (admin) with mock users
        var admin = users.FirstOrDefault(u => u.Username == "admin");
        if (admin != null)
        {
            var mockFriends = MockDataService.GenerateMockFriends(admin.Id, users.Where(u => u.Id != admin.Id).ToList());
            _friendships.AddRange(mockFriends);
            _nextFriendId = _friendships.Max(f => f.Id) + 1;
        }
    }

    public Task<List<Friendship>> GetFriendsAsync(int userId)
    {
        var result = _friendships.Where(f => f.UserId == userId && f.Status == FriendStatus.Accepted).ToList();
        // Update online status from real users
        foreach (var f in result)
        {
            var real = AuthService.GetAllUsers().FirstOrDefault(u => u.Id == f.FriendId);
            if (real != null)
            {
                f.FriendIsOnline = real.IsOnline;
                f.FriendName = real.DisplayName;
                f.FriendLevel = real.Level;
            }
        }
        return Task.FromResult(result);
    }

    public async Task<List<Friendship>> GetOnlineFriendsAsync(int userId)
    {
        var all = await GetFriendsAsync(userId);
        return all.Where(f => f.FriendIsOnline).ToList();
    }

    public Task<List<FriendRequest>> GetPendingRequestsAsync(int userId)
    {
        var result = _requests.Where(r => r.ToUserId == userId).OrderByDescending(r => r.SentAt).ToList();
        return Task.FromResult(result);
    }

    public Task<List<BlockedUser>> GetBlockedUsersAsync(int userId)
    {
        var result = _blocked.Where(b => b.UserId == userId).ToList();
        return Task.FromResult(result);
    }

    public Task<Friendship> AddFriendAsync(int userId, string friendUsername)
    {
        if (string.IsNullOrWhiteSpace(friendUsername))
            throw new InvalidOperationException("Username required");

        var allUsers = AuthService.GetAllUsers();
        var target = allUsers.FirstOrDefault(u => u.Username.Equals(friendUsername, StringComparison.OrdinalIgnoreCase) || u.DisplayName.Equals(friendUsername, StringComparison.OrdinalIgnoreCase));
        if (target == null)
            throw new InvalidOperationException("Player not found");

        if (target.Id == userId)
            throw new InvalidOperationException("Cannot add yourself");

        if (_friendships.Any(f => f.UserId == userId && f.FriendId == target.Id && f.Status == FriendStatus.Accepted))
            throw new InvalidOperationException("Already friends");

        if (_blocked.Any(b => b.UserId == userId && b.BlockedId == target.Id))
            throw new InvalidOperationException("User is blocked");

        if (_blocked.Any(b => b.UserId == target.Id && b.BlockedId == userId))
            throw new InvalidOperationException("You are blocked by this player");

        // Check if request already pending
        if (_requests.Any(r => r.FromUserId == userId && r.ToUserId == target.Id))
            throw new InvalidOperationException("Friend request already sent");

        // For demo, auto-accept if target is mock user, otherwise create request
        var isMock = target.Username != "admin" && target.Id < 100; // mock users have low IDs
        if (isMock)
        {
            var friendship = new Friendship
            {
                Id = _nextFriendId++,
                UserId = userId,
                FriendId = target.Id,
                FriendName = target.DisplayName,
                FriendAvatar = target.AvatarPath,
                FriendIsOnline = target.IsOnline,
                FriendLevel = target.Level,
                FriendClass = target.MainClass.ToString(),
                Status = FriendStatus.Accepted,
                AcceptedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _friendships.Add(friendship);

            // Add reverse friendship
            var requester = allUsers.FirstOrDefault(u => u.Id == userId);
            var reverse = new Friendship
            {
                Id = _nextFriendId++,
                UserId = target.Id,
                FriendId = userId,
                FriendName = requester?.DisplayName ?? $"Player{userId}",
                FriendAvatar = requester?.AvatarPath ?? "Assets/avatar_conquer.png",
                FriendIsOnline = requester?.IsOnline ?? true,
                FriendLevel = requester?.Level ?? 1,
                FriendClass = requester?.MainClass.ToString() ?? "Trojan",
                Status = FriendStatus.Accepted,
                AcceptedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _friendships.Add(reverse);

            return Task.FromResult(friendship);
        }
        else
        {
            var request = new FriendRequest
            {
                Id = _nextRequestId++,
                FromUserId = userId,
                FromUsername = allUsers.FirstOrDefault(u => u.Id == userId)?.DisplayName ?? $"Player{userId}",
                ToUserId = target.Id,
                Message = $"{allUsers.FirstOrDefault(u => u.Id == userId)?.DisplayName} wants to be friends!",
                SentAt = DateTime.UtcNow
            };
            _requests.Add(request);

            // Return pending friendship placeholder
            return Task.FromResult(new Friendship
            {
                Id = -1,
                UserId = userId,
                FriendId = target.Id,
                FriendName = target.DisplayName,
                Status = FriendStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    public Task<bool> RemoveFriendAsync(int userId, int friendId)
    {
        var friendship = _friendships.FirstOrDefault(f => f.UserId == userId && f.FriendId == friendId);
        if (friendship == null) return Task.FromResult(false);

        _friendships.Remove(friendship);

        // Remove reverse
        var reverse = _friendships.FirstOrDefault(f => f.UserId == friendId && f.FriendId == userId);
        if (reverse != null) _friendships.Remove(reverse);

        return Task.FromResult(true);
    }

    public Task<bool> AcceptFriendRequestAsync(int userId, int requestId)
    {
        var request = _requests.FirstOrDefault(r => r.Id == requestId && r.ToUserId == userId);
        if (request == null) return Task.FromResult(false);

        var allUsers = AuthService.GetAllUsers();
        var fromUser = allUsers.FirstOrDefault(u => u.Id == request.FromUserId);
        var toUser = allUsers.FirstOrDefault(u => u.Id == userId);

        if (fromUser == null || toUser == null) return Task.FromResult(false);

        var friendship1 = new Friendship
        {
            Id = _nextFriendId++,
            UserId = userId,
            FriendId = fromUser.Id,
            FriendName = fromUser.DisplayName,
            FriendAvatar = fromUser.AvatarPath,
            FriendIsOnline = fromUser.IsOnline,
            FriendLevel = fromUser.Level,
            FriendClass = fromUser.MainClass.ToString(),
            Status = FriendStatus.Accepted,
            AcceptedAt = DateTime.UtcNow,
            CreatedAt = request.SentAt
        };

        var friendship2 = new Friendship
        {
            Id = _nextFriendId++,
            UserId = fromUser.Id,
            FriendId = userId,
            FriendName = toUser.DisplayName,
            FriendAvatar = toUser.AvatarPath,
            FriendIsOnline = toUser.IsOnline,
            FriendLevel = toUser.Level,
            FriendClass = toUser.MainClass.ToString(),
            Status = FriendStatus.Accepted,
            AcceptedAt = DateTime.UtcNow,
            CreatedAt = request.SentAt
        };

        _friendships.Add(friendship1);
        _friendships.Add(friendship2);
        _requests.Remove(request);

        return Task.FromResult(true);
    }

    public Task<bool> DeclineFriendRequestAsync(int userId, int requestId)
    {
        var request = _requests.FirstOrDefault(r => r.Id == requestId && r.ToUserId == userId);
        if (request == null) return Task.FromResult(false);
        _requests.Remove(request);
        return Task.FromResult(true);
    }

    public Task<BlockedUser> BlockUserAsync(int userId, int blockedId, string reason = "")
    {
        if (userId == blockedId) throw new InvalidOperationException("Cannot block yourself");

        if (_blocked.Any(b => b.UserId == userId && b.BlockedId == blockedId))
            throw new InvalidOperationException("Already blocked");

        // Remove friendship if exists
        var friendship = _friendships.FirstOrDefault(f => f.UserId == userId && f.FriendId == blockedId);
        if (friendship != null) _friendships.Remove(friendship);
        var reverse = _friendships.FirstOrDefault(f => f.UserId == blockedId && f.FriendId == userId);
        if (reverse != null) _friendships.Remove(reverse);

        var blockedUser = AuthService.GetAllUsers().FirstOrDefault(u => u.Id == blockedId);
        var block = new BlockedUser
        {
            Id = _nextBlockId++,
            UserId = userId,
            BlockedId = blockedId,
            BlockedName = blockedUser?.DisplayName ?? $"Player{blockedId}",
            Reason = reason,
            BlockedAt = DateTime.UtcNow
        };

        _blocked.Add(block);
        return Task.FromResult(block);
    }

    public Task<bool> UnblockUserAsync(int userId, int blockedId)
    {
        var block = _blocked.FirstOrDefault(b => b.UserId == userId && b.BlockedId == blockedId);
        if (block == null) return Task.FromResult(false);
        _blocked.Remove(block);
        return Task.FromResult(true);
    }

    public Task<bool> IsBlockedAsync(int userId, int otherId)
    {
        return Task.FromResult(_blocked.Any(b => b.UserId == userId && b.BlockedId == otherId));
    }

    public Task<List<Friendship>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(new List<Friendship>());

        var lower = query.ToLowerInvariant();
        var users = AuthService.GetAllUsers()
            .Where(u => u.Username.ToLowerInvariant().Contains(lower) || u.DisplayName.ToLowerInvariant().Contains(lower))
            .Take(10)
            .Select(u => new Friendship
            {
                Id = u.Id,
                UserId = 0,
                FriendId = u.Id,
                FriendName = u.DisplayName,
                FriendAvatar = u.AvatarPath,
                FriendIsOnline = u.IsOnline,
                FriendLevel = u.Level,
                FriendClass = u.MainClass.ToString(),
                Status = FriendStatus.Pending
            }).ToList();

        return Task.FromResult(users);
    }
}
