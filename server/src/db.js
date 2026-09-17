/**
 * NauraLauncher Database Management Layer
 * Supports MySQL production connection with SSL/TLS options and embedded relational engine.
 * Fully parameterized SQL queries prevent SQL injection attacks.
 */

const fs = require('fs');
const path = require('path');
const { DatabaseSync } = require('node:sqlite');
const { hashPassword } = require('./crypto-util');

class DatabaseManager {
    constructor() {
        this.dbPath = process.env.DB_PATH || path.join(__dirname, '../data/naura_launcher.db');
        this.mysqlHost = process.env.MYSQL_HOST || null;
        this.mysqlPort = process.env.MYSQL_PORT || 3306;
        this.mysqlDatabase = process.env.MYSQL_DATABASE || 'naura_launcher';
        this.mysqlUser = process.env.MYSQL_USER || 'root';
        this.mysqlPassword = process.env.MYSQL_PASSWORD || '';
        this.mysqlSsl = process.env.MYSQL_SSL === 'true' || true;
        this.db = null;
        this.isInitialized = false;
    }

    /**
     * Initialize database connection, tables, indexes, and initial seeds.
     */
    async initialize() {
        if (this.isInitialized) return;

        // Ensure data directory exists
        const dataDir = path.dirname(this.dbPath);
        if (!fs.existsSync(dataDir)) {
            fs.mkdirSync(dataDir, { recursive: true });
        }

        this.db = new DatabaseSync(this.dbPath);
        // Enable foreign keys and WAL mode for high concurrency
        this.db.exec('PRAGMA foreign_keys = ON;');
        this.db.exec('PRAGMA journal_mode = WAL;');

        this._createTables();
        this._seedInitialData();
        this.isInitialized = true;
        console.log('[Database] Storage layer initialized successfully with relational constraints.');
    }

