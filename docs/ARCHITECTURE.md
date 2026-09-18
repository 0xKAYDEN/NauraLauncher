# NauraLauncher - Conquer Online Architecture

## Overview
NauraLauncher is now a Conquer Online themed launcher with clean architecture, supporting:
- Authentication (Login/Register)
- User Panel (Profile, Inventory, Logout)
- Wallet with 4 currencies (CPs, Gold, Silver, Bound CPs)
- Marketplace with buy/sell flow from inventory
- Auction House with bidding and buyout
- Friends system (add, remove, block/unblock)
- Voice calls between players (WebSocket signaling + WebRTC)

## Clean Architecture Layers

### 1. Domain Layer (`NauraLauncher/Domain/`)
Pure business entities, no dependencies.

**Entities:**
- `User` - Conquer Online hero account
- `Wallet` - 4 currencies: CPs, Gold, Silver, BoundCps
- `InventoryItem` - Weapons, Armor, Gems, Dragon Balls, etc.
- `MarketplaceListing` - Twin City Market listings
- `AuctionLot` + `AuctionBid` - Auction house
- `Friendship`, `BlockedUser`, `FriendRequest` - Social
- `VoiceChannel`, `VoiceCall`, `VoiceSignalingMessage` - Voice

**Enums:**
- `CurrencyType` - Cps, Gold, Silver, BoundCps
- `ConquerClass` - Trojan, Warrior, Archer, FireTaoist, WaterTaoist, Ninja, Monk, Pirate, DragonWarrior
- `ItemType` - Weapon, Armor, Garment, Gem, Consumable, Mount, etc.
- `ItemRarity` - Common, Rare, Elite, Super, Epic, Legendary, Mythic

### 2. Application Layer (`NauraLauncher/Application/`)
Business logic, interfaces, services. Depends only on Domain.

**Interfaces:**
- `IAuthService` - Login/Register
- `IInventoryService` + `IWalletService`
- `IMarketplaceService` - Buy/Sell with currency flow
- `IAuctionService` - Bid/Buyout
- `IFriendsService` - Add/Remove/Block
- `IVoiceService` - Voice channels and calls

**Services (In-Memory for Demo):**
- `AuthService` - Mock users + admin/admin123, PBKDF2 hashing
- `InventoryService` - Generates Conquer items: Dragon Blade +12, Super Dragon Gem, Dragon Balls
- `WalletService` - Currency add/deduct/transfer
- `MarketplaceService` - Listing creation, buy transaction (deduct buyer, pay seller, transfer item)
- `AuctionService` - Bid handling with refund of previous bidder, buyout
- `FriendsService` - Bidirectional friendships, block prevents trade/voice
- `VoiceService` - Channel management, call initiation

**MockDataService:**
- Generates Conquer Online themed inventory, marketplace, auction, friends

### 3. Infrastructure Layer (`NauraLauncher/Infrastructure/` + `NauraLauncher.Server/Infrastructure/`)
External concerns: Security, Persistence, Real-time.

**Security:**
- `PasswordHasher` - PBKDF2 with salt, 10000 iterations, SHA256 - MySQL 5.6 safe

**Persistence:**
- `MySqlConnectionFactory` - Uses MySqlConnector 2.3.7 (supports MySQL 5.6)
- `MySqlRepositoryBase` - Base for repositories, pagination with LIMIT offset,count
- Schema in `Database/schema.sql` - MySQL 5.6 compatible (see notes below)

**Voice:**
- `VoiceSignalingServer` - WebSocket server on port 8081
- Protocol: JSON { type, channelId, callId, fromUserId, toUserId, payload }
- Types: call-incoming, call-accepted, offer, answer, ice-candidate, etc.
- Handles channel join/leave, call lifecycle

**Services:**
- `ApiClient` - HTTP client for launcher to talk to server
- `VoiceClient` - WebSocket client for voice signaling

### 4. Presentation Layer (`NauraLauncher/ViewModels/` + `NauraLauncher/Views/`)
WPF MVVM UI.

**ViewModels:**
- `MainViewModel` - Shell coordinator, auth state, wallet display (CPs, Gold, Silver), user menu
- `AuthViewModel` - Login/Register with mode switching
- `HomeViewModel` - Conquer Online themed: Twin City, Battle Power, Guild War news
- `MarketplaceViewModel` - Tabs: Browse, My Listings, Sell (from inventory), Buy flow
- `AuctionViewModel` - Tabs: Browse, My Lots, Sell, Bidding with increments, Buyout
- `InventoryViewModel` - Filter by type, search, select item detail
- `ProfileViewModel` - User info, wallet cards, stats, class progression
- `FriendsViewModel` - Friends list, online, requests, blocked, voice calls, search, add friend
- `SettingsViewModel` - Client settings (existing)

**Views:**
- `AuthPage.xaml` - Split hero + login/register forms, demo accounts info
- `HomePage.xaml` - Conquer themed hero (Twin City), stats, character library
- `MarketplacePage.xaml` - Hero spotlight + Twin City Market listings + My Listings + Sell from inventory
- `AuctionPage.xaml` - Summary cards + active lots list + live lot + bid panel + My Lots/Bids + Sell
- `InventoryPage.xaml` - Grid of items + detail panel with trade options
- `ProfilePage.xaml` - Avatar, wallet cards (CPs, Gold, Silver, Bound CPs), stats, class progression
- `FriendsPage.xaml` - Online friends with voice call buttons, all friends, requests, blocked, search
- `SettingsPage.xaml` - Existing settings

