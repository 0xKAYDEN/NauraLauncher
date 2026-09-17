-- =============================================================================
-- NauraLauncher Production Initial Seed Data
-- =============================================================================

-- Seed User (Password: "Valkyrie@2026#Apex", salt: "9f8a7b6c5d4e3f21")
-- PBKDF2-SHA256 hex hash
INSERT INTO `users` (`id`, `username`, `email`, `password_hash`, `salt`, `role`, `credits`, `avatar_url`, `status`)
VALUES (
    'usr-valkyrie-001',
    'VALKYRIE',
    'valkyrie@apex.protocol.io',
    'a2cf761a2be06cbddbc1544a8647ba951c24e6ffdd472d8e48fbfa6369c762148d56214fa35b5a7949bb2701e6ef61d6bc0f51950e395e282c092c4e9be03816',
    '9f8a7b6c5d4e3f21',
    'OPERATOR',
    148.50,
    'Assets/avatar.png',
    'ONLINE'
) ON DUPLICATE KEY UPDATE `username` = VALUES(`username`);

-- Seed Games Catalog
INSERT INTO `games` (`id`, `slug`, `title`, `studio`, `description`, `category`, `release_tag`, `ribbon`, `ribbon_is_accent`, `price`, `discount_pct`, `image_path`, `hero_image_path`, `executable_name`, `current_version`, `install_size_bytes`, `is_featured`)
VALUES
(
    'gm-protocol9',
    'protocol-9-eclipse',
    'PROTOCOL 9: ECLIPSE',
    'NEXUS ENTERTAINMENT',
    'A high-concept tactical espionage odyssey staged across the layered monolithic sectors of Neo-Brno. Master systemic electronic warfare, augment cranial memory arrays, and unravel an algorithmic conspiracy.',
    'Tactical RPG',
    'BUILD 09.4',
    '99% POSITIVE (14.2K)',
    1,
    59.99,
    25,
    'Assets/hero_protocol9.png',
    'Assets/hero_protocol9.png',
    'Protocol9.exe',
    '2.4.1',
    48318382080,
    1
),
(
    'gm-monolith',
    'monolith-descent',
    'Monolith: Descent',
    'SOVEREIGN ARCH',
    'Descend into brutal subterranean mega-structures where brutalist architecture meets rogue gravitational anomalies.',
    'Dark Fantasy',
    'NOV 2024',
    '96% POSITIVE',
    1,
    34.00,
    0,
    'Assets/card_monolith.png',
    'Assets/card_monolith.png',
    'MonolithDescent.exe',
    '1.2.0',
    27917287424,
    0
),
(
    'gm-synthesis',
    'synthesis-zero',
    'SYNTHESIS // ZERO',
    'AETHER LABS',
    'Neural-link tactical simulation. Command autonomous drone swarms across post-collapse megacities.',
    'Sci-Fi Sim',
    'DEC 2024',
    'OVERWHELMING',
    0,
    44.99,
    15,
    'Assets/card_synthesis.png',
    'Assets/card_synthesis.png',
    'SynthesisZero.exe',
    '1.0.4',
    19327352832,
    0
),
(
    'gm-greyperimeter',
    'grey-perimeter',
    'Grey Perimeter',
    'KINESIS CORE',
    'Zero-tolerance tactical sandbox. Coordinate dynamic breach points and real-time electronic countermeasures.',
    'Tactical RPG',
    'NEW RELEASE',
    'TACTICAL SANDBOX',
    0,
    29.90,
    0,
    'Assets/card_grey.png',
    'Assets/card_grey.png',
    'GreyPerimeter.exe',
    '1.2.0',
    15032385536,
    0
),
(
    'gm-oscillation',
    'oscillation-iv-remaster',
    'Oscillation IV: Remaster',
    'VALENCE SOUND',
    'Audiovisual rhythm shooter featuring dynamic binaural raytraced spatial soundscapes.',
    'Indie Spotlight',
    'EXPANSION',
    'SOUNDTRACK INCLUDED',
    0,
    18.50,
    0,
    'Assets/card_oscillation.png',
    'Assets/card_oscillation.png',
    'Oscillation4.exe',
    '1.0.0',
    8589934592,
    0
) ON DUPLICATE KEY UPDATE `title` = VALUES(`title`);

