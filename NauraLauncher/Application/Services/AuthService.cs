using NauraLauncher.Application.Interfaces;
using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;
using NauraLauncher.Infrastructure.Security;

namespace NauraLauncher.Application.Services;

/// <summary>
/// In-memory auth service for demo - production would use MySQL repository
/// Clean architecture: Application layer, depends on abstractions
/// </summary>
public class AuthService : IAuthService
{
    private static readonly List<User> _users = new();
    private static int _nextId = 100;

    static AuthService()
    {
        // Seed with mock users + a default test account
        var mockUsers = MockDataService.GenerateMockUsers();
        _users.AddRange(mockUsers);
        _nextId = mockUsers.Max(u => u.Id) + 1;

        // Add default login: admin / admin123
        var admin = new User
        {
            Id = _nextId++,
            Username = "admin",
            Email = "admin@conquer.local",
            DisplayName = "ConquerAdmin",
            PasswordHash = PasswordHasher.HashPassword("admin123"),
            MainClass = ConquerClass.Trojan,
            Level = 130,
            Reborn = RebornStage.SecondReborn,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            IsOnline = true,
            AvatarPath = "Assets/avatar_conquer.png",
            Wallet = new Wallet { Cps = 50000, Gold = 10000000, Silver = 5000000, BoundCps = 1000 }
        };
        _users.Add(admin);

        // Give wallets to mock users
        foreach (var u in _users.Where(u => u.Wallet.Cps == 0))
        {
            u.Wallet = new Wallet
            {
                UserId = u.Id,
                Cps = new Random().Next(100, 10000),
                Gold = new Random().Next(10000, 1000000),
                Silver = new Random().Next(1000, 100000),
                BoundCps = new Random().Next(0, 500)
            };
        }
    }

    public Task<AuthResult> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Task.FromResult(new AuthResult { Success = false, Message = "Username and password required" });
        }

        var user = _users.FirstOrDefault(u =>
            u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Equals(request.Username, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return Task.FromResult(new AuthResult { Success = false, Message = "User not found" });
        }

        // For mock users without password hash, allow any password or check hash if present
        bool valid = true;
        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            valid = PasswordHasher.VerifyPassword(request.Password, user.PasswordHash);
        }

        if (!valid)
        {
            return Task.FromResult(new AuthResult { Success = false, Message = "Invalid password" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.IsOnline = true;

        return Task.FromResult(new AuthResult
        {
            Success = true,
            Message = "Login successful",
            User = user,
            Token = PasswordHasher.GenerateSecureToken()
        });
    }

    public Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return Task.FromResult(new AuthResult { Success = false, Message = "Username and password required" });

        if (request.Password != request.ConfirmPassword)
            return Task.FromResult(new AuthResult { Success = false, Message = "Passwords do not match" });

        if (request.Password.Length < 6)
            return Task.FromResult(new AuthResult { Success = false, Message = "Password must be at least 6 characters" });

        if (_users.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult(new AuthResult { Success = false, Message = "Username already taken" });

        if (!string.IsNullOrWhiteSpace(request.Email) && _users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult(new AuthResult { Success = false, Message = "Email already registered" });

        var user = new User
        {
            Id = _nextId++,
            Username = request.Username,
            Email = request.Email,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Username : request.DisplayName,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            MainClass = ConquerClass.Trojan,
            Level = 1,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            IsOnline = true,
            AvatarPath = "Assets/avatar_conquer.png",
            Wallet = new Wallet { UserId = _nextId, Cps = 1000, Gold = 100000, Silver = 50000, BoundCps = 100 }
        };

        _users.Add(user);

        return Task.FromResult(new AuthResult
        {
            Success = true,
            Message = "Registration successful",
            User = user,
            Token = PasswordHasher.GenerateSecureToken()
        });
    }

    public Task<bool> LogoutAsync(int userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null) user.IsOnline = false;
        return Task.FromResult(true);
    }

    public Task<User?> GetUserByIdAsync(int userId)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == userId));
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<bool> IsUsernameAvailableAsync(string username)
    {
        return Task.FromResult(!_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<bool> IsEmailAvailableAsync(string email)
    {
        return Task.FromResult(!_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
    }

    // For internal use by other services
    public static List<User> GetAllUsers() => _users;
}
