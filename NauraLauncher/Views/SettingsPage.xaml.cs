using System.Windows.Controls;

namespace NauraLauncher.Views;

/// <summary>
/// Settings page — sections rail plus grouped Switch, Slider and segmented
/// control rows. DataContext is <c>SettingsViewModel</c>, supplied by
/// MainViewModel.
/// </summary>
public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }
}