-- Seed User Library Entitlements for default user
INSERT INTO `user_library` (`id`, `user_id`, `game_id`, `license_key`, `status`, `playtime_seconds`, `campaign_progress`, `installed_path`, `installed_version`, `is_favorite`, `last_played_at`)
VALUES
(
    'ent-001',
    'usr-valkyrie-001',
    'gm-protocol9',
    'APEX-LIC-9901-PROT9-2026',
    'READY',
    66120, -- 18H 22M
    0.620,
    'C:\\Games\\Protocol9\\Protocol9.exe',
    '2.4.1',
    1,
    NOW()
),
(
    'ent-002',
    'usr-valkyrie-001',
    'gm-monolith',
    'APEX-LIC-9902-MONO-2026',
    'UPDATE 1.2 GB',
    148080, -- 41H 08M
    1.000,
    'C:\\Games\\Monolith\\MonolithDescent.exe',
    '1.1.9',
    0,
    DATE_SUB(NOW(), INTERVAL 2 DAY)
),
(
    'ent-003',
    'usr-valkyrie-001',
    'gm-synthesis',
    'APEX-LIC-9903-SYNT-2026',
    'READY',
    28500, -- 07H 55M
    0.280,
    'C:\\Games\\SynthesisZero\\SynthesisZero.exe',
    '1.0.4',
    0,
    DATE_SUB(NOW(), INTERVAL 4 DAY)
),
(
    'ent-004',
    'usr-valkyrie-001',
    'gm-greyperimeter',
    'APEX-LIC-9904-GREY-2026',
    'VERIFYING',
    7860, -- 02H 11M
    0.910,
    'C:\\Games\\GreyPerimeter\\GreyPerimeter.exe',
    '1.2.0',
    0,
    DATE_SUB(NOW(), INTERVAL 1 DAY)
) ON DUPLICATE KEY UPDATE `status` = VALUES(`status`);

-- Seed Live Auction Lot
INSERT INTO `auction_lots` (`id`, `lot_code`, `title`, `serial_tag`, `rarity`, `tone_hex`, `image_path`, `cert_label`, `cert_code`, `provenance`, `floor_price`, `buyout_price`, `current_bid`, `bid_count`, `top_bidder_id`, `top_bidder_name`, `top_bidder_meta`, `starts_at`, `ends_at`, `status`)
VALUES (
    'lot-katana-0042',
    'LOT-0042',
    'OBSIDIAN KATANA',
    'SERIAL #0042',
    'OBSIDIAN',
    '#34D399',
    'Assets/card_monolith.png',
    'CERTIFIED',
    'AV-9F2C-0042',
    'FORGED SECTOR 09 · 3 CUSTODIANS · ESCROW HELD',
    38000.00,
    78000.00,
    52400.00,
    137,
    NULL,
    'SHOGUN_07',
    'LEVEL 42 · VERIFIED COLLECTOR',
    NOW(),
    DATE_ADD(NOW(), INTERVAL 2 HOUR + INTERVAL 41 MINUTE + INTERVAL 17 SECOND),
    'ACTIVE'
) ON DUPLICATE KEY UPDATE `current_bid` = VALUES(`current_bid`);

-- Seed Auction Attributes
INSERT INTO `auction_attributes` (`lot_id`, `label`, `value`, `mono_value`, `is_accent`, `sort_order`)
VALUES
('lot-katana-0042', 'ORIGIN', 'FORGE SECTOR 09 — NEO-BRNO', NULL, 0, 1),
('lot-katana-0042', 'FORGE DATE', '14 · 09 · 2089', '14·09·2089', 0, 2),
('lot-katana-0042', 'ALLOY', 'CARBON-LATTICE OBSIDIAN', NULL, 0, 3),
('lot-katana-0042', 'EDGE GEOMETRY', 'SINGLE BEVEL · 11.4°', NULL, 0, 4),
('lot-katana-0042', 'MASS', '1.18 KG', NULL, 0, 5),
('lot-katana-0042', 'OWNER HISTORY', '3 REGISTERED CUSTODIANS', NULL, 0, 6),
('lot-katana-0042', 'CERTIFICATION', 'AV-9F2C-0042', 'AV-9F2C-0042', 1, 7),
('lot-katana-0042', 'VAULT STATUS', 'ESCROWED · TRANSFER LOCKED', NULL, 1, 8);

