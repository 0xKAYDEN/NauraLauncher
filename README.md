# NauraLauncher — Conquer Online Launcher

Conquer Online themed launcher — WPF / .NET 8, clean architecture, with marketplace, auction, inventory, friends, voice calls, and backend server.

> Transformed from APEX sci-fi shell to Conquer Online martial arts fantasy. Now supports Twin City Market, Dragon Balls, CPs/Gold currencies, Trojan/Ninja/Taoist classes, and voice chat between heroes.

## Features Implemented

### 🔐 Authentication
- **Login Page** - Username/email + password, demo: admin/admin123 or mock heroes (DragonLord, FireQueen, etc.)
- **Register Page** - Username, Display Name, Email, Password, auto-creates wallet with starter CPs/Gold
- **User Panel** - Click user area in header → dropdown with Profile, Inventory, Friends & Voice, Logout

### 💰 Wallet - 4 Currencies (Conquer Online)
- **CPs (Conquer Points)** - Premium currency for Shopping Mall, lottery (27 CPs/entry), rare items, garments, mounts
- **Gold** - Trade currency for Twin City Market and player vending
- **Silver** - Basic currency
- **Bound CPs** - Non-tradable from events
- Displayed in header as two pills: CPs (emerald) + Gold (gold), plus full wallet in Profile page

### 🎒 Inventory
- Conquer Online items: Dragon Blade +12 Super 2-socket, Super Dragon Gem, Dragon Ball x27, Heaven Fan +9, Meteor Scroll, Ninja Katana, etc.
- Filter by type (Weapon, Armor, Garment, Gem, Consumable, Mount), search, select detail
- Shows Plus (+12), Sockets (2), Socket Gems, Tradable/Bound status
- Trade options: List in Marketplace, Put in Auction

### 🏪 Marketplace - Twin City Market (178,182)
- **Browse Market** - Hero spotlight (Dragon Blade +12), status tiles (Market Volume, CPs Trade, Traders Online), listings with Buy buttons, filter by category, search
- **My Listings** - Your active listings with cancel (returns item to inventory)
- **Sell Item** - Select from tradable inventory, set price + currency (CPs/Gold), create listing
- **Buy Flow** - Deduct buyer wallet, pay seller, transfer item, create transaction

### 🔨 Auction House
- **Browse Auctions** - Rarity filters (Common, Rare, Elite, Super, Epic, Legendary), active lots list selectable, live lot card with countdown, attributes (Origin Twin City, Enhancement, Sockets), bid panel with increments (+100/+250/+500/+1K), Place Bid, Buyout, Watchlist
- **My Lots** - My active lots (cancel if no bids) + My bids
- **Sell** - Select from inventory, set starting price, buyout price, currency, create auction (3 days)

### 👥 Friends & Voice
- **Friend List** - All friends, Online friends with Twin City location, level, class
- **Add Friend** - Search by username, add (auto-accept for mock users, else pending request)
- **Remove Friend** - Bidirectional removal
- **Block/Unblock** - Block prevents trade, voice, friend requests; shows in Blocked Players with unblock
- **Friend Requests** - Pending with accept/decline
- **Voice Calls** - Voice call button on online friends, initiates call via VoiceService, active call card with mute/end, WebSocket signaling server for WebRTC (offer/answer/ice-candidate)

### 🖥️ Backend Server (`NauraLauncher.Server/`)
- **API Server** - HttpListener on 0.0.0.0:8080, REST endpoints: auth, marketplace, auction, friends, inventory, wallet, health
- **Voice Signaling Server** - HttpListener with WebSocket on 0.0.0.0:8081, handles call initiation, SDP exchange, ICE candidates, channel management
- **MySQL 5.6 Compatible Schema** - `Database/schema.sql` with no JSON type (using TEXT), TIMESTAMP DEFAULT CURRENT_TIMESTAMP, InnoDB, utf8, BIGINT for currencies, foreign keys
- **Clean Architecture** - Domain, Application, Infrastructure, Api, Realtime layers
- **In-Memory Demo** - No MySQL required for demo, but ready for MySQL 5.6 with MySqlConnector

## Structure

