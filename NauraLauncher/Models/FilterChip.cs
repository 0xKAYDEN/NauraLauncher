using NauraLauncher.Common;

namespace NauraLauncher.Models;

/// <summary>
/// A selectable filter chip (auction rarity filter, marketplace categories…).
/// <see cref="ToneHex"/> keeps the accent colour in data so the view stays generic.
/// </summary>
public class FilterChip : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string Count { get; set; } = string.Empty;
    public string ToneHex { get; set; } = "#8A8F99";

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
