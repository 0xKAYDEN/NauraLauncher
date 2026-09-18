using NauraLauncher.Application.Interfaces;
using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;

namespace NauraLauncher.Application.Services;

public class AuctionService : IAuctionService
{
    private static readonly List<AuctionLot> _lots = new();
    private static readonly List<AuctionBid> _bids = new();
    private static int _nextLotId = 1;
    private static int _nextBidId = 1;

    static AuctionService()
    {
        var users = AuthService.GetAllUsers();
        var allItems = InventoryService.GetAllInventories().SelectMany(kv => kv.Value).ToList();
        var mockLots = MockDataService.GenerateMockAuctions(allItems, users);
        _lots.AddRange(mockLots);
        _nextLotId = _lots.Max(l => l.Id) + 1;
    }

    public Task<List<AuctionLot>> GetActiveLotsAsync(int page = 0, int pageSize = 20, ItemRarity? rarity = null)
    {
        var query = _lots.Where(l => l.Status == AuctionStatus.Active && !l.IsExpired).AsQueryable();
        if (rarity.HasValue)
            query = query.Where(l => l.Item != null && l.Item.Rarity == rarity.Value);

        var result = query.OrderBy(l => l.EndsAt).Skip(page * pageSize).Take(pageSize).ToList();
        return Task.FromResult(result);
    }

    public Task<List<AuctionLot>> GetUserLotsAsync(int userId)
    {
        var result = _lots.Where(l => l.SellerId == userId).OrderByDescending(l => l.CreatedAt).ToList();
        return Task.FromResult(result);
    }

    public Task<List<AuctionLot>> GetUserBidsAsync(int userId)
    {
        var lotIds = _bids.Where(b => b.BidderId == userId).Select(b => b.LotId).Distinct().ToList();
        var result = _lots.Where(l => lotIds.Contains(l.Id)).ToList();
        return Task.FromResult(result);
    }

    public Task<AuctionLot?> GetLotByIdAsync(int lotId)
    {
        return Task.FromResult(_lots.FirstOrDefault(l => l.Id == lotId));
    }

    public async Task<AuctionLot> CreateLotAsync(int sellerId, int itemId, long startingPrice, long buyoutPrice, CurrencyType currency, TimeSpan duration)
    {
        var inventoryService = new InventoryService();
        var item = await inventoryService.GetItemByIdAsync(itemId);
        if (item == null) throw new InvalidOperationException("Item not found");
        if (item.UserId != sellerId) throw new InvalidOperationException("Not your item");
        if (!item.IsTradable || item.IsBound) throw new InvalidOperationException("Item not tradable");

        await inventoryService.RemoveItemAsync(sellerId, itemId);

        var seller = AuthService.GetAllUsers().FirstOrDefault(u => u.Id == sellerId);
        var lot = new AuctionLot
        {
            Id = _nextLotId++,
            SellerId = sellerId,
            SellerName = seller?.DisplayName ?? $"Player{sellerId}",
            ItemId = itemId,
            Item = item,
            StartingPrice = startingPrice,
            CurrentBid = startingPrice,
            BuyoutPrice = buyoutPrice,
            Currency = currency,
            Status = AuctionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.Add(duration)
        };

        _lots.Add(lot);
        return lot;
    }

    public Task<bool> CancelLotAsync(int userId, int lotId)
    {
        var lot = _lots.FirstOrDefault(l => l.Id == lotId);
        if (lot == null || lot.SellerId != userId) return Task.FromResult(false);
        if (lot.Status != AuctionStatus.Active) return Task.FromResult(false);
        if (lot.BidCount > 0) return Task.FromResult(false); // Can't cancel if bids exist

        lot.Status = AuctionStatus.Cancelled;

        if (lot.Item != null)
        {
            var invService = new InventoryService();
            _ = invService.AddItemAsync(userId, lot.Item);
        }

        return Task.FromResult(true);
    }

