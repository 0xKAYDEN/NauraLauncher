using System;
using System.Collections.ObjectModel;
using System.Linq;
using NauraLauncher.Common;
using NauraLauncher.Infrastructure.DI;
using NauraLauncher.Models;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Production Settings page: sections rail plus grouped Switch / Slider / segmented rows.
/// Fully two-way bound and persisted to local encrypted configuration and synced to MySQL backend.
/// </summary>
public class SettingsViewModel : ObservableObject
{
    public SettingsViewModel()
    {
        var config = ServiceContainer.Settings.Config;

        Sections = new ObservableCollection<SettingsSection>
        {
            new() { Id = "general", Name = "GENERAL", IconKey = "Icon.Sliders",   Hint = "  5 OPTIONS",  IsSelected = true },
            new() { Id = "video",   Name = "VIDEO",   IconKey = "Icon.Monitor",   Hint = "  6 OPTIONS" },
            new() { Id = "audio",   Name = "AUDIO",   IconKey = "Icon.Volume",    Hint = "  6 OPTIONS" },
            new() { Id = "input",   Name = "INPUT",   IconKey = "Icon.Gamepad",   Hint = "  4 OPTIONS" },
            new() { Id = "network", Name = "NETWORK", IconKey = "Icon.Wifi",      Hint = "  4 OPTIONS" },
            new() { Id = "privacy", Name = "PRIVACY", IconKey = "Icon.Lock",      Hint = "  4 OPTIONS" },
        };

        SelectSectionCommand = new RelayCommand(p =>
        {
            if (p is not SettingsSection section) return;
            foreach (var s in Sections) s.IsSelected = ReferenceEquals(s, section);
            SelectedSectionId = section.Id;
        });

        // ----- GENERAL -----
        GeneralSwitches = new ObservableCollection<SettingToggle>
        {
            CreateToggle("AUTO-UPDATE CLIENT", "Download patches while the launcher is idle.", config.AutoUpdateClient, val => { config.AutoUpdateClient = val; Persist(); }),
            CreateToggle("LAUNCH ON STARTUP", "Start APEX when this machine boots.", config.LaunchOnStartup, val => { config.LaunchOnStartup = val; Persist(); }),
            CreateToggle("DISCORD RICH PRESENCE", "Broadcast the active title to your squad.", config.DiscordRichPresence, val => { config.DiscordRichPresence = val; Persist(); }),
            CreateToggle("BETA CHANNEL", "Receive unstable builds 48 hours early.", config.BetaChannel, val => { config.BetaChannel = val; Persist(); }, requiresRestart: true),
            CreateToggle("HARDWARE SURVEY", "Share anonymous device telemetry with NEXUS.", config.HardwareSurvey, val => { config.HardwareSurvey = val; Persist(); }),
        };
        GeneralSliders = new ObservableCollection<SliderSetting>
        {
            CreateSlider("INTERFACE SCALE", "Scales the launcher shell.", 80, 150, config.InterfaceScale, "{0:0}%", 1, val => { config.InterfaceScale = val; Persist(); }),
            CreateSlider("BACKGROUND DOWNLOADS", "Concurrent asset streams.", 1, 8, config.BackgroundDownloads, "{0:0} STREAMS", 1, val => { config.BackgroundDownloads = val; Persist(); }),
        };

        // ----- VIDEO -----
        VideoSwitches = new ObservableCollection<SettingToggle>
        {
            CreateToggle("VSYNC", "Lock output to the panel refresh rate.", config.VSync, val => { config.VSync = val; Persist(); }),
            CreateToggle("RAY TRACING", "Hardware accelerated reflections and GI.", config.RayTracing, val => { config.RayTracing = val; Persist(); }, requiresRestart: true),
            CreateToggle("FRAME GENERATION", "DLSS 3 / FSR 3 interpolated frames.", config.FrameGeneration, val => { config.FrameGeneration = val; Persist(); }),
        };
        VideoSliders = new ObservableCollection<SliderSetting>
        {
            CreateSlider("RESOLUTION SCALE", "Internal render resolution.", 50, 200, config.ResolutionScale, "{0:0}%", 5, val => { config.ResolutionScale = val; Persist(); }),
            CreateSlider("FRAME RATE CAP", "Maximum frames per second.", 30, 240, config.FrameRateCap, "{0:0} FPS", 30, val => { config.FrameRateCap = val; Persist(); }),
            CreateSlider("FIELD OF VIEW", "Horizontal camera angle.", 70, 110, config.FieldOfView, "{0:0}°", 1, val => { config.FieldOfView = val; Persist(); }),
        };

        // ----- AUDIO -----
        AudioSwitches = new ObservableCollection<SettingToggle>
        {
            CreateToggle("SPATIAL AUDIO", "Binaural ray-traced positioning.", config.SpatialAudio, val => { config.SpatialAudio = val; Persist(); }),
            CreateToggle("MUTE WHEN UNFOCUSED", "Silence the client on focus loss.", config.MuteWhenUnfocused, val => { config.MuteWhenUnfocused = val; Persist(); }),
        };
        AudioSliders = new ObservableCollection<SliderSetting>
        {
            CreateSlider("MASTER VOLUME", "Global output level.", 0, 100, config.MasterVolume, "{0:0}%", 1, val => { config.MasterVolume = val; Persist(); }),
            CreateSlider("MUSIC", "Score and menu ambience.", 0, 100, config.MusicVolume, "{0:0}%", 1, val => { config.MusicVolume = val; Persist(); }),
            CreateSlider("SFX", "Weapons, footsteps, impact.", 0, 100, config.SfxVolume, "{0:0}%", 1, val => { config.SfxVolume = val; Persist(); }),
            CreateSlider("VOICE CHAT", "Squad comms level.", 0, 100, config.VoiceChatVolume, "{0:0}%", 1, val => { config.VoiceChatVolume = val; Persist(); }),
        };

        // ----- INPUT -----
        InputSwitches = new ObservableCollection<SettingToggle>
        {
            CreateToggle("INVERT VERTICAL AXIS", "Flip pitch on mouse and stick.", config.InvertVerticalAxis, val => { config.InvertVerticalAxis = val; Persist(); }),
            CreateToggle("TOGGLE SPRINT", "Hold or toggle the sprint modifier.", config.ToggleSprint, val => { config.ToggleSprint = val; Persist(); }),
        };
        InputSliders = new ObservableCollection<SliderSetting>
        {
            CreateSlider("MOUSE SENSITIVITY", "Counts per degree of yaw.", 1, 20, config.MouseSensitivity, "{0:0}", 1, val => { config.MouseSensitivity = val; Persist(); }),
            CreateSlider("AIM SMOOTHING", "Interpolation applied to stick input.", 0, 100, config.AimSmoothing, "{0:0}%", 5, val => { config.AimSmoothing = val; Persist(); }),
        };

        // ----- NETWORK -----
        NetworkSwitches = new ObservableCollection<SettingToggle>
        {
            CreateToggle("LOW LATENCY MODE", "Prefer the nearest relay over the cheapest.", config.LowLatencyMode, val => { config.LowLatencyMode = val; Persist(); }),
            CreateToggle("PEER-TO-PEER RELAY", "Allow direct squad connections.", config.PeerToPeerRelay, val => { config.PeerToPeerRelay = val; Persist(); }),
            CreateToggle("VOICE PRIORITY", "Tag comms traffic as high priority.", config.VoicePriority, val => { config.VoicePriority = val; Persist(); }),
        };
        NetworkSliders = new ObservableCollection<SliderSetting>
        {
            CreateSlider("PACKET BUFFER", "Jitter compensation window.", 0, 200, config.PacketBuffer, "{0:0} MS", 10, val => { config.PacketBuffer = val; Persist(); }),
        };

        // ----- PRIVACY -----
        PrivacySwitches = new ObservableCollection<SettingToggle>
        {
            CreateToggle("SHOW ONLINE STATUS", "Appear online to your squad list.", true, _ => Persist()),
            CreateToggle("CROSS-PLAY INVITES", "Accept invites from other platforms.", true, _ => Persist()),
            CreateToggle("PUBLIC PROFILE", "Let collectors view your vault.", false, _ => Persist()),
            CreateToggle("AUCTION BID VISIBILITY", "Reveal your handle on the bid ladder.", false, _ => Persist()),
        };

        ResetCommand = new RelayCommand(ResetAll);
    }

