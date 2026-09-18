using NauraLauncher.Domain.Entities;

namespace NauraLauncher.Application.Interfaces;

public interface IFriendsService
{
    Task<List<Friendship>> GetFriendsAsync(int userId);
    Task<List<Friendship>> GetOnlineFriendsAsync(int userId);
    Task<List<FriendRequest>> GetPendingRequestsAsync(int userId);
    Task<List<BlockedUser>> GetBlockedUsersAsync(int userId);

    Task<Friendship> AddFriendAsync(int userId, string friendUsername);
    Task<bool> RemoveFriendAsync(int userId, int friendId);
    Task<bool> AcceptFriendRequestAsync(int userId, int requestId);
    Task<bool> DeclineFriendRequestAsync(int userId, int requestId);

    Task<BlockedUser> BlockUserAsync(int userId, int blockedId, string reason = "");
    Task<bool> UnblockUserAsync(int userId, int blockedId);
    Task<bool> IsBlockedAsync(int userId, int otherId);

    Task<List<Friendship>> SearchUsersAsync(string query);
}
