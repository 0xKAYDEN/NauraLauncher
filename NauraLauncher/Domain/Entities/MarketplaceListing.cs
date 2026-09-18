using NauraLauncher.Domain.Enums;

namespace NauraLauncher.Domain.Entities;

public enum ListingStatus
{
    Active = 0,
    Sold = 1,
    Cancelled = 2,
    Expired = 3
}

public class MarketplaceListing
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int ItemId { get; set; }                    // Reference to InventoryItem
    public InventoryItem? Item { get; set; }

    public long Price { get; set; }
    public CurrencyType Currency { get; set; } = CurrencyType.Cps;

    public ListingStatus Status { get; set; } = ListingStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? SoldAt { get; set; }
    public int? BuyerId { get; set; }

    // For display
    public string Title => Item?.GetDisplayName() ?? "Unknown Item";
    public string RarityHex => Item?.GetRarityHex() ?? "#8A8F99";
}

public class MarketplaceTransaction
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public int SellerId { get; set; }
    public int BuyerId { get; set; }
    public long Price { get; set; }
    public CurrencyType Currency { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
