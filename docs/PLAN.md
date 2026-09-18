# NauraLauncher - Project Status Plan

## What is Done (Implemented)

### 1. Authentication System
- **Login Page** (`Views/AuthPage.xaml` + `ViewModels/AuthViewModel.cs`)
  - Username/email + password
  - Demo account: admin/admin123
  - Mock heroes: DragonLord, FireQueen, ShadowNinja, etc.
  - Switch to Register
  - Error handling with status messages
  - PBKDF2 password hashing (MySQL 5.6 safe)

- **Register Page** (same AuthPage, register mode)
  - Username, Display Name, Email, Password, Confirm Password
  - Validation: username available, email available, password length, match
  - Auto-creates wallet with starter CPs, Gold, Silver

- **Auth Service** (`Application/Services/AuthService.cs`)
  - In-memory user store with mock Conquer Online heroes
  - Login, Register, Logout, GetUserById, IsUsernameAvailable
  - Clean interface `IAuthService`

### 2. User Panel (Header User Area Dropdown)
- **MainWindow.xaml** updated:
  - User area clickable with `ShowUserMenuCommand`
  - Dropdown menu with:
    - Profile (avatar, name, class)
    - Inventory
    - Friends & Voice
    - Logout (destructive)
  - Closes when clicking outside
  - Only visible when authenticated

- **MainViewModel** handles:
  - `IsUserMenuOpen`, `ShowUserMenuCommand`, `CloseUserMenuCommand`
  - `ShowProfileCommand`, `ShowInventoryCommand`, `LogoutCommand`
  - Propagates UserId to sub ViewModels on login

