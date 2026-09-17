using System;
using System.Threading.Tasks;
using NauraLauncher.Core.Entities;

namespace NauraLauncher.Core.Interfaces;

public interface IAuthService
{
    UserProfile CurrentUser { get; }
    bool IsAuthenticated { get; }
    string? AccessToken { get; }

    Task<bool> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(string username, string email, string password);
    Task LogoutAsync();
    Task<bool> RefreshProfileAsync();

    event EventHandler<UserProfile>? UserChanged;
    event EventHandler<double>? CreditsChanged;
}
