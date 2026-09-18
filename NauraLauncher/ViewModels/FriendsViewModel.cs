using System.Collections.ObjectModel;
using NauraLauncher.Application.Interfaces;
using NauraLauncher.Application.Services;
using NauraLauncher.Common;
using NauraLauncher.Domain.Entities;

namespace NauraLauncher.ViewModels;

public class FriendsViewModel : ObservableObject
{
    private readonly IFriendsService _friendsService;
    private readonly IVoiceService _voiceService;

    public FriendsViewModel() : this(new FriendsService(), new VoiceService()) { }

    public FriendsViewModel(IFriendsService friendsService, IVoiceService voiceService)
    {
        _friendsService = friendsService;
        _voiceService = voiceService;

        Friends = new ObservableCollection<Friendship>();
        OnlineFriends = new ObservableCollection<Friendship>();
        PendingRequests = new ObservableCollection<FriendRequest>();
        BlockedUsers = new ObservableCollection<BlockedUser>();
        SearchResults = new ObservableCollection<Friendship>();

        AddFriendCommand = new RelayCommand(async () => await AddFriendAsync(), () => !IsBusy && !string.IsNullOrWhiteSpace(AddFriendUsername));
        RemoveFriendCommand = new RelayCommand(async p => { if (p is Friendship f) await RemoveFriendAsync(f); });
        BlockUserCommand = new RelayCommand(async p => { if (p is Friendship f) await BlockUserAsync(f); });
        UnblockUserCommand = new RelayCommand(async p => { if (p is BlockedUser b) await UnblockUserAsync(b); });
        AcceptRequestCommand = new RelayCommand(async p => { if (p is FriendRequest r) await AcceptRequestAsync(r); });
        DeclineRequestCommand = new RelayCommand(async p => { if (p is FriendRequest r) await DeclineRequestAsync(r); });
        StartVoiceCallCommand = new RelayCommand(async p => { if (p is Friendship f) await StartVoiceCallAsync(f); });
        SearchCommand = new RelayCommand(async () => await SearchUsersAsync());
        RefreshCommand = new RelayCommand(async () => await LoadAllAsync());
    }

    private int _userId;
    public int UserId
    {
        get => _userId;
        set
        {
            if (SetProperty(ref _userId, value))
                _ = LoadAllAsync();
        }
    }

    public ObservableCollection<Friendship> Friends { get; }
    public ObservableCollection<Friendship> OnlineFriends { get; }
    public ObservableCollection<FriendRequest> PendingRequests { get; }
    public ObservableCollection<BlockedUser> BlockedUsers { get; }
    public ObservableCollection<Friendship> SearchResults { get; }

    private string _addFriendUsername = string.Empty;
    public string AddFriendUsername
    {
        get => _addFriendUsername;
        set => SetProperty(ref _addFriendUsername, value);
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private VoiceCall? _activeCall;
    public VoiceCall? ActiveCall
    {
        get => _activeCall;
        set => SetProperty(ref _activeCall, value);
    }

    public bool HasActiveCall => ActiveCall != null && ActiveCall.Status == VoiceCallStatus.Active;

    public string FriendsCount => $"{Friends.Count} FRIENDS";
    public string OnlineCount => $"{OnlineFriends.Count} ONLINE";

    public RelayCommand AddFriendCommand { get; }
    public RelayCommand RemoveFriendCommand { get; }
    public RelayCommand BlockUserCommand { get; }
    public RelayCommand UnblockUserCommand { get; }
    public RelayCommand AcceptRequestCommand { get; }
    public RelayCommand DeclineRequestCommand { get; }
    public RelayCommand StartVoiceCallCommand { get; }
    public RelayCommand SearchCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public async Task LoadAllAsync()
    {
        if (UserId == 0) return;
        IsBusy = true;
        try
        {
            var friends = await _friendsService.GetFriendsAsync(UserId);
            var online = await _friendsService.GetOnlineFriendsAsync(UserId);
            var pending = await _friendsService.GetPendingRequestsAsync(UserId);
            var blocked = await _friendsService.GetBlockedUsersAsync(UserId);

            Friends.Clear();
            foreach (var f in friends) Friends.Add(f);

            OnlineFriends.Clear();
            foreach (var f in online) OnlineFriends.Add(f);

            PendingRequests.Clear();
            foreach (var r in pending) PendingRequests.Add(r);

            BlockedUsers.Clear();
            foreach (var b in blocked) BlockedUsers.Add(b);

            OnPropertyChanged(nameof(FriendsCount));
            OnPropertyChanged(nameof(OnlineCount));
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddFriendAsync()
    {
        if (string.IsNullOrWhiteSpace(AddFriendUsername)) return;
        IsBusy = true;
        try
        {
            var result = await _friendsService.AddFriendAsync(UserId, AddFriendUsername);
            if (result.Id != -1) // Accepted immediately
            {
                StatusMessage = $"Added {result.FriendName} as friend!";
                Friends.Add(result);
                OnPropertyChanged(nameof(FriendsCount));
            }
            else
            {
                StatusMessage = $"Friend request sent to {result.FriendName}";
            }
            AddFriendUsername = string.Empty;
            await LoadAllAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveFriendAsync(Friendship friend)
    {
        IsBusy = true;
        try
        {
            var success = await _friendsService.RemoveFriendAsync(UserId, friend.FriendId);
            if (success)
            {
                Friends.Remove(friend);
                OnPropertyChanged(nameof(FriendsCount));
                StatusMessage = $"Removed {friend.FriendName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task BlockUserAsync(Friendship friend)
    {
        IsBusy = true;
        try
        {
            var block = await _friendsService.BlockUserAsync(UserId, friend.FriendId, "Blocked from launcher");
            StatusMessage = $"Blocked {friend.FriendName}";
            await LoadAllAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UnblockUserAsync(BlockedUser blocked)
    {
        IsBusy = true;
        try
        {
            var success = await _friendsService.UnblockUserAsync(UserId, blocked.BlockedId);
            if (success)
            {
                StatusMessage = $"Unblocked {blocked.BlockedName}";
                await LoadAllAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AcceptRequestAsync(FriendRequest request)
    {
        IsBusy = true;
        try
        {
            var success = await _friendsService.AcceptFriendRequestAsync(UserId, request.Id);
            if (success)
            {
                StatusMessage = $"Accepted {request.FromUsername}";
                await LoadAllAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeclineRequestAsync(FriendRequest request)
    {
        IsBusy = true;
        try
        {
            var success = await _friendsService.DeclineFriendRequestAsync(UserId, request.Id);
            if (success)
            {
                PendingRequests.Remove(request);
                StatusMessage = $"Declined {request.FromUsername}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StartVoiceCallAsync(Friendship friend)
    {
        IsBusy = true;
        try
        {
            var call = await _voiceService.InitiateCallAsync(UserId, friend.FriendId);
            ActiveCall = call;
            OnPropertyChanged(nameof(HasActiveCall));
            StatusMessage = $"Calling {friend.FriendName}...";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task EndActiveCallAsync()
    {
        if (ActiveCall == null) return;
        await _voiceService.EndCallAsync(ActiveCall.CallId, UserId, "Ended by user");
        ActiveCall = null;
        OnPropertyChanged(nameof(HasActiveCall));
        StatusMessage = "Call ended";
    }

    private async Task SearchUsersAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        IsBusy = true;
        try
        {
            var results = await _friendsService.SearchUsersAsync(SearchQuery);
            SearchResults.Clear();
            foreach (var r in results) SearchResults.Add(r);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
