using System.Collections.Generic;

namespace NauraLauncher.Core.Entities;

/// <summary>
/// Persistent launcher settings model.
/// </summary>
public class LauncherConfig
{
    // General
    public bool AutoUpdateClient { get; set; } = true;
    public bool LaunchOnStartup { get; set; } = false;
    public bool DiscordRichPresence { get; set; } = true;
    public bool BetaChannel { get; set; } = false;
    public bool HardwareSurvey { get; set; } = true;
    public double InterfaceScale { get; set; } = 100;
    public double BackgroundDownloads { get; set; } = 4;

    // Video
    public bool VSync { get; set; } = false;
    public bool RayTracing { get; set; } = true;
    public bool FrameGeneration { get; set; } = true;
    public double ResolutionScale { get; set; } = 100;
    public double FrameRateCap { get; set; } = 144;
    public double FieldOfView { get; set; } = 90;

    // Audio
    public bool SpatialAudio { get; set; } = true;
    public bool MuteWhenUnfocused { get; set; } = false;
    public double MasterVolume { get; set; } = 82;
    public double MusicVolume { get; set; } = 45;
    public double SfxVolume { get; set; } = 90;
    public double VoiceChatVolume { get; set; } = 70;

    // Input
    public bool InvertVerticalAxis { get; set; } = false;
    public bool ToggleSprint { get; set; } = true;
    public double MouseSensitivity { get; set; } = 8;
    public double AimSmoothing { get; set; } = 30;

    // Network
    public bool LowLatencyMode { get; set; } = true;
    public bool PeerToPeerRelay { get; set; } = false;
    public bool VoicePriority { get; set; } = true;
    public double PacketBuffer { get; set; } = 60;

    // Server / Backend settings
    public string ApiBaseUrl { get; set; } = "https://127.0.0.1:8443";
    public string WebSocketUrl { get; set; } = "wss://127.0.0.1:8443/ws";
    public string MySqlConnectionString { get; set; } = "Server=127.0.0.1;Port=3306;Database=naura_launcher;Uid=apex_user;Pwd=ApexSecure2026!;SslMode=Preferred;";
}