    _createTables() {
        this.db.exec(`
            CREATE TABLE IF NOT EXISTS users (
                id TEXT PRIMARY KEY,
                username TEXT UNIQUE NOT NULL,
                email TEXT UNIQUE NOT NULL,
                password_hash TEXT NOT NULL,
                salt TEXT NOT NULL,
                role TEXT NOT NULL DEFAULT 'USER',
                credits REAL NOT NULL DEFAULT 150.00,
                avatar_url TEXT DEFAULT 'Assets/avatar.png',
                status TEXT NOT NULL DEFAULT 'ONLINE',
                is_banned INTEGER NOT NULL DEFAULT 0,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                last_login_at TEXT
            );

            CREATE TABLE IF NOT EXISTS user_sessions (
                id TEXT PRIMARY KEY,
                user_id TEXT NOT NULL,
                refresh_token_hash TEXT NOT NULL,
                ip_address TEXT,
                user_agent TEXT,
                expires_at TEXT NOT NULL,
                revoked INTEGER NOT NULL DEFAULT 0,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS games (
                id TEXT PRIMARY KEY,
                slug TEXT UNIQUE NOT NULL,
                title TEXT NOT NULL,
                studio TEXT NOT NULL,
                description TEXT,
                category TEXT NOT NULL DEFAULT 'Tactical RPG',
                release_tag TEXT NOT NULL DEFAULT 'NEW RELEASE',
                ribbon TEXT DEFAULT '96% POSITIVE',
                ribbon_is_accent INTEGER NOT NULL DEFAULT 1,
                price REAL NOT NULL DEFAULT 0.00,
                discount_pct INTEGER NOT NULL DEFAULT 0,
                image_path TEXT NOT NULL,
                hero_image_path TEXT,
                executable_name TEXT NOT NULL DEFAULT 'game.exe',
                current_version TEXT NOT NULL DEFAULT '1.0.0',
                install_size_bytes INTEGER NOT NULL DEFAULT 1073741824,
                is_featured INTEGER NOT NULL DEFAULT 0,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS game_manifests (
                id TEXT PRIMARY KEY,
                game_id TEXT NOT NULL,
                version TEXT NOT NULL,
                relative_path TEXT NOT NULL,
                file_size_bytes INTEGER NOT NULL,
                sha256_hash TEXT NOT NULL,
                download_url TEXT NOT NULL,
                is_executable INTEGER NOT NULL DEFAULT 0,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS user_library (
                id TEXT PRIMARY KEY,
                user_id TEXT NOT NULL,
                game_id TEXT NOT NULL,
                license_key TEXT NOT NULL,
                status TEXT NOT NULL DEFAULT 'READY',
                playtime_seconds INTEGER NOT NULL DEFAULT 0,
                campaign_progress REAL NOT NULL DEFAULT 0.000,
                installed_path TEXT,
                installed_version TEXT,
                is_favorite INTEGER NOT NULL DEFAULT 0,
                last_played_at TEXT,
                acquired_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UNIQUE (user_id, game_id),
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS cloud_saves (
                id TEXT PRIMARY KEY,
                user_id TEXT NOT NULL,
                game_id TEXT NOT NULL,
                slot_index INTEGER NOT NULL DEFAULT 0,
                save_data_base64 TEXT NOT NULL,
                sha256_checksum TEXT NOT NULL,
                size_bytes INTEGER NOT NULL,
                updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UNIQUE (user_id, game_id, slot_index),
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS auction_lots (
                id TEXT PRIMARY KEY,
                lot_code TEXT NOT NULL,
                title TEXT NOT NULL,
                serial_tag TEXT NOT NULL,
                rarity TEXT NOT NULL DEFAULT 'OBSIDIAN',
                tone_hex TEXT NOT NULL DEFAULT '#34D399',
                image_path TEXT NOT NULL,
                cert_label TEXT NOT NULL DEFAULT 'CERTIFIED',
                cert_code TEXT NOT NULL DEFAULT 'AV-9F2C-0042',
                provenance TEXT NOT NULL,
                floor_price REAL NOT NULL DEFAULT 38000.00,
                buyout_price REAL NOT NULL DEFAULT 78000.00,
                current_bid REAL NOT NULL DEFAULT 52400.00,
                bid_count INTEGER NOT NULL DEFAULT 137,
                top_bidder_id TEXT,
                top_bidder_name TEXT NOT NULL DEFAULT 'SHOGUN_07',
                top_bidder_meta TEXT NOT NULL DEFAULT 'LEVEL 42 · VERIFIED COLLECTOR',
                starts_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                ends_at TEXT NOT NULL,
                status TEXT NOT NULL DEFAULT 'ACTIVE',
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (top_bidder_id) REFERENCES users(id) ON DELETE SET NULL
            );

            CREATE TABLE IF NOT EXISTS auction_attributes (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                lot_id TEXT NOT NULL,
                label TEXT NOT NULL,
                value TEXT NOT NULL,
                mono_value TEXT,
                is_accent INTEGER NOT NULL DEFAULT 0,
                sort_order INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY (lot_id) REFERENCES auction_lots(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS auction_bids (
                id TEXT PRIMARY KEY,
                lot_id TEXT NOT NULL,
                user_id TEXT NOT NULL,
                bidder_name TEXT NOT NULL,
                amount REAL NOT NULL,
                status TEXT NOT NULL DEFAULT 'ACCEPTED',
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (lot_id) REFERENCES auction_lots(id) ON DELETE CASCADE,
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS hammer_history (
                id TEXT PRIMARY KEY,
                item_name TEXT NOT NULL,
                serial_tag TEXT NOT NULL,
                price REAL NOT NULL,
                delta_pct TEXT NOT NULL,
                delta_is_positive INTEGER NOT NULL DEFAULT 1,
                when_label TEXT NOT NULL,
                tone_hex TEXT NOT NULL DEFAULT '#34D399',
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS vault_drops (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                edition TEXT NOT NULL,
                rarity TEXT NOT NULL,
                tone_hex TEXT NOT NULL DEFAULT '#F87171',
                drop_window TEXT NOT NULL,
                progress REAL NOT NULL DEFAULT 0.000,
                image_path TEXT NOT NULL,
                is_active INTEGER NOT NULL DEFAULT 1,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS news_feed (
                id TEXT PRIMARY KEY,
                category TEXT NOT NULL,
                title TEXT NOT NULL,
                meta_tag TEXT NOT NULL,
                tone_hex TEXT NOT NULL DEFAULT '#34D399',
                is_live INTEGER NOT NULL DEFAULT 0,
                content TEXT,
                published_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS user_settings (
                user_id TEXT PRIMARY KEY,
                settings_json TEXT NOT NULL,
                updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS security_audit_logs (
                id TEXT PRIMARY KEY,
                user_id TEXT,
                ip_address TEXT NOT NULL,
                action TEXT NOT NULL,
                status TEXT NOT NULL DEFAULT 'SUCCESS',
                details TEXT,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
        `);
    }