```
NauraLauncher/
├── NauraLauncher/
│   ├── Domain/
│   │   ├── Entities/          # User, Wallet, InventoryItem, MarketplaceListing, AuctionLot, Friendship, VoiceCall, etc.
│   │   └── Enums/             # CurrencyType, ConquerClass, ItemType, ItemRarity
│   ├── Application/
│   │   ├── Interfaces/        # IAuthService, IInventoryService, IMarketplaceService, IAuctionService, IFriendsService, IVoiceService
│   │   └── Services/          # AuthService, InventoryService, WalletService, MarketplaceService, AuctionService, FriendsService, VoiceService, MockDataService
│   ├── Infrastructure/
│   │   ├── Security/          # PasswordHasher (PBKDF2)
│   │   ├── Persistence/       # MySqlConnectionFactory, MySqlRepositoryBase
│   │   └── Voice/             # Voice signaling (client side)
│   ├── Services/              # ApiClient (HTTP), VoiceClient (WebSocket)
│   ├── ViewModels/            # MainViewModel, AuthViewModel, HomeViewModel, MarketplaceViewModel, AuctionViewModel, InventoryViewModel, ProfileViewModel, FriendsViewModel, SettingsViewModel
│   ├── Views/                 # AuthPage, HomePage, MarketplacePage, AuctionPage, InventoryPage, ProfilePage, FriendsPage, SettingsPage
│   ├── Themes/                # Colors, Typography, Icons (now with Conquer icons: Sword, Gem, Gold, Dragon, etc.), Controls, Converters
│   ├── Assets/                # hero_conquer.png, card_trojan.png, card_warrior.png, card_archer.png, card_taoist.png, card_ninja.png, card_monk.png, avatar_conquer.png, item_dragonball.png
│   └── MainWindow.xaml        # Shell with NAURA logo, nav (Home, Marketplace, Auction, Friends, Settings), wallet (CPs, Gold), user dropdown
├── NauraLauncher.Server/
│   ├── Domain/                # ServerModels (CurrencyType, ConquerClass, etc.)
│   ├── Infrastructure/        # DatabaseConfig, DatabaseService, MySqlSchemaValidator, MySqlConnectionFactory
│   ├── Api/                   # ApiServer (REST)
│   ├── Realtime/              # VoiceSignalingServer (WebSocket)
│   └── Program.cs             # Entry point, starts API + Voice servers
├── Database/
│   └── schema.sql             # MySQL 5.6 compatible schema with Conquer Online tables
├── docs/
│   ├── ARCHITECTURE.md        # Clean architecture explanation
│   └── PLAN.md                # What is done, prototype, demo, missing
└── tests/
    └── test_xaml.py           # XAML validation + Conquer assets + schema checks
```

## Conquer Online Lore

- **Game:** 2D martial arts MMORPG by TQ Digital, 20+ years, PvP focused, PK with item drop, rebirth system
- **Classes:** Trojan (dual blades, high damage), Warrior (tank, heavy armor), Archer (bow, AoE, fastest leveler), Fire Taoist (offensive magic), Water Taoist (healing), Ninja (katana, high damage low defense), Monk (prayer beads, chi), Pirate (pistol/rapier, half PK penalty, double drop chance), Dragon Warrior (nunchaku, combos)
- **Currencies:** CPs (Conquer Points, from point cards, trading gems/DBs, events, used for Shopping Mall, lottery), Gold (trade), Silver, Bound CPs
- **Items:** Dragon Ball (rebirth, upgrades, CPs exchange, 27 DBs = 1 lottery entry worth 1075 CPs), Meteor (upgrade), Gems (Dragon, Phoenix, Moon, etc.), Weapons +1 to +12 with 2 sockets
- **Market:** Twin City Market (178,182) where CP Admin (178,182) claims CPs, players vend items for CPs/Gold
- **Rebirth:** At Lv 120, reset to Lv 15 as different class, keep some skills, get unique weapon, summon guard/monster
- **PK:** Red/Black names drop equipment, can pay CPs to winner to avoid losing gear, Pirate class best for PK

## Build & Run

### Launcher (WPF - Windows only)
```powershell
cd NauraLauncher
dotnet restore
dotnet run --project NauraLauncher/NauraLauncher.csproj
```
- Requires .NET 8 SDK on Windows
- Login: admin/admin123 or DragonLord/FireQueen/etc. with any password (mock users)

### Server (Cross-platform)
```bash
cd NauraLauncher.Server
dotnet restore
dotnet run
```
- API: http://0.0.0.0:8080
- Voice: ws://0.0.0.0:8081
- In-memory demo mode (no MySQL needed)
- For MySQL 5.6: set MYSQL_CONNECTION env var, set UseInMemory=false in Program.cs

### MySQL 5.6 Setup
```sql
CREATE DATABASE nauralauncher CHARACTER SET utf8 COLLATE utf8_general_ci;
SOURCE Database/schema.sql;
CREATE USER 'naura'@'localhost' IDENTIFIED BY 'password';
GRANT ALL PRIVILEGES ON nauralauncher.* TO 'naura'@'localhost';
FLUSH PRIVILEGES;
```

### Tests (Linux sandbox)
```bash
python3 -m unittest discover -s tests -v
```

## What is Prototype vs Demo vs Missing

See `docs/PLAN.md` for detailed breakdown:
- **Done:** Auth, User Panel, Wallet (CPs, Gold), Inventory, Marketplace with My Listings + Sell from Inventory + Buy, Auction with My Lots + Sell + Bid/Buyout, Friends (add/remove/block), Voice signaling server
- **Prototype:** In-memory services (not MySQL), mock data, voice signaling only (no audio), API mock responses, PBKDF2 hashing
- **Demo:** Hardcoded mock users, random prices, auto-accept friends, fake stats, no thread safety, no pagination UI, AI-generated images
- **Missing:** Real MySQL repos, JWT, game client launch, real-time updates, WebRTC audio, escrow, advanced filters, notifications, logging, tests, installer

## Clean Code

- Domain has no dependencies, Application depends only on Domain
- Interfaces for all services, ViewModels depend on interfaces
- Single Responsibility, Dependency Inversion, DRY
- Enums for all fixed values, clear Conquer Online naming
- Async/await, error handling with StatusMessage, no magic numbers
- MySQL 5.6 compatibility validated, PBKDF2 hashing, BIGINT for currencies

## License

For educational purposes. Conquer Online is property of TQ Digital. Images are AI-generated placeholders.
