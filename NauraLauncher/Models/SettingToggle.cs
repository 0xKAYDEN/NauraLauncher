using NauraLauncher.Common;

namespace NauraLauncher.Models;

/// <summary>
/// A labelled shadcn Switch row (title + description + toggle).
/// </summary>
public class SettingToggle : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresRestart { get; set; }

    private bool _isOn;
    public bool IsOn
    {
        get => _isOn;
        set => SetProperty(ref _isOn, value);
    }
}