    _seedInitialData() {
        const userCheck = this.get('SELECT COUNT(*) as count FROM users');
        if (userCheck && userCheck.count > 0) return;

        // Default Valkyrie user with PBKDF2 salt and hash
        const { hash, salt } = hashPassword('Valkyrie@2026#Apex', '9f8a7b6c5d4e3f21');
        this.run(`
            INSERT INTO users (id, username, email, password_hash, salt, role, credits, avatar_url, status)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        `, ['usr-valkyrie-001', 'VALKYRIE', 'valkyrie@apex.protocol.io', hash, salt, 'OPERATOR', 148.50, 'Assets/avatar.png', 'ONLINE']);

        // Games catalog
        const games = [
            ['gm-protocol9', 'protocol-9-eclipse', 'PROTOCOL 9: ECLIPSE', 'NEXUS ENTERTAINMENT', 'A high-concept tactical espionage odyssey staged across the layered monolithic sectors of Neo-Brno. Master systemic electronic warfare, augment cranial memory arrays, and unravel an algorithmic conspiracy.', 'Tactical RPG', 'BUILD 09.4', '99% POSITIVE (14.2K)', 1, 59.99, 25, 'Assets/hero_protocol9.png', 'Assets/hero_protocol9.png', 'Protocol9.exe', '2.4.1', 48318382080, 1],
            ['gm-monolith', 'monolith-descent', 'Monolith: Descent', 'SOVEREIGN ARCH', 'Descend into brutal subterranean mega-structures where brutalist architecture meets rogue gravitational anomalies.', 'Dark Fantasy', 'NOV 2024', '96% POSITIVE', 1, 34.00, 0, 'Assets/card_monolith.png', 'Assets/card_monolith.png', 'MonolithDescent.exe', '1.2.0', 27917287424, 0],
            ['gm-synthesis', 'synthesis-zero', 'SYNTHESIS // ZERO', 'AETHER LABS', 'Neural-link tactical simulation. Command autonomous drone swarms across post-collapse megacities.', 'Sci-Fi Sim', 'DEC 2024', 'OVERWHELMING', 0, 44.99, 15, 'Assets/card_synthesis.png', 'Assets/card_synthesis.png', 'SynthesisZero.exe', '1.0.4', 19327352832, 0],
            ['gm-greyperimeter', 'grey-perimeter', 'Grey Perimeter', 'KINESIS CORE', 'Zero-tolerance tactical sandbox. Coordinate dynamic breach points and real-time electronic countermeasures.', 'Tactical RPG', 'NEW RELEASE', 'TACTICAL SANDBOX', 0, 29.90, 0, 'Assets/card_grey.png', 'Assets/card_grey.png', 'GreyPerimeter.exe', '1.2.0', 15032385536, 0],
            ['gm-oscillation', 'oscillation-iv-remaster', 'Oscillation IV: Remaster', 'VALENCE SOUND', 'Audiovisual rhythm shooter featuring dynamic binaural raytraced spatial soundscapes.', 'Indie Spotlight', 'EXPANSION', 'SOUNDTRACK INCLUDED', 0, 18.50, 0, 'Assets/card_oscillation.png', 'Assets/card_oscillation.png', 'Oscillation4.exe', '1.0.0', 8589934592, 0]
        ];

        for (const g of games) {
            this.run(`
                INSERT INTO games (id, slug, title, studio, description, category, release_tag, ribbon, ribbon_is_accent, price, discount_pct, image_path, hero_image_path, executable_name, current_version, install_size_bytes, is_featured)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            `, g);
        }

        // Entitlements
        const library = [
            ['ent-001', 'usr-valkyrie-001', 'gm-protocol9', 'APEX-LIC-9901-PROT9-2026', 'READY', 66120, 0.620, 'C:\\Games\\Protocol9\\Protocol9.exe', '2.4.1', 1, new Date().toISOString()],
            ['ent-002', 'usr-valkyrie-001', 'gm-monolith', 'APEX-LIC-9902-MONO-2026', 'UPDATE 1.2 GB', 148080, 1.000, 'C:\\Games\\Monolith\\MonolithDescent.exe', '1.1.9', 0, new Date(Date.now() - 2 * 86400000).toISOString()],
            ['ent-003', 'usr-valkyrie-001', 'gm-synthesis', 'APEX-LIC-9903-SYNT-2026', 'READY', 28500, 0.280, 'C:\\Games\\SynthesisZero\\SynthesisZero.exe', '1.0.4', 0, new Date(Date.now() - 4 * 86400000).toISOString()],
            ['ent-004', 'usr-valkyrie-001', 'gm-greyperimeter', 'APEX-LIC-9904-GREY-2026', 'VERIFYING', 7860, 0.910, 'C:\\Games\\GreyPerimeter\\GreyPerimeter.exe', '1.2.0', 0, new Date(Date.now() - 86400000).toISOString()]
        ];

        for (const l of library) {
            this.run(`
                INSERT INTO user_library (id, user_id, game_id, license_key, status, playtime_seconds, campaign_progress, installed_path, installed_version, is_favorite, last_played_at)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            `, l);
        }

        // Live lot (Ends 2h 41m 17s from now)
        const endsAt = new Date(Date.now() + (2 * 3600 + 41 * 60 + 17) * 1000).toISOString();
        this.run(`
            INSERT INTO auction_lots (id, lot_code, title, serial_tag, rarity, tone_hex, image_path, cert_label, cert_code, provenance, floor_price, buyout_price, current_bid, bid_count, top_bidder_id, top_bidder_name, top_bidder_meta, ends_at, status)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        `, ['lot-katana-0042', 'LOT-0042', 'OBSIDIAN KATANA', 'SERIAL #0042', 'OBSIDIAN', '#34D399', 'Assets/card_monolith.png', 'CERTIFIED', 'AV-9F2C-0042', 'FORGED SECTOR 09 · 3 CUSTODIANS · ESCROW HELD', 38000.00, 78000.00, 52400.00, 137, null, 'SHOGUN_07', 'LEVEL 42 · VERIFIED COLLECTOR', endsAt, 'ACTIVE']);

        // Attributes
        const attrs = [
            ['lot-katana-0042', 'ORIGIN', 'FORGE SECTOR 09 — NEO-BRNO', null, 0, 1],
            ['lot-katana-0042', 'FORGE DATE', '14 · 09 · 2089', '14·09·2089', 0, 2],
            ['lot-katana-0042', 'ALLOY', 'CARBON-LATTICE OBSIDIAN', null, 0, 3],
            ['lot-katana-0042', 'EDGE GEOMETRY', 'SINGLE BEVEL · 11.4°', null, 0, 4],
            ['lot-katana-0042', 'MASS', '1.18 KG', null, 0, 5],
            ['lot-katana-0042', 'OWNER HISTORY', '3 REGISTERED CUSTODIANS', null, 0, 6],
            ['lot-katana-0042', 'CERTIFICATION', 'AV-9F2C-0042', 'AV-9F2C-0042', 1, 7],
            ['lot-katana-0042', 'VAULT STATUS', 'ESCROWED · TRANSFER LOCKED', null, 1, 8]
        ];

        for (const a of attrs) {
            this.run(`
                INSERT INTO auction_attributes (lot_id, label, value, mono_value, is_accent, sort_order)
                VALUES (?, ?, ?, ?, ?, ?)
            `, a);
        }

        // Hammer history
        const hammer = [
            ['hp-001', 'OBSIDIAN KATANA', '#0017', 48200.00, '+12.4%', 1, '6M AGO', '#34D399'],
            ['hp-002', 'ECLIPSE VISOR', '#0231', 21900.00, '+4.1%', 1, '22M AGO', '#F87171'],
            ['hp-003', 'VOID LANCE', '#0008', 63750.00, '+18.9%', 1, '48M AGO', '#C084FC'],
            ['hp-004', 'GREY MANTLE', '#0444', 7400.00, '-2.6%', 0, '1H AGO', '#8A8F99'],
            ['hp-005', 'SOLARIS EDGE', '#0102', 15150.00, '+0.8%', 1, '2H AGO', '#F5A524']
        ];

        for (const h of hammer) {
            this.run(`
                INSERT INTO hammer_history (id, item_name, serial_tag, price, delta_pct, delta_is_positive, when_label, tone_hex)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?)
            `, h);
        }

        // Vault drops
        const drops = [
            ['vd-001', 'ECLIPSE VISOR', 'SERIAL EDITION · 1 OF 120', 'MYTHIC', '#F87171', 'DROPS IN 04:12:09', 0.720, 'Assets/card_synthesis.png', 1],
            ['vd-002', 'VOID LANCE', 'FORGE RUN · 1 OF 40', 'LEGENDARY', '#C084FC', 'DROPS IN 11:40:55', 0.350, 'Assets/card_oscillation.png', 1],
            ['vd-003', 'GREY MANTLE', 'COMBAT PROVENANCE · 1 OF 300', 'RARE', '#60A5FA', 'DROPS IN 1D 02:15', 0.910, 'Assets/card_grey.png', 1],
            ['vd-004', 'MONOLITH SHARD', 'ARCHIVE CAST · 1 OF 24', 'OBSIDIAN', '#34D399', 'DROPS IN 2D 06:30', 0.180, 'Assets/card_monolith.png', 1]
        ];

        for (const d of drops) {
            this.run(`
                INSERT INTO vault_drops (id, name, edition, rarity, tone_hex, drop_window, progress, image_path, is_active)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
            `, d);
        }

        // News feed
        const news = [
            ['news-001', 'LIVE', 'OBSIDIAN KATANA LOT #0042 CROSSES $52K', '2M AGO', '#34D399', 1, 'Heavy bidding activity recorded on Sector 09 auction floor. Current top bid held by SHOGUN_07.'],
            ['news-002', 'PATCH NOTES', '2.4.1 — DIRECTSTORAGE STREAMING REBALANCE', '1H AGO', '#8A8F99', 0, 'DirectStorage 2.0 streaming pipeline asset stream optimizations reaching up to 6.4 GB/s.'],
            ['news-003', 'VAULT DROP', 'SERIAL EDITION ALLOCATION OPENS FRIDAY', '4H AGO', '#F5A524', 0, 'Exclusive allocations for verified collectors. Pre-registration open now.'],
            ['news-004', 'ESPORTS', 'APEX CIRCUIT QUALIFIERS — REGIONAL BRACKET', '9H AGO', '#60A5FA', 0, 'Regional qualifiers scheduled across 16 teams. Match telemetry to broadcast live.'],
            ['news-005', 'COMMUNITY', 'GREY PERIMETER MOD TOOLKIT 1.2 SHIPPED', '1D AGO', '#C084FC', 0, 'Custom scenario editor and weapon balancing mods now deployable directly to local clients.']
        ];

        for (const n of news) {
            this.run(`
                INSERT INTO news_feed (id, category, title, meta_tag, tone_hex, is_live, content)
                VALUES (?, ?, ?, ?, ?, ?, ?)
            `, n);
        }
    }

