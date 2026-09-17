using System.Windows.Controls;

namespace NauraLauncher.Views;

/// <summary>
/// Marketplace page — the original single-window content, now hosted by the
/// shell. DataContext is <c>MarketplaceViewModel</c>, supplied by MainViewModel.
/// </summary>
public partial class MarketplacePage : UserControl
{
    public MarketplacePage()
    {
        InitializeComponent();
    }
}
