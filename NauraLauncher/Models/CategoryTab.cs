using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NauraLauncher.Models;

/// <summary>
/// A filter chip in the toolbar (All Entries, Tactical RPG, ...).
/// </summary>
public class CategoryTab : INotifyPropertyChanged
{
    public string Name { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
