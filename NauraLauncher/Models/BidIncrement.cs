using NauraLauncher.Common;

namespace NauraLauncher.Models;

/// <summary>
/// A bid increment chip in the auction bid panel (+100 / +250 / +500 / +1K).
/// </summary>
public class BidIncrement : ObservableObject
{
    public string Label { get; set; } = string.Empty;      // "+1K"
    public double Value { get; set; }                      // 1000

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