    public async Task<AuctionBid> PlaceBidAsync(int bidderId, int lotId, long amount)
    {
        var lot = _lots.FirstOrDefault(l => l.Id == lotId);
        if (lot == null) throw new InvalidOperationException("Lot not found");
        if (lot.Status != AuctionStatus.Active) throw new InvalidOperationException("Lot not active");
        if (lot.IsExpired) throw new InvalidOperationException("Lot expired");
        if (lot.SellerId == bidderId) throw new InvalidOperationException("Cannot bid on your own lot");
        if (amount <= lot.CurrentBid) throw new InvalidOperationException("Bid must be higher than current bid");

        var walletService = new WalletService();
        var bidderWallet = await walletService.GetWalletAsync(bidderId);

        // Check if bidder has enough - in real auction we would hold funds, here we just check
        if (bidderWallet.GetBalance(lot.Currency) < amount)
            throw new InvalidOperationException($"Not enough {lot.Currency.ToDisplayName()}");

        // Refund previous bidder
        if (lot.CurrentBidderId.HasValue)
        {
            await walletService.AddCurrencyAsync(lot.CurrentBidderId.Value, lot.Currency, lot.CurrentBid);
        }

        // Deduct from new bidder
        if (!bidderWallet.TryDeduct(lot.Currency, amount))
            throw new InvalidOperationException("Failed to deduct currency");

        var bidder = AuthService.GetAllUsers().FirstOrDefault(u => u.Id == bidderId);

        var bid = new AuctionBid
        {
            Id = _nextBidId++,
            LotId = lotId,
            BidderId = bidderId,
            BidderName = bidder?.DisplayName ?? $"Player{bidderId}",
            Amount = amount,
            Currency = lot.Currency,
            PlacedAt = DateTime.UtcNow
        };

        _bids.Add(bid);
        lot.CurrentBid = amount;
        lot.CurrentBidderId = bidderId;
        lot.CurrentBidderName = bid.BidderName;
        lot.BidCount++;

        return bid;
    }

    public async Task<bool> BuyoutAsync(int buyerId, int lotId)
    {
        var lot = _lots.FirstOrDefault(l => l.Id == lotId);
        if (lot == null) return false;
        if (lot.Status != AuctionStatus.Active) return false;
        if (lot.BuyoutPrice <= 0) return false;
        if (lot.SellerId == buyerId) return false;

        var walletService = new WalletService();
        var buyerWallet = await walletService.GetWalletAsync(buyerId);
        if (!buyerWallet.TryDeduct(lot.Currency, lot.BuyoutPrice)) return false;

        // Refund current bidder if any
        if (lot.CurrentBidderId.HasValue)
        {
            await walletService.AddCurrencyAsync(lot.CurrentBidderId.Value, lot.Currency, lot.CurrentBid);
        }

        // Pay seller
        await walletService.AddCurrencyAsync(lot.SellerId, lot.Currency, lot.BuyoutPrice);

        // Transfer item
        if (lot.Item != null)
        {
            var invService = new InventoryService();
            lot.Item.UserId = buyerId;
            await invService.AddItemAsync(buyerId, lot.Item);
        }

        lot.Status = AuctionStatus.Sold;
        lot.SoldAt = DateTime.UtcNow;
        lot.CurrentBidderId = buyerId;
        return true;
    }

    public Task<List<AuctionBid>> GetBidHistoryAsync(int lotId)
    {
        var result = _bids.Where(b => b.LotId == lotId).OrderByDescending(b => b.PlacedAt).ToList();
        return Task.FromResult(result);
    }

    public Task<List<AuctionLot>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetActiveLotsAsync();

        var lower = query.ToLowerInvariant();
        var result = _lots.Where(l =>
            l.Status == AuctionStatus.Active &&
            l.Item != null &&
            (l.Item.Name.ToLowerInvariant().Contains(lower) || l.Item.Description.ToLowerInvariant().Contains(lower))
        ).ToList();

        return Task.FromResult(result);
    }
}