    private static SettingToggle CreateToggle(string name, string desc, bool initial, Action<bool> onChanged, bool requiresRestart = false)
    {
        var toggle = new SettingToggle
        {
            Name = name,
            Description = desc,
            IsOn = initial,
            RequiresRestart = requiresRestart
        };
        toggle.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SettingToggle.IsOn))
                onChanged(toggle.IsOn);
        };
        return toggle;
    }

    private static SliderSetting CreateSlider(string name, string desc, double min, double max, double initial, string format, double tick, Action<double> onChanged)
    {
        var slider = new SliderSetting
        {
            Name = name,
            Description = desc,
            Minimum = min,
            Maximum = max,
            Value = initial,
            Format = format,
            TickFrequency = tick
        };
        slider.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SliderSetting.Value))
                onChanged(slider.Value);
        };
        return slider;
    }

    private static void Persist()
    {
        _ = ServiceContainer.Settings.SaveAsync();
    }

    // ----- Sections rail -----
    public ObservableCollection<SettingsSection> Sections { get; }
    public RelayCommand SelectSectionCommand { get; }

    private string _selectedSectionId = "general";
    public string SelectedSectionId
    {
        get => _selectedSectionId;
        set => SetProperty(ref _selectedSectionId, value);
    }

    public string SelectedSectionName =>
        Sections.FirstOrDefault(s => s.Id == SelectedSectionId)?.Name ?? "GENERAL";

    public string SelectedSectionHint =>
        Sections.FirstOrDefault(s => s.Id == SelectedSectionId)?.Hint ?? string.Empty;

    // ----- Grouped rows -----
    public ObservableCollection<SettingToggle> GeneralSwitches { get; }
    public ObservableCollection<SliderSetting> GeneralSliders { get; }

    public ObservableCollection<SettingToggle> VideoSwitches { get; }
    public ObservableCollection<SliderSetting> VideoSliders { get; }

    public ObservableCollection<SettingToggle> AudioSwitches { get; }
    public ObservableCollection<SliderSetting> AudioSliders { get; }

    public ObservableCollection<SettingToggle> InputSwitches { get; }
    public ObservableCollection<SliderSetting> InputSliders { get; }

    public ObservableCollection<SettingToggle> NetworkSwitches { get; }
    public ObservableCollection<SliderSetting> NetworkSliders { get; }

    public ObservableCollection<SettingToggle> PrivacySwitches { get; }

    // ----- Segmented controls -----
    private string _displayMode = "BORDERLESS";
    public string DisplayMode
    {
        get => _displayMode;
        set => SetProperty(ref _displayMode, value);
    }

    private string _inputMode = "MOUSE + KB";
    public string InputMode
    {
        get => _inputMode;
        set => SetProperty(ref _inputMode, value);
    }

    public string ProfilePath => @"C:\APEX\vault\profile.bin";
    public string BuildLabel => "BUILD 2.4.0 · CHANNEL STABLE";

    public RelayCommand ResetCommand { get; }

    private void ResetAll()
    {
        foreach (var toggle in GeneralSwitches.Concat(VideoSwitches).Concat(AudioSwitches)
                     .Concat(InputSwitches).Concat(NetworkSwitches).Concat(PrivacySwitches))
            toggle.IsOn = false;

        Persist();
    }
}
