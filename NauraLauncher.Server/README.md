# NauraLauncher.Server — Conquer Online Backend

Backend server for NauraLauncher Conquer Online edition.

## What it Does

- **API Server** (port 8080) - REST for auth, marketplace, auction, friends, inventory, wallet
- **Voice Signaling Server** (port 8081) - WebSocket for WebRTC signaling (offer/answer/ice-candidate), call lifecycle
- **MySQL 5.6 Compatible** - Schema in `../Database/schema.sql` with no JSON, TIMESTAMP, InnoDB, utf8
- **Clean Architecture** - Domain, Infrastructure, Api, Realtime

## Running

```bash
dotnet restore
dotnet run
```

- API: http://0.0.0.0:8080
- Voice: ws://0.0.0.0:8081
- In-memory demo mode (no MySQL needed)

For MySQL 5.6 production:

```bash
MYSQL_CONNECTION="Server=localhost;Port=3306;Database=nauralauncher;Uid=root;Pwd=yourpwd;CharSet=utf8;" dotnet run
```

And set `UseInMemory=false` in Program.cs.

## Endpoints

- POST /api/auth/login - Login
- POST /api/auth/register - Register
- GET /api/marketplace/listings - Browse Twin City Market
- POST /api/marketplace/list - List item from inventory
- POST /api/marketplace/buy - Buy item (CPs/Gold flow: deduct buyer, pay seller, transfer item)
- GET /api/auction/lots - Browse auction
- POST /api/auction/bid - Place bid (refund previous bidder)
- POST /api/auction/buyout - Instant buyout
- GET /api/friends - Friend list
- POST /api/friends/add - Add friend (auto-accept mock for demo)
- POST /api/friends/block - Block/unblock
- GET /api/inventory - Player inventory
- GET /api/wallet - Wallet (CPs, Gold, Silver, Bound CPs)
- GET /health - Health check with Conquer Online info
- WS /ws/voice (port 8081) - Voice signaling

## Voice Protocol

JSON over WebSocket:

```json
{
  "type": "offer",
  "channelId": "uuid",
  "callId": "uuid",
  "fromUserId": 100,
  "toUserId": 200,
  "payload": "{ \"sdp\": \"...\" }"
}
```

Types:
- auth, auth-ok
- create-channel, channel-created, join-channel, leave-channel, user-joined, user-left
- initiate-call, call-incoming, call-ringing, call-accepted, call-declined, call-ended, end-call
- offer, answer, ice-candidate, mute, unmute

## MySQL 5.6 Compatibility

Schema fixes:
- No JSON type → TEXT for participant_ids, payloads
- TIMESTAMP DEFAULT CURRENT_TIMESTAMP (not DATETIME, requires 5.6.5+ but TIMESTAMP works on older)
- No GENERATED columns
- ENGINE=InnoDB
- utf8 charset (utf8mb4 optional on 5.6.5+)
- No CHECK constraints (app validation)
- BIGINT for CPs/Gold (Conquer Online can have 2B+)
- MySqlConnector 2.3.7 supports MySQL 5.6

Tables: users, wallets, inventory_items, marketplace_listings, marketplace_transactions, auction_lots, auction_bids, friendships, friend_requests, blocked_users, voice_channels, voice_calls

## Clean Architecture

- Domain/ServerModels.cs - Pure entities, no dependencies
- Infrastructure/DatabaseConfig.cs - DatabaseService, MySqlSchemaValidator
- Infrastructure/MySqlConnectionFactory.cs - MySqlConnector factory, MySQL 5.6 safe helpers
- Api/ApiServer.cs - HttpListener REST
- Realtime/VoiceSignalingServer.cs - HttpListener WebSocket signaling

## Future

- Replace in-memory with MySQL repositories
- Add JWT auth
- Add real WebRTC audio with NAudio + SIPSorcery + STUN/TURN
- Add SignalR for live marketplace/auction updates
- Add logging (Serilog), rate limiting, caching (Redis)
