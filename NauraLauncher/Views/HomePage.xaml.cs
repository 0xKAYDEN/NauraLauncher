using System.Windows.Controls;

namespace NauraLauncher.Views;

/// <summary>
/// Home page — greeting, quick stats, the Continue Playing hero, the Latest
/// Intel feed and the installed-library grid. DataContext is
/// <c>HomeViewModel</c>, supplied by MainViewModel.
/// </summary>
public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
    }
}
