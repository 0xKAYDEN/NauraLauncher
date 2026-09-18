using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;

namespace NauraLauncher.Core.Interfaces;

public interface IAuctionService
{
    Task<List<AuctionLot>> GetActiveLotsAsync(int page = 0, int pageSize = 20, ItemRarity? rarity = null);
    Task<List<AuctionLot>> GetUserLotsAsync(int userId);
    Task<List<AuctionLot>> GetUserBidsAsync(int userId);
    Task<AuctionLot?> GetLotByIdAsync(int lotId);
    Task<AuctionLot> CreateLotAsync(int sellerId, int itemId, long startingPrice, long buyoutPrice, CurrencyType currency, TimeSpan duration);
    Task<bool> CancelLotAsync(int userId, int lotId);
    Task<AuctionBid> PlaceBidAsync(int bidderId, int lotId, long amount);
    Task<bool> BuyoutAsync(int buyerId, int lotId);
    Task<List<AuctionBid>> GetBidHistoryAsync(int lotId);
    Task<List<AuctionLot>> SearchAsync(string query);
}
