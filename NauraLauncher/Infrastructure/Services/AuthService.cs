using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NauraLauncher.Core.Entities;
using NauraLauncher.Core.Interfaces;

namespace NauraLauncher.Infrastructure.Services;

/// <summary>
/// Production authentication service managing JWT sessions, secure token storage, and user state.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;
    private readonly ISecurityService _securityService;
    private readonly string _tokenCachePath;

    public UserProfile CurrentUser { get; private set; } = new();
    public bool IsAuthenticated { get; private set; }
    public string? AccessToken { get; private set; }

    public event EventHandler<UserProfile>? UserChanged;
    public event EventHandler<double>? CreditsChanged;

    public AuthService(IApiClient apiClient, ISecurityService securityService)
    {
        _apiClient = apiClient;
        _securityService = securityService;

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string folder = Path.Combine(appData, "NauraLauncher");
        Directory.CreateDirectory(folder);
        _tokenCachePath = Path.Combine(folder, "session.enc");

        // Try restoring cached session
        _ = TryRestoreSessionAsync();
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, LoginResponse>("/api/v1/auth/login", new
            {
                username,
                password
            });

            if (result != null && !string.IsNullOrEmpty(result.Token) && result.User != null)
            {
                AccessToken = result.Token;
                _apiClient.SetBearerToken(AccessToken);

                CurrentUser = new UserProfile
                {
                    Id = result.User.Id,
                    Username = result.User.Username,
                    Email = result.User.Email,
                    Role = result.User.Role,
                    Credits = result.User.Credits,
                    Status = result.User.Status,
                    AvatarUrl = result.User.AvatarUrl ?? "Assets/avatar.png"
                };

                IsAuthenticated = true;
                await CacheSessionAsync(AccessToken, result.RefreshToken);

                UserChanged?.Invoke(this, CurrentUser);
                CreditsChanged?.Invoke(this, CurrentUser.Credits);
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] Login error: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        try
        {
            var result = await _apiClient.PostAsync<object, LoginResponse>("/api/v1/auth/register", new
            {
                username,
                email,
                password
            });

            if (result != null && !string.IsNullOrEmpty(result.Token) && result.User != null)
            {
                AccessToken = result.Token;
                _apiClient.SetBearerToken(AccessToken);

                CurrentUser = new UserProfile
                {
                    Id = result.User.Id,
                    Username = result.User.Username,
                    Email = result.User.Email,
                    Role = result.User.Role,
                    Credits = result.User.Credits,
                    Status = result.User.Status,
                    AvatarUrl = result.User.AvatarUrl ?? "Assets/avatar.png"
                };

                IsAuthenticated = true;
                await CacheSessionAsync(AccessToken, result.RefreshToken);

                UserChanged?.Invoke(this, CurrentUser);
                CreditsChanged?.Invoke(this, CurrentUser.Credits);
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] Register error: {ex.Message}");
        }

        return false;
    }

    public async Task LogoutAsync()
    {
        AccessToken = null;
        _apiClient.SetBearerToken(null);
        IsAuthenticated = false;

        CurrentUser = new UserProfile
        {
            Username = "GUEST",
            Role = "RECRUIT",
            Credits = 0,
            Status = "OFFLINE"
        };

        try
        {
            if (File.Exists(_tokenCachePath)) File.Delete(_tokenCachePath);
        }
        catch { }

        UserChanged?.Invoke(this, CurrentUser);
        CreditsChanged?.Invoke(this, CurrentUser.Credits);
        await Task.CompletedTask;
    }

    public async Task<bool> RefreshProfileAsync()
    {
        try
        {
            var result = await _apiClient.GetAsync<MeResponse>("/api/v1/auth/me");
            if (result?.User != null)
            {
                CurrentUser.Credits = result.User.Credits;
                CurrentUser.Status = result.User.Status;
                CreditsChanged?.Invoke(this, CurrentUser.Credits);
                UserChanged?.Invoke(this, CurrentUser);
                return true;
            }
        }
        catch { }
        return false;
    }

    private async Task CacheSessionAsync(string token, string? refreshToken)
    {
        try
        {
            var data = new { token, refreshToken, savedAt = DateTime.UtcNow };
            string raw = JsonSerializer.Serialize(data);
            string encrypted = _securityService.ProtectSecret(raw);
            await File.WriteAllTextAsync(_tokenCachePath, encrypted);
        }
        catch { }
    }

    private async Task TryRestoreSessionAsync()
    {
        try
        {
            if (File.Exists(_tokenCachePath))
            {
                string encrypted = await File.ReadAllTextAsync(_tokenCachePath);
                string raw = _securityService.UnprotectSecret(encrypted);
                if (!string.IsNullOrEmpty(raw))
                {
                    using var doc = JsonDocument.Parse(raw);
                    if (doc.RootElement.TryGetProperty("token", out var tokenProp))
                    {
                        AccessToken = tokenProp.GetString();
                        _apiClient.SetBearerToken(AccessToken);
                        await RefreshProfileAsync();
                    }
                }
            }
        }
        catch { }
    }

    private record LoginResponse(string Token, string? RefreshToken, UserDto? User);
    private record MeResponse(UserDto? User);
    private record UserDto(string Id, string Username, string Email, string Role, double Credits, string Status, string? AvatarUrl);
}
