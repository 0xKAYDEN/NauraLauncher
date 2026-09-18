using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;

namespace NauraLauncher.Application.Interfaces;

public interface IMarketplaceService
{
    Task<List<MarketplaceListing>> GetActiveListingsAsync(int page = 0, int pageSize = 20, ItemType? filterType = null, ItemRarity? filterRarity = null);
    Task<List<MarketplaceListing>> GetUserListingsAsync(int userId);
    Task<MarketplaceListing?> GetListingByIdAsync(int listingId);
    Task<MarketplaceListing> CreateListingAsync(int sellerId, int itemId, long price, CurrencyType currency);
    Task<bool> CancelListingAsync(int userId, int listingId);
    Task<MarketplaceTransaction> BuyItemAsync(int buyerId, int listingId);
    Task<List<MarketplaceListing>> SearchAsync(string query, ItemType? type = null);
}
