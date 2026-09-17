namespace NauraLauncher.Core.Entities;

/// <summary>
/// Authenticated user profile entity.
/// </summary>
public class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = "VALKYRIE";
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "OPERATOR";
    public double Credits { get; set; } = 148.50;
    public string AvatarUrl { get; set; } = "Assets/avatar.png";
    public string Status { get; set; } = "ONLINE";
}
