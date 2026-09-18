using NauraLauncher.Domain.Entities;

namespace NauraLauncher.Core.Interfaces;

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public User? User { get; set; }
    public string? Token { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<bool> LogoutAsync(int userId);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
}
