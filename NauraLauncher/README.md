# NauraLauncher — Conquer Online Edition

Conquer Online launcher — WPF / .NET 8, clean architecture, no third-party dependencies for UI.

## Quick Start

```powershell
dotnet restore
dotnet run
```

Login with `admin` / `admin123` or any mock hero: DragonLord, FireQueen, ShadowNinja, HolyMonk, PirateKing (any password for mock).

## What's New - Conquer Online Overhaul

### Auth & User Panel
- `Views/AuthPage.xaml` - Split hero (Twin City) + login/register forms, demo accounts info
- `MainWindow.xaml` - User area clickable → dropdown with Profile, Inventory, Friends & Voice, Logout
- `ViewModels/AuthViewModel.cs` - Login/Register with mode switching, PBKDF2 hashing
- `ViewModels/MainViewModel.cs` - Auth state, wallet (CPs, Gold), user menu, propagates UserId to sub VMs

### Wallet - CPs, Gold, Silver
- `Domain/Entities/User.cs` - User with Wallet (Cps, BoundCps, Gold, Silver)
- `Domain/Enums/CurrencyType.cs` - Cps, Gold, Silver, BoundCps with display names
- `Application/Services/WalletService.cs` - Add, Deduct, Transfer
- Header shows CPs (emerald gem) + Gold (gold coin) pills

### Inventory
- `Domain/Entities/InventoryItem.cs` - Conquer items: Dragon Blade +12 Super 2-socket, Super Dragon Gem, Dragon Ball x27, etc.
- `ViewModels/InventoryViewModel.cs` - Filter by type, search, select
- `Views/InventoryPage.xaml` - Grid + detail panel with trade options

### Marketplace - Twin City Market
- `Domain/Entities/MarketplaceListing.cs` - Listing with price, currency, status, seller, item
- `Application/Services/MarketplaceService.cs` - Create listing (removes from inventory), Cancel (returns item), Buy (deduct buyer, pay seller, transfer item)
- `ViewModels/MarketplaceViewModel.cs` - Tabs: Browse, My Listings, Sell (from inventory)
- `Views/MarketplacePage.xaml` - Hero spotlight + listings with Buy + My Listings + Sell from inventory

### Auction House
- `Domain/Entities/AuctionLot.cs` - Lot with starting bid, current bid, buyout, bidder, expiry
- `Application/Services/AuctionService.cs` - Create lot, Cancel (if no bids), Place bid (refund previous), Buyout
- `ViewModels/AuctionViewModel.cs` - Tabs: Browse, My Lots, Sell, plus live lot with countdown and bid panel
- `Views/AuctionPage.xaml` - Summary cards + active lots list + live lot + bid panel + My Lots + Sell

### Friends & Voice
- `Domain/Entities/Friend.cs` - Friendship, BlockedUser, FriendRequest
- `Domain/Entities/VoiceCall.cs` - VoiceChannel, VoiceCall, VoiceSignalingMessage
- `Application/Services/FriendsService.cs` - Add (auto-accept mock), Remove, Block/Unblock, Requests, Search
- `Application/Services/VoiceService.cs` - Channel management, call lifecycle, signaling event
- `ViewModels/FriendsViewModel.cs` - Friends, Online, Requests, Blocked, Search, Voice calls
- `Views/FriendsPage.xaml` - Online friends with voice call buttons, requests, blocked, search

### Backend Server
- `NauraLauncher.Server/Program.cs` - Starts API (8080) + Voice (8081), checks MySQL 5.6 compat
- `NauraLauncher.Server/Api/ApiServer.cs` - REST: auth, marketplace, auction, friends, inventory, wallet
- `NauraLauncher.Server/Realtime/VoiceSignalingServer.cs` - WebSocket signaling for WebRTC: offer/answer/ice-candidate, call lifecycle
- `NauraLauncher.Server/Infrastructure/` - DatabaseConfig, MySqlConnectionFactory (MySqlConnector 2.3.7 supports MySQL 5.6)
- `Database/schema.sql` - MySQL 5.6 compatible: no JSON (TEXT), TIMESTAMP DEFAULT CURRENT_TIMESTAMP, InnoDB, utf8, BIGINT for CPs/Gold

### Conquer Online Theming
- Assets: hero_conquer.png (Twin City, dragons), card_trojan.png, card_warrior.png, card_archer.png, card_taoist.png, card_ninja.png, card_monk.png, avatar_conquer.png, item_dragonball.png
- Icons: Added Sword, Gem, Gold, Silver, Coin, Dragon, Inventory, Logout, Phone, Block, UserPlus, UserMinus
- ViewModels: Home shows Twin City, Battle Power, Wealth, Enter Twin City, news about Dragon Gem auction, Patch 7009 Ninja skills, Guild War
- MainWindow: NAURA logo with dragon, v2.4.0 CO 7009, Eternity server, status Twin City Connected

### Clean Architecture
```
Domain/Entities      - Pure business objects (User, Wallet, InventoryItem, etc.)
Domain/Enums         - CurrencyType, ConquerClass, ItemType, ItemRarity
Application/Interfaces - IAuthService, IInventoryService, IMarketplaceService, etc.
Application/Services   - In-memory implementations for demo, ready for MySQL repos
Infrastructure/      - Security (PasswordHasher PBKDF2), Persistence (MySqlConnectionFactory), Voice
Services/            - ApiClient (HTTP), VoiceClient (WebSocket)
ViewModels/          - MVVM with ObservableObject, RelayCommand, async loading
Views/               - XAML pages with Conquer theming
```

## Build

```powershell
cd NauraLauncher
dotnet restore
dotnet run
```

Requires .NET 8 SDK on Windows (WPF is Windows-only).

## Tests

```bash
python3 -m unittest discover -s tests -v
```

Checks XAML validity, Conquer assets exist, domain entities exist, MySQL 5.6 schema compatible.

## Server

```bash
cd ../NauraLauncher.Server
dotnet run
```

API on 8080, Voice on 8081, in-memory demo (no MySQL needed). For MySQL 5.6, set connection string and UseInMemory=false.

## Docs

- `../docs/ARCHITECTURE.md` - Clean architecture layers, MySQL 5.6 compat, Conquer theming
- `../docs/PLAN.md` - What is done, prototype, demo, missing, known issues fixed
