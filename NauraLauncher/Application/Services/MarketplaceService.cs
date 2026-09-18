using NauraLauncher.Application.Interfaces;
using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;

namespace NauraLauncher.Application.Services;

public class MarketplaceService : IMarketplaceService
{
    private static readonly List<MarketplaceListing> _listings = new();
    private static readonly List<MarketplaceTransaction> _transactions = new();
    private static int _nextId = 1;

    static MarketplaceService()
    {
        var users = AuthService.GetAllUsers();
        var allItems = InventoryService.GetAllInventories().SelectMany(kv => kv.Value).ToList();
        var mockListings = MockDataService.GenerateMockMarketplace(allItems, users);
        _listings.AddRange(mockListings);
        _nextId = _listings.Max(l => l.Id) + 1;
    }

    public Task<List<MarketplaceListing>> GetActiveListingsAsync(int page = 0, int pageSize = 20, ItemType? filterType = null, ItemRarity? filterRarity = null)
    {
        var query = _listings.Where(l => l.Status == ListingStatus.Active).AsQueryable();
        if (filterType.HasValue)
            query = query.Where(l => l.Item != null && l.Item.Type == filterType.Value);
        if (filterRarity.HasValue)
            query = query.Where(l => l.Item != null && l.Item.Rarity == filterRarity.Value);

        var result = query.OrderByDescending(l => l.CreatedAt).Skip(page * pageSize).Take(pageSize).ToList();
        return Task.FromResult(result);
    }

    public Task<List<MarketplaceListing>> GetUserListingsAsync(int userId)
    {
        var result = _listings.Where(l => l.SellerId == userId).OrderByDescending(l => l.CreatedAt).ToList();
        return Task.FromResult(result);
    }

    public Task<MarketplaceListing?> GetListingByIdAsync(int listingId)
    {
        return Task.FromResult(_listings.FirstOrDefault(l => l.Id == listingId));
    }

    public async Task<MarketplaceListing> CreateListingAsync(int sellerId, int itemId, long price, CurrencyType currency)
    {
        var inventoryService = new InventoryService();
        var item = await inventoryService.GetItemByIdAsync(itemId);
        if (item == null) throw new InvalidOperationException("Item not found");
        if (item.UserId != sellerId) throw new InvalidOperationException("Not your item");
        if (!item.IsTradable || item.IsBound) throw new InvalidOperationException("Item not tradable");

        // Remove from inventory and create listing
        await inventoryService.RemoveItemAsync(sellerId, itemId);

        var seller = AuthService.GetAllUsers().FirstOrDefault(u => u.Id == sellerId);
        var listing = new MarketplaceListing
        {
            Id = _nextId++,
            SellerId = sellerId,
            SellerName = seller?.DisplayName ?? $"Player{sellerId}",
            ItemId = itemId,
            Item = item,
            Price = price,
            Currency = currency,
            Status = ListingStatus.Active,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _listings.Add(listing);
        return listing;
    }

    public Task<bool> CancelListingAsync(int userId, int listingId)
    {
        var listing = _listings.FirstOrDefault(l => l.Id == listingId);
        if (listing == null || listing.SellerId != userId) return Task.FromResult(false);
        if (listing.Status != ListingStatus.Active) return Task.FromResult(false);

        listing.Status = ListingStatus.Cancelled;

        // Return item to inventory
        if (listing.Item != null)
        {
            var invService = new InventoryService();
            // Fire and forget for demo - in real would await
            _ = invService.AddItemAsync(userId, listing.Item);
        }

        return Task.FromResult(true);
    }

    public async Task<MarketplaceTransaction> BuyItemAsync(int buyerId, int listingId)
    {
        var listing = _listings.FirstOrDefault(l => l.Id == listingId);
        if (listing == null) throw new InvalidOperationException("Listing not found");
        if (listing.Status != ListingStatus.Active) throw new InvalidOperationException("Listing not active");
        if (listing.SellerId == buyerId) throw new InvalidOperationException("Cannot buy your own item");

        var walletService = new WalletService();
        var buyerWallet = await walletService.GetWalletAsync(buyerId);
        if (!buyerWallet.TryDeduct(listing.Currency, listing.Price))
            throw new InvalidOperationException($"Not enough {listing.Currency.ToDisplayName()}");

        // Pay seller
        await walletService.AddCurrencyAsync(listing.SellerId, listing.Currency, listing.Price);

        // Transfer item
        if (listing.Item != null)
        {
            var invService = new InventoryService();
            listing.Item.UserId = buyerId;
            await invService.AddItemAsync(buyerId, listing.Item);
        }

        listing.Status = ListingStatus.Sold;
        listing.BuyerId = buyerId;
        listing.SoldAt = DateTime.UtcNow;

        var transaction = new MarketplaceTransaction
        {
            Id = _transactions.Count + 1,
            ListingId = listingId,
            SellerId = listing.SellerId,
            BuyerId = buyerId,
            Price = listing.Price,
            Currency = listing.Currency,
            CompletedAt = DateTime.UtcNow
        };
        _transactions.Add(transaction);

        return transaction;
    }

    public Task<List<MarketplaceListing>> SearchAsync(string query, ItemType? type = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetActiveListingsAsync();

        var lower = query.ToLowerInvariant();
        var result = _listings.Where(l =>
            l.Status == ListingStatus.Active &&
            (l.Item != null && (l.Item.Name.ToLowerInvariant().Contains(lower) || l.Item.Description.ToLowerInvariant().Contains(lower)))
        ).ToList();

        if (type.HasValue)
            result = result.Where(l => l.Item != null && l.Item.Type == type.Value).ToList();

        return Task.FromResult(result);
    }
}