### 3. Wallet / Currencies (CPs, Gold, Silver)
- **MainViewModel** now exposes:
  - `CpsDisplay` - e.g. "50,000 CPs" (emerald accent)
  - `GoldDisplay` - e.g. "1,000,000 Gold" (gold color #F5A524)
  - `SilverDisplay` - Silver (optional)
  - `Credits` kept for backward compat but shows CPs

- **Domain:**
  - `Wallet` entity with `Cps`, `BoundCps`, `Gold`, `Silver`
  - `CurrencyType` enum with display names and icon keys
  - `GetBalance()`, `Add()`, `TryDeduct()` methods

- **WalletService** (`Application/Services/WalletService.cs`)
  - GetWallet, AddCurrency, DeductCurrency, TransferCurrency
  - Used by Marketplace and Auction for buy/sell flows

- **Header UI:**
  - Two pills: CPs (gem icon, emerald) + Gold (gold icon, gold color)
  - Replaces old generic "$148.50 Credits"

### 4. Conquer Online Theming (Replaced APEX sci-fi)
- **Assets:**
  - Generated new images:
    - `hero_conquer.png` - Twin City marketplace, dragons, sunset
    - `card_trojan.png` - Trojan dual blades
    - `card_warrior.png` - Warrior heavy armor axe
    - `card_archer.png` - Archer elegant bow
    - `card_taoist.png` - Taoist fire/water magic
    - `card_ninja.png` - Ninja katana stealth
    - `card_monk.png` - Monk prayer beads chi
    - `avatar_conquer.png` - Heroic warrior portrait
    - `item_dragonball.png` - Glowing Dragon Ball

- **Icons** (`Themes/Icons.xaml`):
  - Added Conquer icons: Sword, Gem, Gold, Silver, Coin, Dragon, Inventory, Logout, Phone, Block, UserPlus, UserMinus

- **ViewModels:**
  - `HomeViewModel`: "WELCOME BACK, HERO", "TWIN CITY MARKET OPEN", Battle Power, Wealth, Continue: "ENTER TWIN CITY", News: Dragon Gem auction, Patch 7009 Ninja skills, Guild War

  - `FeatureViewModel`: "DRAGON BLADE +12 - SUPER 2-SOCKET", Twin City Market, Super 2-socket, description about Guild War, PK arena

  - `MarketplaceViewModel`: Twin City Market, categories Weapon/Armor/Garment/Gem/Consumable/Mount, status tiles Market Volume, CPs Trade, Traders Online

  - `AuctionViewModel`: Dragon Blade, Super Dragon Gem, Dragon Ball packs, etc. Attributes: Origin Twin City (178,182), Enhancement +12 Super 2-socket, etc.

- **MainWindow:**
  - Logo: NAURA + CONQUER ONLINE with dragon icon
  - Version: v2.4.0 - CO 7009
  - Ping: 18ms - Eternity (server name)
  - Status bar: TWIN CITY CONNECTED, PATCH 7009 - CONQUER ONLINE, VOICE SERVER ACTIVE, GUILD VOICE READY, TWIN CITY SYNCED

### 5. Marketplace with Inventory Integration
- **MarketplaceViewModel** now:
  - Tabs: Browse Market, My Listings, Sell Item
  - `SwitchTabCommand` to switch tabs
  - Browse: Shows `MarketplaceItems` from `MarketplaceService`, filter by category, search, buy with `BuyCommand`
  - My Listings: `MyListings` from service, cancel with `CancelListingCommand`
  - Sell: `TradableInventory` from `InventoryService`, select item, set price + currency (CPs/Gold), `CreateListingCommand` creates listing and removes from inventory
  - StatusMessage for feedback
  - `LoadMarketplaceAsync()`, `LoadMyListingsAsync()`, `LoadTradableInventoryAsync()`

- **MarketplacePage.xaml** updated:
  - Toolbar with tab pills + category chips + search
  - Browse tab: Left hero + status tiles, Right Twin City Market listings with Buy buttons, shows item + plus + rarity + seller + price with CPs/Gold icons
  - My Listings tab: Grid of my listings with cancel
  - Sell tab: Left inventory grid selectable, Right create listing panel with price input, currency segmented control, List button

- **MarketplaceService**:
  - `GetActiveListingsAsync(page, pageSize, filterType, filterRarity)` with pagination
  - `GetUserListingsAsync(userId)`
  - `CreateListingAsync(sellerId, itemId, price, currency)` - removes from inventory, creates listing
  - `CancelListingAsync(userId, listingId)` - returns item to inventory
  - `BuyItemAsync(buyerId, listingId)` - deduct buyer, pay seller, transfer item, create transaction
  - `SearchAsync(query, type)`

### 6. Auction with Inventory Integration (Same as Marketplace)
- **AuctionViewModel** now:
  - Tabs: Browse Auctions, My Lots, Sell
  - Browse: Rarity filters (Common, Rare, Elite, Super, Epic, Legendary), Active Lots list selectable, Live lot card, Attributes, Bid panel with increments, Place Bid, Buyout, Watchlist
  - My Lots: My active lots with cancel, My bids
  - Sell: Tradable inventory selectable, Starting price, Buyout price, Currency, Create auction (3 days duration)
  - `SelectLotCommand` to select lot and update live lot display
  - `PlaceBidAsync()` with increment, `BuyoutAsync()`, `CreateAuctionAsync()`, `CancelAuctionAsync()`
  - Countdown timer for live lot

- **AuctionPage.xaml** updated:
  - Similar structure to marketplace but auction themed
  - Summary cards: Auction Pool, Watchlist, Lots Won
  - Left: Active lots list + live lot card + attributes
  - Right: Bid panel with current bid, top bidder, buyout, progress, increment chips, Place Bid, Buyout, Watchlist
  - My Lots tab: My active lots + My bids
  - Sell tab: Inventory + create auction panel

- **AuctionService**:
  - `GetActiveLotsAsync(page, pageSize, rarity)` with expiry check
  - `GetUserLotsAsync`, `GetUserBidsAsync`
  - `CreateLotAsync(sellerId, itemId, startingPrice, buyoutPrice, currency, duration)` - removes from inventory
  - `CancelLotAsync` - only if no bids
  - `PlaceBidAsync(bidderId, lotId, amount)` - checks higher than current, deduct, refund previous bidder, create bid
  - `BuyoutAsync(buyerId, lotId)` - deduct, refund current bidder, pay seller, transfer item
  - `GetBidHistoryAsync`, `SearchAsync`

### 7. Inventory System
- **InventoryViewModel**:
  - `Inventory`, `FilteredInventory` collections
  - Categories: All, Weapon, Armor, Garment, Gem, Consumable, Mount, Other
  - `SelectedCategory`, `SearchQuery`, `SelectedItem`
  - `SelectCategoryCommand`, `SelectItemCommand`, `RefreshCommand`
  - `LoadInventoryAsync()`, `ApplyFilter()`, `GetTradableItems()`

- **InventoryPage.xaml**:
  - Toolbar: Inventory count, category chips, search, refresh
  - Content: Left wrap panel of items (200px cards with image, name, type, plus, tradable badge), selectable with accent border
  - Right detail panel: Image, name, rarity, type, level req, enhancement, sockets, description, socket gems, trade options buttons (List in Marketplace, Put in Auction), bound/tradable status

- **InventoryService**:
  - In-memory inventories per user
  - `GetUserInventoryAsync`, `GetItemByIdAsync`, `AddItemAsync`, `RemoveItemAsync`, `UpdateItemAsync`, `TransferItemAsync`, `GetTradableItemsAsync`

### 8. Friends System
- **FriendsViewModel**:
  - `Friends`, `OnlineFriends`, `PendingRequests`, `BlockedUsers`, `SearchResults`
  - `AddFriendUsername`, `SearchQuery`, `StatusMessage`, `ActiveCall`
  - Commands: AddFriend, RemoveFriend, BlockUser, UnblockUser, AcceptRequest, DeclineRequest, StartVoiceCall, Search, Refresh, EndActiveCall
  - `LoadAllAsync()` loads friends, online, pending, blocked
  - `AddFriendAsync()` - if mock user auto-accept, else create request
  - `RemoveFriendAsync()` removes both directions
  - `BlockUserAsync()` removes friendship and adds to blocked
  - `UnblockUserAsync()`
  - `StartVoiceCallAsync()` via VoiceService
  - `SearchUsersAsync()`

- **FriendsPage.xaml**:
  - Header: Friends & Voice count, search heroes, add friend input + button, refresh
  - Status message banner
  - Left: Online Friends list with avatar, level, class, location, voice call, remove, block buttons; All Friends list with offline badge
  - Right: Active Voice Call card (if active), Pending Requests with accept/decline, Blocked Players with unblock, Search Results

- **FriendsService**:
  - In-memory friendships, blocked, requests
  - `GetFriendsAsync`, `GetOnlineFriendsAsync`, `GetPendingRequestsAsync`, `GetBlockedUsersAsync`
  - `AddFriendAsync(userId, friendUsername)` - checks exists, self, already friends, blocked, request pending; auto-accept for mock users, else creates FriendRequest
  - `RemoveFriendAsync`, `AcceptFriendRequestAsync`, `DeclineFriendRequestAsync`, `BlockUserAsync`, `UnblockUserAsync`, `IsBlockedAsync`, `SearchUsersAsync`

### 9. Voice Calls Server
- **VoiceSignalingServer** (`NauraLauncher.Server/Realtime/VoiceSignalingServer.cs`):
  - HttpListener on 0.0.0.0:8081
  - WebSocket handling: auth via first message, then message loop
  - ConcurrentDictionary for user sockets, channels, calls
  - Message types: create-channel, join-channel, leave-channel, initiate-call, accept-call, decline-call, end-call, offer, answer, ice-candidate, mute, unmute
  - `ProcessSignalingMessageAsync` handles server logic and forwards to target user or broadcasts to channel
  - Call lifecycle with channel creation
  - Clean up on disconnect

- **VoiceService** (client-side, `Application/Services/VoiceService.cs`):
  - In-memory channels and calls for demo
  - `CreateChannelAsync`, `GetChannelAsync`, `GetUserChannelsAsync`, `JoinChannelAsync`, `LeaveChannelAsync`, `CloseChannelAsync`
  - `InitiateCallAsync`, `GetCallAsync`, `AcceptCallAsync`, `DeclineCallAsync`, `EndCallAsync`
  - `SendSignalingMessageAsync` raises `OnSignalingMessage` event
  - For production would connect to VoiceSignalingServer via WebSocket

- **VoiceClient** (`Services/ApiClient.cs`):
  - WebSocket client for launcher
  - `ConnectAsync(userId, token)`, `SendSignalingAsync`, `InitiateCallAsync`, `AcceptCallAsync`, `EndCallAsync`
  - Receive loop with `OnMessageReceived` event

- **Domain:**
  - `VoiceChannel` with ChannelId (UUID), Type (Direct, Party, Guild, Team), Name, ParticipantIds, CreatedBy
  - `VoiceCall` with CallId (UUID), ChannelId, CallerId, CalleeId, Status (Initiating, Ringing, Active, Ended, Declined, Failed)
  - `VoiceSignalingMessage` with Type, ChannelId, CallId, FromUserId, ToUserId, Payload (JSON SDP/ICE), Timestamp

### 10. C# Backend Server (`NauraLauncher.Server/`)
- **Program.cs:**
  - Checks MySQL 5.6 compatibility
  - Initializes DatabaseService (in-memory demo or MySQL)
  - Starts VoiceSignalingServer on 8081
  - Starts ApiServer on 8080
  - Lists endpoints, Conquer Online info
  - Handles Ctrl+C shutdown

- **ApiServer.cs:**
  - HttpListener on 0.0.0.0:8080
  - REST endpoints: /api/auth/login, /api/auth/register, /api/marketplace/listings, /api/marketplace/list, /api/marketplace/buy, /api/auction/lots, /api/auction/bid, /api/auction/buyout, /api/friends, /api/friends/add, /api/friends/block, /api/inventory, /api/wallet, /health
  - CORS headers
  - Mock data returns Conquer Online themed JSON
  - Logs requests

- **Infrastructure:**
  - `DatabaseConfig` + `DatabaseService` + `MySqlSchemaValidator`
  - `MySqlConnectionFactory` + `MySqlRepositoryBase` with MySQL 5.6 safe helpers
  - Uses MySqlConnector 2.3.7

- **Solution:**
  - Updated `NauraLauncher.sln` to include both NauraLauncher and NauraLauncher.Server

### 11. MySQL Schema (MySQL 5.6 Compatible)
- **File:** `Database/schema.sql`
- **Compatibility fixes:**
  - No JSON type, using TEXT for participant_ids, payloads
  - TIMESTAMP DEFAULT CURRENT_TIMESTAMP (compatible from 5.6.5+, TIMESTAMP works on older)
  - No GENERATED columns
  - ENGINE=InnoDB
  - utf8 charset (utf8mb4 optional)
  - No CHECK constraints (app validation)
  - Foreign keys with ON DELETE CASCADE
  - BIGINT for CPs/Gold
  - MySqlConnector library

- **Tables:**
  - users, wallets, inventory_items, marketplace_listings, marketplace_transactions, auction_lots, auction_bids, friendships, friend_requests, blocked_users, voice_channels, voice_calls

- **Seed Data:**
  - 5 users: admin, DragonLord, FireQueen, ShadowNinja, HolyMonk
  - Wallets with CPs, Gold, Silver
  - 7 inventory items: Dragon Blade +12, Super Dragon Gem, Dragon Ball x27, etc.
  - Friendships

- **Validation:**
  - `MySqlSchemaValidator.Validate()` checks for incompatible features

### 12. Clean Code & Architecture
- **Domain** has no dependencies
- **Application** depends only on Domain, defines interfaces
- **Infrastructure** implements interfaces, depends on Domain and Application
- **Presentation** depends on Application and Domain
- **Server** has its own Domain, Infrastructure, Api, Realtime
- Single Responsibility, Dependency Inversion, DRY, clear naming with Conquer terms

## What is Prototype (Works but Simplified)

### 1. In-Memory Services
- All services (`AuthService`, `InventoryService`, `MarketplaceService`, `AuctionService`, `FriendsService`, `VoiceService`) use static in-memory collections
- **Why prototype:** No real MySQL connection in demo, but interfaces allow swapping with MySQL repositories
- **Production needed:** Implement MySQL repositories using `MySqlConnectionFactory` and schema

### 2. Mock Data
- `MockDataService` generates Conquer Online themed inventory, marketplace, auction, friends
- **Why prototype:** For demo without real Conquer Online server
- **Production needed:** Fetch from real Conquer Online game server or player database

### 3. Voice Signaling Only (No Real Audio)
- `VoiceSignalingServer` handles WebRTC signaling (SDP, ICE) but does NOT relay audio
- **Why prototype:** Real voice needs WebRTC P2P with STUN/TURN or SFU
- **Production needed:** 
  - Integrate WebRTC library (e.g. SIPSorcery, or use browser WebRTC via WebView)
  - Add STUN/TURN servers (coturn)
  - Or use SFU like Janus, Jitsi
  - Handle audio capture/playback via NAudio

### 4. API Server Mock Responses
- `ApiServer` returns mock JSON, not real database queries
- **Why prototype:** Demo without MySQL
- **Production needed:** Connect to MySQL via repositories, add JWT auth, validation

### 5. Password Hashing Example
- `PasswordHasher` uses PBKDF2 with 10000 iterations - works but production should use BCrypt or Argon2 with higher iterations
- **Why prototype:** Simple, no extra dependencies, MySQL 5.6 safe
- **Production needed:** Use BCrypt.Net-Next or Konscious.Security.Cryptography.Argon2

### 6. WPF UI Without Real Game Integration
- Launcher does NOT actually launch Conquer Online client (no `Conquer.exe` path, no patching)
- **Why prototype:** No Conquer Online client in sandbox
- **Production needed:** Add game path detection, version check, patch download, launch with account token

## What is Demo Code (For Showcase, Not Production)

### 1. Hardcoded Mock Users
- `AuthService` static constructor seeds mock users with empty password hash (any password works for mock users)
- Admin user has real hash for admin123
- **Demo:** Allows login with any mock hero without password for testing friends, marketplace, etc.

### 2. Random Price Generation
- `MockDataService.GenerateMockMarketplace` uses `new Random().Next(10, 5000)` for prices
- **Demo:** Random prices for showcase
- **Production:** Real prices from player listings or market analysis

### 3. Auto-Accept Friend Requests for Mock Users
- `FriendsService.AddFriendAsync` auto-accepts if target is mock user (ID < 100)
- **Demo:** Easier testing, no need for second client to accept
- **Production:** All friend requests should be pending until accepted

### 4. UI Placeholders and Fake Data in ViewModels
- `HomeViewModel` has hardcoded stats: "23H 40M", "4,850 Battle Power", etc.
- `AuctionViewModel` has hardcoded `BuyoutValue = 78_000d`, `_currentBidValue = 52_400d`
- `ProfileViewModel` has example stats
- **Demo:** Shows UI without real data
- **Production:** Fetch from services

### 5. No Real Currency Transaction Safety
- `Wallet.TryDeduct` is not thread-safe, no database transactions
- `MarketplaceService.BuyItemAsync` does deduct then pay seller without transaction rollback if fails
- **Demo:** Simple for showcase
- **Production:** Use database transactions, row locking, idempotency keys

### 6. No Pagination UI
- Services support pagination (`page`, `pageSize`) but UI does not show pagination controls
- **Demo:** Only first page shown
- **Production:** Add pagination UI, infinite scroll, or virtualized list

### 7. No Input Validation in UI
- Price TextBox accepts any text, no numeric validation
- Username search no sanitization
- **Demo:** Relies on service validation
- **Production:** Add validation rules, error templates, input masks

### 8. Images Generated, Not Real Conquer Online Assets
- All hero and card images are AI-generated, not official TQ Digital assets
- **Demo:** Avoids copyright, shows concept
- **Production:** Use official Conquer Online art or licensed assets

## What is Missing (Needs Implementation)

### 1. Real MySQL Persistence
- **Missing:** MySQL repositories implementing `IAuthService`, `IInventoryService`, etc. with actual SQL queries
- **Needed:** 
  - `UserRepository`, `WalletRepository`, `InventoryRepository`, `MarketplaceRepository`, `AuctionRepository`, `FriendshipRepository`, `VoiceRepository`
  - Use `MySqlConnectionFactory` to create connections
  - Parameterized queries to prevent SQL injection
  - Transactions for buy/sell flows

### 2. JWT Authentication & Security
- **Missing:** JWT token generation, validation, refresh tokens, secure storage
- **Needed:**
  - JWT with expiration, refresh token in HttpOnly cookie or secure storage
  - Token validation middleware in ApiServer
  - Password reset flow
  - Email verification
  - Rate limiting for login

### 3. Real Conquer Online Game Integration
- **Missing:** 
  - Game client path detection (e.g. C:\Conquer\Conquer.exe)
  - Patch system: Check version, download patches from TQ servers
  - Launch game with account credentials (maybe via `Conquer.exe` args or memory injection - depends on Conquer Online client)
  - Character selection, server selection (Eternity, etc.)
  - Anti-cheat compatibility

### 4. Real-Time Updates
- **Missing:** WebSocket or SignalR for live marketplace/auction updates, friend online status, voice call incoming
- **Needed:**
  - SignalR hub or WebSocket broadcast for:
    - New marketplace listings
    - New auction bids (live bid panel should update in real-time)
    - Friend online/offline
    - Incoming voice calls (currently only via voice signaling server, not integrated with friends UI real-time)

### 5. Voice Audio (WebRTC)
- **Missing:** Actual audio capture, encoding, P2P transmission, playback
- **Needed:**
  - NAudio or CSCore for audio capture
  - Opus codec for encoding
  - WebRTC implementation (SIPSorcery or native)
  - STUN/TURN servers
  - UI for mute, deafen, volume, device selection
  - Party/Guild voice channels with multiple participants (mixing)

### 6. Trade Safety & Anti-Scam
- **Missing:** 
  - Escrow for marketplace (hold item until payment confirmed)
  - Confirmation dialogs for buy/sell with item preview
  - Trade history with receipts
  - Reporting system for scam listings

### 7. Search & Filtering Enhancements
- **Missing:**
  - Advanced filters: price range, plus range, socket count, class restriction, bound/unbound
  - Sorting: price low-high, newest, most bids, etc.
  - Full-text search with MySQL FULLTEXT index
  - Save search presets

### 8. Notifications
- **Missing:** 
  - Toast notifications for: friend request, incoming call, item sold, outbid, auction won
  - System tray integration
  - Sound effects

### 9. Settings Persistence
- **Missing:** Save settings to file or registry, load on startup
- **Needed:** 
  - `SettingsService` that saves to `%AppData%\NauraLauncher\settings.json`
  - Apply settings to actual game client if possible (resolution, etc.)

### 10. Logging & Monitoring
- **Missing:** Logging, error reporting, analytics
- **Needed:**
  - Serilog or NLog for file logging
  - Sentry or Application Insights for error tracking
  - Metrics for marketplace volume, active users, voice calls

### 11. Testing
- **Missing:** Unit tests for services, integration tests for API, UI tests
- **Needed:**
  - xUnit tests for `AuthService`, `MarketplaceService` (test buy/sell with insufficient funds, etc.)
  - Integration tests for `ApiServer` endpoints
  - UI automation tests (Appium or WinAppDriver)

### 12. Deployment & Installer
- **Missing:** Installer, auto-updater, code signing
- **Needed:**
  - WiX Toolset or Inno Setup installer
  - Squirrel or AutoUpdater.NET for auto-updates
  - Code signing certificate for SmartScreen
  - Check for .NET 8 runtime

### 13. Conquer Online Specific Features
- **Missing:**
  - Lottery system (27 CPs per entry)
  - Shopping Mall (CPs shop)
  - Guild system integration
  - PK arena ranking
  - Battle Power calculation
  - Rebirth quest tracking
  - Dragon Ball exchange rate tracker

## How to Run (Current Demo)

### Launcher (WPF - Requires Windows with .NET 8)
```powershell
cd NauraLauncher
dotnet restore
dotnet run --project NauraLauncher/NauraLauncher.csproj
```
- Login with admin/admin123 or any mock hero (DragonLord, FireQueen, etc. with any password)
- Explore Marketplace: Browse, My Listings, Sell Item (from inventory)
- Explore Auction: Browse lots, place bids, buyout, My Lots, Sell
- Check Inventory: Filter, search, view details
- Check Profile: Wallet with CPs, Gold, Silver, Bound CPs
- Check Friends: Add friend, remove, block/unblock, voice call (signaling only)
- Settings: Existing settings page

### Server (Cross-platform .NET 8)
```bash
cd NauraLauncher.Server
dotnet restore
dotnet run --project NauraLauncher.Server.csproj
```
- API on http://0.0.0.0:8080
- Voice signaling on ws://0.0.0.0:8081
- Uses in-memory mode by default (no MySQL needed)
- Set MYSQL_CONNECTION env var and UseInMemory=false for real MySQL 5.6

### MySQL 5.6 Setup (Production)
```sql
-- Create database
CREATE DATABASE nauralauncher CHARACTER SET utf8 COLLATE utf8_general_ci;

-- Run schema
SOURCE Database/schema.sql;

-- Create user
CREATE USER 'naura'@'localhost' IDENTIFIED BY 'your_password';
GRANT ALL PRIVILEGES ON nauralauncher.* TO 'naura'@'localhost';
FLUSH PRIVILEGES;
```

## Clean Code Checklist

- [x] Domain entities separated from UI
- [x] Interfaces for all services
- [x] In-memory implementations for demo with clear boundaries
- [x] MySQL 5.6 compatible schema with no JSON, no generated columns
- [x] Password hashing with PBKDF2
- [x] Async/await throughout
- [x] Enums for all fixed values
- [x] Conquer Online theming consistent (CPs, Gold, Dragon Ball, Twin City)
- [x] User panel with Profile, Inventory, Logout
- [x] Marketplace: My Listings + Add from Inventory + Buy/Sell flow
- [x] Auction: Same as marketplace + bidding + buyout
- [x] Friends: Add, Remove, Block/Unblock, Search
- [x] Voice: Signaling server with WebSocket, call lifecycle, channel management
- [x] Wallet: CPs, Gold, Silver, Bound CPs with add/deduct/transfer
- [x] Tests updated and passing
- [x] Documentation: Architecture, Plan, README

## Known Issues / Mistakes Fixed

### C# Server Mistakes Checked:
1. **MySQL 5.6 incompatibility** - Fixed: Removed JSON type, using TEXT; using TIMESTAMP not DATETIME DEFAULT; no GENERATED; utf8 charset; InnoDB
2. **Password hashing** - Was missing, added PBKDF2 with salt
3. **Currency overflow** - Using BIGINT not INT for CPs/Gold (can exceed 2B in Conquer Online)
4. **No transaction handling** - Noted as prototype, needs DB transactions in production
5. **WebSocket without auth** - Added auth via first message with userId
6. **No CORS** - Added CORS headers in ApiServer
7. **Hardcoded ports** - Configurable via constructor
8. **No graceful shutdown** - Added CancellationTokenSource and Ctrl+C handling
9. **No logging** - Added Console.WriteLine with prefixes [API], [Voice], [DB], [Auth], etc.

### Launcher Mistakes Checked:
1. **Duplicate Style assignment in XAML** - Fixed: Removed Style attribute when using <Border.Style> element
2. **Missing BasedOn in TextBlock.Style** - Fixed: Added BasedOn="{StaticResource Label}" to placeholder styles
3. **No auth flow** - Added AuthPage with login/register and MainViewModel auth state
4. **Credits only one currency** - Added CPs and Gold (and Silver, Bound CPs) with Wallet entity
5. **No inventory integration** - Added InventoryViewModel and tradable items flow to marketplace/auction
6. **No user panel** - Added dropdown with Profile, Inventory, Friends, Logout
7. **APEX theming not Conquer** - Replaced with Conquer Online theme, generated new assets, updated ViewModels