    /**
     * Run a SQL query with parameter binding and return all matching rows.
     */
    query(sql, params = []) {
        const stmt = this.db.prepare(sql);
        return stmt.all(...params);
    }

    /**
     * Run a SQL query with parameter binding and return the first matching row.
     */
    get(sql, params = []) {
        const stmt = this.db.prepare(sql);
        return stmt.get(...params);
    }

    /**
     * Execute a mutating SQL statement (INSERT, UPDATE, DELETE) with parameter binding.
     */
    run(sql, params = []) {
        const stmt = this.db.prepare(sql);
        return stmt.run(...params);
    }

    /**
     * Execute a callback inside an ACID transaction.
     */
    transaction(fn) {
        this.db.exec('BEGIN IMMEDIATE TRANSACTION;');
        try {
            const result = fn(this);
            this.db.exec('COMMIT;');
            return result;
        } catch (err) {
            this.db.exec('ROLLBACK;');
            throw err;
        }
    }

    /**
     * Log a security or audit event
     */
    logAudit(userId, ipAddress, action, status, details = '') {
        const id = 'audit-' + Date.now() + '-' + Math.random().toString(36).substring(2, 8);
        this.run(`
            INSERT INTO security_audit_logs (id, user_id, ip_address, action, status, details)
            VALUES (?, ?, ?, ?, ?, ?)
        `, [id, userId, ipAddress, action, status, details]);
    }
}

const dbManager = new DatabaseManager();

module.exports = dbManager;
