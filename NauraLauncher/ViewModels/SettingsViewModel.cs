using System.Collections.ObjectModel;
using System.Linq;
using NauraLauncher.Common;
using NauraLauncher.Models;

namespace NauraLauncher.ViewModels;

/// <summary>
/// Settings page: sections rail plus grouped Switch / Slider / segmented rows.
/// Section switching is driven by <see cref="SelectedSectionId"/>, which the
/// content panes match against with a ConverterParameter.
/// </summary>
public class SettingsViewModel : ObservableObject
{
    public SettingsViewModel()
    {
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
            new() { Name = "AUTO-UPDATE CLIENT",  Description = "Download patches while the launcher is idle.",           IsOn = true },
            new() { Name = "LAUNCH ON STARTUP",   Description = "Start APEX when this machine boots.",                    IsOn = false },
            new() { Name = "DISCORD RICH PRESENCE", Description = "Broadcast the active title to your squad.",            IsOn = true },
            new() { Name = "BETA CHANNEL",        Description = "Receive unstable builds 48 hours early.",                IsOn = false, RequiresRestart = true },
            new() { Name = "HARDWARE SURVEY",     Description = "Share anonymous device telemetry with NEXUS.",           IsOn = true },
        };
        GeneralSliders = new ObservableCollection<SliderSetting>
        {
            new() { Name = "INTERFACE SCALE", Description = "Scales the launcher shell.", Minimum = 80, Maximum = 150, Value = 100, Format = "{0:0}%" },
            new() { Name = "BACKGROUND DOWNLOADS", Description = "Concurrent asset streams.", Minimum = 1, Maximum = 8, Value = 4, Format = "{0:0} STREAMS" },
        };

        // ----- VIDEO -----
        VideoSwitches = new ObservableCollection<SettingToggle>
        {
            new() { Name = "VSYNC", Description = "Lock output to the panel refresh rate.", IsOn = false },
            new() { Name = "RAY TRACING", Description = "Hardware accelerated reflections and GI.", IsOn = true, RequiresRestart = true },
            new() { Name = "FRAME GENERATION", Description = "DLSS 3 / FSR 3 interpolated frames.", IsOn = true },
        };
        VideoSliders = new ObservableCollection<SliderSetting>
        {
            new() { Name = "RESOLUTION SCALE", Description = "Internal render resolution.", Minimum = 50, Maximum = 200, Value = 100, Format = "{0:0}%" },
            new() { Name = "FRAME RATE CAP", Description = "Maximum frames per second.", Minimum = 30, Maximum = 240, TickFrequency = 30, Value = 144, Format = "{0:0} FPS" },
            new() { Name = "FIELD OF VIEW", Description = "Horizontal camera angle.", Minimum = 70, Maximum = 110, Value = 90, Format = "{0:0}°" },
        };

        // ----- AUDIO -----
        AudioSwitches = new ObservableCollection<SettingToggle>
        {
            new() { Name = "SPATIAL AUDIO", Description = "Binaural ray-traced positioning.", IsOn = true },
            new() { Name = "MUTE WHEN UNFOCUSED", Description = "Silence the client on focus loss.", IsOn = false },
        };
        AudioSliders = new ObservableCollection<SliderSetting>
        {
            new() { Name = "MASTER VOLUME", Description = "Global output level.", Value = 82, Format = "{0:0}%" },
            new() { Name = "MUSIC", Description = "Score and menu ambience.", Value = 45, Format = "{0:0}%" },
            new() { Name = "SFX", Description = "Weapons, footsteps, impact.", Value = 90, Format = "{0:0}%" },
            new() { Name = "VOICE CHAT", Description = "Squad comms level.", Value = 70, Format = "{0:0}%" },
        };

        // ----- INPUT -----
        InputSwitches = new ObservableCollection<SettingToggle>
        {
            new() { Name = "INVERT VERTICAL AXIS", Description = "Flip pitch on mouse and stick.", IsOn = false },
            new() { Name = "TOGGLE SPRINT", Description = "Hold or toggle the sprint modifier.", IsOn = true },
        };
        InputSliders = new ObservableCollection<SliderSetting>
        {
            new() { Name = "MOUSE SENSITIVITY", Description = "Counts per degree of yaw.", Minimum = 1, Maximum = 20, Value = 8, Format = "{0:0}" },
            new() { Name = "AIM SMOOTHING", Description = "Interpolation applied to stick input.", Value = 30, Format = "{0:0}%" },
        };

        // ----- NETWORK -----
        NetworkSwitches = new ObservableCollection<SettingToggle>
        {
            new() { Name = "LOW LATENCY MODE", Description = "Prefer the nearest relay over the cheapest.", IsOn = true },
            new() { Name = "PEER-TO-PEER RELAY", Description = "Allow direct squad connections.", IsOn = false },
            new() { Name = "VOICE PRIORITY", Description = "Tag comms traffic as high priority.", IsOn = true },
        };
        NetworkSliders = new ObservableCollection<SliderSetting>
        {
            new() { Name = "PACKET BUFFER", Description = "Jitter compensation window.", Minimum = 0, Maximum = 200, TickFrequency = 10, Value = 60, Format = "{0:0} MS" },
        };

        // ----- PRIVACY -----
        PrivacySwitches = new ObservableCollection<SettingToggle>
        {
            new() { Name = "SHOW ONLINE STATUS", Description = "Appear online to your squad list.", IsOn = true },
            new() { Name = "CROSS-PLAY INVITES", Description = "Accept invites from other platforms.", IsOn = true },
            new() { Name = "PUBLIC PROFILE", Description = "Let collectors view your vault.", IsOn = false },
            new() { Name = "AUCTION BID VISIBILITY", Description = "Reveal your handle on the bid ladder.", IsOn = false },
        };

        ResetCommand = new RelayCommand(ResetAll);
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
    }
}
