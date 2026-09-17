using System;
using System.Collections.Generic;

namespace NauraLauncher.Core.Entities;

/// <summary>
/// Domain model for active auction lots.
/// </summary>
public class AuctionLotData
{
    public string Id { get; set; } = string.Empty;
    public string LotCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SerialTag { get; set; } = string.Empty;
    public string Rarity { get; set; } = "OBSIDIAN";
    public string ToneHex { get; set; } = "#34D399";
    public string ImagePath { get; set; } = string.Empty;
    public string CertLabel { get; set; } = "CERTIFIED";
    public string CertCode { get; set; } = string.Empty;
    public string Provenance { get; set; } = string.Empty;
    public double FloorPrice { get; set; }
    public double BuyoutPrice { get; set; }
    public double CurrentBid { get; set; }
    public int BidCount { get; set; }
    public string TopBidderName { get; set; } = string.Empty;
    public string TopBidderMeta { get; set; } = string.Empty;
    public DateTime EndsAt { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public List<KeyValuePair<string, string>> Attributes { get; set; } = new();
}
