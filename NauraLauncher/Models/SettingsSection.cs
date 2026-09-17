using NauraLauncher.Common;

namespace NauraLauncher.Models;

/// <summary>
/// An entry in the Settings page left-hand sections rail.
/// </summary>
public class SettingsSection : ObservableObject
{
    public string Id { get; set; } = string.Empty;         // "video"
    public string Name { get; set; } = string.Empty;       // "VIDEO"
    public string IconKey { get; set; } = string.Empty;    // resource key in Icons.xaml
    public string Hint { get; set; } = string.Empty;       // right-aligned meta, e.g. "3 CHANGED"

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
