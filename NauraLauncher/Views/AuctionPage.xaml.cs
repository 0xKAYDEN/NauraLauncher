using System.Windows.Controls;

namespace NauraLauncher.Views;

/// <summary>
/// Auction page — rarity filters, escrow / watchlist summary, the live lot with
/// countdown, the bid panel, provenance table, hammer-price feed and the vault
/// drop carousel. DataContext is <c>AuctionViewModel</c>, supplied by
/// MainViewModel.
/// </summary>
public partial class AuctionPage : UserControl
{
    public AuctionPage()
    {
        InitializeComponent();
    }
}
