using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;

namespace NauraLauncher.Application.Services;

/// <summary>
/// Provides mock data for demo/prototype - Conquer Online themed
/// </summary>
public static class MockDataService
{
    public static List<InventoryItem> GenerateMockInventory(int userId)
    {
        return new List<InventoryItem>
        {
            new() {
                Id = 1, UserId = userId, Name = "Dragon Blade", Description = "Legendary blade forged in Twin City, +12 with 2 sockets",
                Type = ItemType.Weapon, Rarity = ItemRarity.Legendary, LevelRequired = 120,
                Plus = 12, SocketCount = 2, SocketGems = "Super Dragon Gem, Super Phoenix Gem",
                IconPath = "Assets/card_trojan.png", ImagePath = "Assets/card_trojan.png",
                Quantity = 1, IsTradable = true
            },
            new() {
                Id = 2, UserId = userId, Name = "Super Dragon Gem", Description = "Increases attack power significantly",
                Type = ItemType.Gem, Rarity = ItemRarity.Super, LevelRequired = 1,
                IconPath = "Assets/item_dragonball.png", ImagePath = "Assets/card_monk.png",
                Quantity = 5, IsTradable = true
            },
            new() {
                Id = 3, UserId = userId, Name = "Dragon Ball", Description = "Mysterious orb containing dragon power, used for rebirth and upgrades",
                Type = ItemType.Consumable, Rarity = ItemRarity.Epic, LevelRequired = 1,
                IconPath = "Assets/item_dragonball.png", ImagePath = "Assets/card_archer.png",
                Quantity = 27, IsTradable = true
            },
            new() {
                Id = 4, UserId = userId, Name = "Heaven Fan", Description = "Water Taoist fan, +9 with healing boost",
                Type = ItemType.Weapon, Rarity = ItemRarity.Epic, LevelRequired = 110,
                Plus = 9, SocketCount = 1, SocketGems = "Super Moon Gem",
                IconPath = "Assets/card_taoist.png", ImagePath = "Assets/card_taoist.png",
                Quantity = 1, IsTradable = true
            },
            new() {
                Id = 5, UserId = userId, Name = "Meteor Scroll", Description = "Fire Taoist spell scroll - Meteor",
                Type = ItemType.Consumable, Rarity = ItemRarity.Rare, LevelRequired = 40,
                IconPath = "Assets/card_warrior.png", ImagePath = "Assets/card_warrior.png",
                Quantity = 10, IsTradable = true
            },
            new() {
                Id = 6, UserId = userId, Name = "Ninja Katana", Description = "Twin katanas, +8, poison effect",
                Type = ItemType.Weapon, Rarity = ItemRarity.Elite, LevelRequired = 100,
                Plus = 8, SocketCount = 2,
                IconPath = "Assets/card_ninja.png", ImagePath = "Assets/card_ninja.png",
                Quantity = 1, IsTradable = true
            },
            new() {
                Id = 7, UserId = userId, Name = "Super Armor - Trojan", Description = "Super Conquer Armor for Trojan, +12, 2 sockets",
                Type = ItemType.Armor, Rarity = ItemRarity.Super, LevelRequired = 120,
                Plus = 12, SocketCount = 2,
                IconPath = "Assets/card_trojan.png", ImagePath = "Assets/card_trojan.png",
                Quantity = 1, IsTradable = true
            },
            new() {
                Id = 8, UserId = userId, Name = "Pirate Rapier", Description = "Pirate's rapier with critical strike bonus",
                Type = ItemType.Weapon, Rarity = ItemRarity.Rare, LevelRequired = 70,
                Plus = 5,
                IconPath = "Assets/card_warrior.png", ImagePath = "Assets/card_warrior.png",
                Quantity = 1, IsTradable = true
            },
        };
    }

    public static List<MarketplaceListing> GenerateMockMarketplace(List<InventoryItem> allItems, List<User> users)
    {
        var rnd = new Random();
        var listings = new List<MarketplaceListing>();
        int id = 1;
        foreach (var item in allItems.Take(20))
        {
            var seller = users[rnd.Next(users.Count)];
            listings.Add(new MarketplaceListing
            {
                Id = id++,
                SellerId = seller.Id,
                SellerName = seller.DisplayName,
                ItemId = item.Id,
                Item = item,
                Price = rnd.Next(10, 5000),
                Currency = rnd.Next(0, 2) == 0 ? CurrencyType.Cps : CurrencyType.Gold,
                Status = Domain.Entities.ListingStatus.Active,
                CreatedAt = DateTime.UtcNow.AddHours(-rnd.Next(1, 72))
            });
        }
        return listings;
    }

    public static List<User> GenerateMockUsers()
    {
        return new List<User>
        {
            new() { Id = 1, Username = "DragonLord", DisplayName = "DragonLord", MainClass = ConquerClass.Trojan, Level = 130, Reborn = RebornStage.SecondReborn, IsOnline = true, AvatarPath = "Assets/avatar_conquer.png" },
            new() { Id = 2, Username = "FireQueen", DisplayName = "FireQueen", MainClass = ConquerClass.FireTaoist, Level = 125, Reborn = RebornStage.FirstReborn, IsOnline = true, AvatarPath = "Assets/avatar_conquer.png" },
            new() { Id = 3, Username = "ShadowNinja", DisplayName = "ShadowNinja", MainClass = ConquerClass.Ninja, Level = 120, Reborn = RebornStage.SecondReborn, IsOnline = false, AvatarPath = "Assets/avatar_conquer.png" },
            new() { Id = 4, Username = "HolyMonk", DisplayName = "HolyMonk", MainClass = ConquerClass.Monk, Level = 115, Reborn = RebornStage.FirstReborn, IsOnline = true, AvatarPath = "Assets/avatar_conquer.png" },
            new() { Id = 5, Username = "PirateKing", DisplayName = "PirateKing", MainClass = ConquerClass.Pirate, Level = 130, Reborn = RebornStage.SecondReborn, IsOnline = true, AvatarPath = "Assets/avatar_conquer.png" },
        };
    }

    public static List<AuctionLot> GenerateMockAuctions(List<InventoryItem> items, List<User> users)
    {
        var rnd = new Random();
        var lots = new List<AuctionLot>();
        int id = 1;
        foreach (var item in items.Take(10))
        {
            var seller = users[rnd.Next(users.Count)];
            var starting = rnd.Next(100, 2000);
            lots.Add(new AuctionLot
            {
                Id = id++,
                SellerId = seller.Id,
                SellerName = seller.DisplayName,
                ItemId = item.Id,
                Item = item,
                StartingPrice = starting,
                CurrentBid = starting + rnd.Next(0, 500),
                CurrentBidderId = users[rnd.Next(users.Count)].Id,
                CurrentBidderName = users[rnd.Next(users.Count)].DisplayName,
                BuyoutPrice = starting * 3,
                Currency = CurrencyType.Cps,
                BidCount = rnd.Next(1, 50),
                EndsAt = DateTime.UtcNow.AddHours(rnd.Next(1, 48))
            });
        }
        return lots;
    }

    public static List<Friendship> GenerateMockFriends(int userId, List<User> allUsers)
    {
        return allUsers.Where(u => u.Id != userId).Select(u => new Friendship
        {
            Id = u.Id,
            UserId = userId,
            FriendId = u.Id,
            FriendName = u.DisplayName,
            FriendAvatar = u.AvatarPath,
            FriendIsOnline = u.IsOnline,
            FriendLevel = u.Level,
            FriendClass = u.MainClass.ToDisplayName(),
            Status = FriendStatus.Accepted,
            AcceptedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 30))
        }).ToList();
    }
}