**MainWindow.xaml:**
- Header: NAURA logo with dragon icon, version v2.4.0 CO 7009
- Nav: Home, Marketplace, Auction, Friends, Settings (Friends is new)
- Right: Search (market), Wallet: CPs (emerald) + Gold (gold), User area clickable with dropdown
- User dropdown: Profile, Inventory, Friends & Voice, Logout
- Page host: Auth overlay when not authenticated, else pages
- Status bar: Twin City Connected, Patch 7009, Voice Server Active, Guild Voice Ready, Twin City Synced

### 5. Server Layer (`NauraLauncher.Server/`)
Standalone C# server for backend.

**Program.cs:**
- Initializes database (in-memory demo or MySQL 5.6)
- Starts VoiceSignalingServer on 8081
- Starts ApiServer on 8080
- Health endpoint, Conquer Online info

**ApiServer.cs:**
- HttpListener on 0.0.0.0:8080
- REST endpoints: auth, marketplace, auction, friends, inventory, wallet
- CORS enabled for launcher
- Mock data returns Conquer Online themed JSON

**VoiceSignalingServer.cs:**
- HttpListener with WebSocket upgrade on 0.0.0.0:8081
- ConcurrentDictionary for user sockets, channels, calls
- Auth via first message { type: "auth", userId }
- Message forwarding: offer/answer/ice-candidate to target user or broadcast to channel
- Call lifecycle: initiate -> ringing -> accepted/declined -> active -> ended

## MySQL 5.6 Compatibility

**Schema File:** `Database/schema.sql`

**Compatibility Fixes Applied:**
1. No JSON type - using TEXT for `participant_ids`, `payload`, etc.
2. Using TIMESTAMP with DEFAULT CURRENT_TIMESTAMP (works from MySQL 5.6.5+)
   - For MySQL 5.6.0-5.6.4, TIMESTAMP still works, DATETIME DEFAULT would fail
   - We use TIMESTAMP for all created_at, etc.
3. No GENERATED columns
4. ENGINE=InnoDB for all tables
5. utf8 charset (not utf8mb4) for max compatibility, but utf8mb4 works on 5.6.5+
6. No CHECK constraints (MySQL 5.6 parses but ignores) - validation in application
7. Foreign keys with ON DELETE CASCADE
8. Using INT UNSIGNED, BIGINT for CPs/Gold (can exceed 2B)
9. MySqlConnector library 2.3.7 supports MySQL 5.6
10. Password hashing: PBKDF2 (Rfc2898DeriveBytes) not bcrypt/Argon2 which may need newer libs

**Tables:**
- users, wallets, inventory_items, marketplace_listings, marketplace_transactions, auction_lots, auction_bids, friendships, friend_requests, blocked_users, voice_channels, voice_calls

**Indexes:**
- On frequently queried columns: user_id, status, rarity, type, etc.

## Conquer Online Theming

**Currencies:**
- CPs (Conquer Points): Premium, from point cards, events, trading gems/DBs. Used for Shopping Mall (rare items, garments, mounts, lottery 27 CPs/entry)
- Gold: Trade currency, player vending
- Silver: Basic currency
- Bound CPs: Non-tradable from events

**Classes:**
- Trojan (dual-wield blades), Warrior (heavy armor, axe), Archer (bow, AoE), Fire/Water Taoist (magic), Ninja (katana, poison), Monk (prayer beads, chi), Pirate (pistol/rapier, half PK penalty), Dragon Warrior (nunchaku)

**Items:**
- Weapons: Blade, Sword, Backsword, Bow, etc. with +1 to +12 enhancement, 0-2 sockets
- Gems: Dragon Gem, Phoenix Gem, Moon Gem, etc. Super, Refined, etc.
- Consumables: Dragon Ball (rebirth, upgrades, CPs exchange), Meteor (upgrade), Exp Ball
- Garments, Mounts, etc.

**Locations:**
- Twin City Market (178,182) - CP Admin location for claiming CPs
- Phoenix Castle, Desert City, etc.

**Systems:**
- Rebirth: At Lv 120, reset to Lv 15 as different class, keep some skills, get unique weapon
- PK: Red/Black names drop equipment, can pay CPs to avoid losing gear
- Guild War, Lottery, etc.

## Clean Code Principles Applied

1. **Single Responsibility:** Each service has one job (Auth, Inventory, Marketplace, etc.)
2. **Dependency Inversion:** ViewModels depend on interfaces, not concrete services
3. **Separation of Concerns:** Domain has no dependencies, Application depends only on Domain
4. **DRY:** MockDataService centralizes mock generation
5. **Naming:** Conquer Online terms (CPs, Gold, Dragon Ball, Twin City) used consistently
6. **Error Handling:** Services throw InvalidOperationException with clear messages, ViewModels catch and show in StatusMessage
7. **Async/Await:** All services async for future MySQL implementation
8. **No Magic Numbers:** Enums for class, rarity, type, currency
9. **Immutable where possible:** Value objects, but entities mutable for EF-like pattern
10. **Comments:** XML docs explaining Conquer Online context

## Future Improvements for Production

- Replace in-memory services with MySQL repositories using MySqlConnector
- Add JWT authentication with refresh tokens
- Add real WebRTC with STUN/TURN servers for voice (currently signaling only)
- Add caching (Redis) for marketplace/auction listings
- Add rate limiting for API
- Add logging (Serilog)
- Add unit tests for services
- Add integration tests for API