-- Seed Hammer Prices
INSERT INTO `hammer_history` (`id`, `item_name`, `serial_tag`, `price`, `delta_pct`, `delta_is_positive`, `when_label`, `tone_hex`)
VALUES
('hp-001', 'OBSIDIAN KATANA', '#0017', 48200.00, '+12.4%', 1, '6M AGO', '#34D399'),
('hp-002', 'ECLIPSE VISOR', '#0231', 21900.00, '+4.1%', 1, '22M AGO', '#F87171'),
('hp-003', 'VOID LANCE', '#0008', 63750.00, '+18.9%', 1, '48M AGO', '#C084FC'),
('hp-004', 'GREY MANTLE', '#0444', 7400.00, '-2.6%', 0, '1H AGO', '#8A8F99'),
('hp-005', 'SOLARIS EDGE', '#0102', 15150.00, '+0.8%', 1, '2H AGO', '#F5A524');

-- Seed Vault Drops
INSERT INTO `vault_drops` (`id`, `name`, `edition`, `rarity`, `tone_hex`, `drop_window`, `progress`, `image_path`, `is_active`)
VALUES
('vd-001', 'ECLIPSE VISOR', 'SERIAL EDITION · 1 OF 120', 'MYTHIC', '#F87171', 'DROPS IN 04:12:09', 0.720, 'Assets/card_synthesis.png', 1),
('vd-002', 'VOID LANCE', 'FORGE RUN · 1 OF 40', 'LEGENDARY', '#C084FC', 'DROPS IN 11:40:55', 0.350, 'Assets/card_oscillation.png', 1),
('vd-003', 'GREY MANTLE', 'COMBAT PROVENANCE · 1 OF 300', 'RARE', '#60A5FA', 'DROPS IN 1D 02:15', 0.910, 'Assets/card_grey.png', 1),
('vd-004', 'MONOLITH SHARD', 'ARCHIVE CAST · 1 OF 24', 'OBSIDIAN', '#34D399', 'DROPS IN 2D 06:30', 0.180, 'Assets/card_monolith.png', 1);

-- Seed News
INSERT INTO `news_feed` (`id`, `category`, `title`, `meta_tag`, `tone_hex`, `is_live`, `content`)
VALUES
('news-001', 'LIVE', 'OBSIDIAN KATANA LOT #0042 CROSSES $52K', '2M AGO', '#34D399', 1, 'Heavy bidding activity recorded on Sector 09 auction floor. Current top bid held by SHOGUN_07.'),
('news-002', 'PATCH NOTES', '2.4.1 — DIRECTSTORAGE STREAMING REBALANCE', '1H AGO', '#8A8F99', 0, 'DirectStorage 2.0 streaming pipeline asset stream optimizations reaching up to 6.4 GB/s.'),
('news-003', 'VAULT DROP', 'SERIAL EDITION ALLOCATION OPENS FRIDAY', '4H AGO', '#F5A524', 0, 'Exclusive allocations for verified collectors. Pre-registration open now.'),
('news-004', 'ESPORTS', 'APEX CIRCUIT QUALIFIERS — REGIONAL BRACKET', '9H AGO', '#60A5FA', 0, 'Regional qualifiers scheduled across 16 teams. Match telemetry to broadcast live.'),
('news-005', 'COMMUNITY', 'GREY PERIMETER MOD TOOLKIT 1.2 SHIPPED', '1D AGO', '#C084FC', 0, 'Custom scenario editor and weapon balancing mods now deployable directly to local clients.');
