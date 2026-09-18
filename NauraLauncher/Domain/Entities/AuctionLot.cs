using NauraLauncher.Domain.Enums;

namespace NauraLauncher.Domain.Entities;

public enum AuctionStatus
{
    Active = 0,
    Sold = 1,
    Expired = 2,
    Cancelled = 3
}

public class AuctionLot
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public InventoryItem? Item { get; set; }

    public long StartingPrice { get; set; }
    public long CurrentBid { get; set; }
    public int? CurrentBidderId { get; set; }
    public string? CurrentBidderName { get; set; }
    public long BuyoutPrice { get; set; }              // 0 = no buyout
    public CurrencyType Currency { get; set; } = CurrencyType.Cps;

    public int BidCount { get; set; } = 0;
    public AuctionStatus Status { get; set; } = AuctionStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime EndsAt { get; set; } = DateTime.UtcNow.AddDays(3);
    public DateTime? SoldAt { get; set; }

    public TimeSpan TimeRemaining => EndsAt - DateTime.UtcNow;
    public bool IsExpired => DateTime.UtcNow >= EndsAt;

    public string Title => Item?.GetDisplayName() ?? "Unknown Lot";
}

public class AuctionBid
{
    public int Id { get; set; }
    public int LotId { get; set; }
    public int BidderId { get; set; }
    public string BidderName { get; set; } = string.Empty;
    public long Amount { get; set; }
    public CurrencyType Currency { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
}
