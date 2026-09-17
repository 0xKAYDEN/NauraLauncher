using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NauraLauncher.Core.Entities;
using NauraLauncher.Models;

namespace NauraLauncher.Core.Interfaces;

public interface IAuctionService
{
    Task<AuctionLotData?> GetActiveLotAsync(string lotId);
    Task<bool> PlaceBidAsync(string lotId, double increment);
    Task<bool> BuyoutAsync(string lotId);
    Task<IReadOnlyList<HammerPrice>> GetHammerHistoryAsync();
    Task<IReadOnlyList<VaultDrop>> GetVaultDropsAsync();

    event EventHandler<(string LotId, double NewBid, int BidCount, string TopBidder, string TopBidderMeta)>? BidBroadcastReceived;
}
