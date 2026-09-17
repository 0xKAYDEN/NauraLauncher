# NauraLauncher — Production Grade APEX Game Launcher

Enterprise, production-grade game launcher client and secure backend infrastructure.
Engineered with Clean Architecture (Domain, Core, Infrastructure, MVVM Presentation) with high security, native TLS 1.3/1.2 encryption, MySQL database persistence with connection pooling and prepared statements, and RFC 6455 WebSockets for sub-millisecond real-time communication.

---

## 🌟 Key Features & Production Capabilities

### 1. Clean Architecture Client (.NET 8 WPF)
- **Domain & Core (`Core/Entities`, `Core/Interfaces`)**: Pure entities (`UserProfile`, `GameItem`, `GameManifest`, `AuctionLotData`, `LauncherConfig`, `DownloadProgressInfo`) and decoupled service interfaces.
- **Infrastructure Layer (`Infrastructure/`)**:
  - `Security/CryptoService.cs`: Hardware/Windows DPAPI (`ProtectedData.Protect`) with AES-256-GCM fallback for secret storage; streaming SHA-256 anti-tamper file integrity verification.
  - `Network/SecureApiClient.cs`: High-performance HTTP client enforcing TLS 1.2/1.3, connection pooling, automatic Bearer JWT header injection, and exponential backoff retry.
  - `Network/RealtimeWebSocketService.cs`: Thread-safe RFC 6455 WebSocket client with TLS (`wss://`), heartbeat ping/pong latency measurement, and topic pub/sub routing.
  - `Services/AuthService.cs`: Production session manager with secure token persistence and profile synchronization.
  - `Services/GameProcessLauncher.cs`: Process supervisor that checks SHA-256 integrity, obtains signed launch tokens, spawns game processes, hooks exit codes, and syncs playtime to MySQL backend.
  - `Services/DownloadPatcherService.cs`: Chunked resumable downloader utilizing HTTP Range headers, staging deployment (`.staging/`), SHA-256 validation, and transfer speed smoothing.
  - `Services/AuctionService.cs`: Real-time auction synchronization connecting the live UI to WebSocket floor updates and atomic backend transactions.
  - `Services/SettingsService.cs`: Two-way persistent configuration saving to `%LOCALAPPDATA%/NauraLauncher/settings.json` and cloud sync.
  - `Database/MySqlService.cs`: Direct MySQL connectivity testing, prepared statements, and SSL/TLS encrypted connection strings.
  - `DI/ServiceContainer.cs`: Dependency injection composition root.

### 2. Hardened Backend & Real-time Server (`server/`)
- **Native TLS 1.3 / 1.2**: HTTPS and Secure WebSockets (`wss://`) with custom cipher suites and X.509 certificate management.
- **Relational MySQL Database Persistence**:
  - Full production DDL schema (`server/db/schema.sql`) and seed data (`server/db/seed.sql`).
  - 12 normalized tables: `users`, `user_sessions`, `games`, `game_manifests`, `user_library`, `cloud_saves`, `auction_lots`, `auction_attributes`, `auction_bids`, `hammer_history`, `vault_drops`, `news_feed`, `user_settings`, and `security_audit_logs`.
  - 100% Parameterized queries with prepared statements (zero SQL injection vulnerabilities).
  - ACID transactions for critical actions (bidding, balance deductions, game claiming).
- **Security & Anti-Tamper**:
  - PBKDF2-SHA256 password hashing with 100,000 iterations and per-user cryptographic salts.
  - Constant-time hash verification (`crypto.timingSafeEqual`) to defeat timing attacks.
  - JWT authentication (HMAC-SHA256) with refresh token rotation and revocation.
  - HMAC-signed game launch tokens preventing executable tampering and unauthorized launching.
  - Resumable file streaming with HTTP Range headers (`206 Partial Content`) and `X-Checksum-SHA256`.

---

## 🏗️ Architecture Layout

```
NauraLauncher/
├── Core/
│   ├── Entities/               # Domain models (UserProfile, GameItem, Manifest, Config)
│   └── Interfaces/             # Core service contracts (IAuth, IApiClient, IWebSocket, etc.)
├── Infrastructure/
│   ├── Security/               # DPAPI encryption, SHA-256 integrity verifier
│   ├── Network/                # SecureApiClient (TLS), RealtimeWebSocketService (WSS)
│   ├── Services/               # Launcher engine, Patcher, AuthService, AuctionService, Settings
│   ├── Database/               # MySQL client & connection pooling
│   └── DI/                     # ServiceContainer composition root
├── ViewModels/                 # MVVM Presentation layer wired to production services
├── Views/                      # WPF XAML views (HomePage, MarketplacePage, AuctionPage, SettingsPage)
├── Themes/                     # Design tokens, shadcn dark zinc controls, typography, icons
└── Common/                     # ObservableObject, RelayCommand, Converters

server/
├── certs/                      # X.509 SSL/TLS Certificate and Private Key
├── db/                         # Production MySQL DDL schema and initial seed data
├── src/                        # Server engine (HTTP/HTTPS, WebSocket, Crypto, Router, Database)
└── tests/                      # Automated backend integration tests (10/10 test suite)
```

---

## 🚀 Running the Backend Server

```bash
# Start backend server (TLS HTTPS on 8443, HTTP on 8080, WSS on /ws)
node server/src/server.js
```

### Running Backend Integration Tests
```bash
node server/tests/server.test.js
```

---

## 💻 Building the Client (.NET 8 WPF)

```bash
dotnet restore NauraLauncher/NauraLauncher.csproj
dotnet build   NauraLauncher/NauraLauncher.csproj
dotnet run     --project NauraLauncher/NauraLauncher.csproj
```

---

## 🧪 Automated Test Suite

Run the full Python and architecture test suite:
```bash
python3 -m unittest discover -s tests
```
