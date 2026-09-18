-- =============================================
-- NauraLauncher - Conquer Online
-- MySQL 5.6 Compatible Schema
-- =============================================
-- Compatibility notes for MySQL 5.6:
-- - No JSON type (using TEXT)
-- - No GENERATED columns
-- - Using TIMESTAMP with DEFAULT CURRENT_TIMESTAMP (compatible from 5.6.5+)
-- - For older 5.6.0-5.6.4, TIMESTAMP still works, DATETIME DEFAULT would fail
-- - Using utf8 charset (utf8mb4 also works but utf8 is safest for 5.6)
-- - ENGINE=InnoDB for all tables
-- - Foreign keys with ON DELETE CASCADE
-- - No CHECK constraints (enforced in application)
-- - No window functions, CTEs in schema
-- - Using INT UNSIGNED, BIGINT UNSIGNED where appropriate
-- =============================================

SET NAMES utf8;
SET FOREIGN_KEY_CHECKS = 0;

-- Drop existing tables in correct order (reverse of creation due to FKs)
DROP TABLE IF EXISTS `voice_calls`;
DROP TABLE IF EXISTS `voice_channels`;
DROP TABLE IF EXISTS `blocked_users`;
DROP TABLE IF EXISTS `friend_requests`;
DROP TABLE IF EXISTS `friendships`;
DROP TABLE IF EXISTS `auction_bids`;
DROP TABLE IF EXISTS `auction_lots`;
DROP TABLE IF EXISTS `marketplace_transactions`;
DROP TABLE IF EXISTS `marketplace_listings`;
DROP TABLE IF EXISTS `inventory_items`;
DROP TABLE IF EXISTS `wallets`;
DROP TABLE IF EXISTS `users`;

-- =============================================
-- USERS TABLE - Conquer Online heroes
-- =============================================
CREATE TABLE `users` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `username` VARCHAR(50) NOT NULL,
  `email` VARCHAR(100) NOT NULL,
  `password_hash` VARCHAR(255) NOT NULL,
  `display_name` VARCHAR(50) NOT NULL,
  `main_class` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=Trojan,1=Warrior,2=Archer,3=FireTaoist,4=WaterTaoist,5=Ninja,6=Monk,7=Pirate,8=DragonWarrior',
  `level` SMALLINT UNSIGNED NOT NULL DEFAULT 1,
  `reborn_stage` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=None,1=First,2=Second',
  `avatar_path` VARCHAR(255) NOT NULL DEFAULT 'Assets/avatar_conquer.png',
  `status_message` VARCHAR(100) NOT NULL DEFAULT 'Online in Twin City',
  `is_online` TINYINT(1) NOT NULL DEFAULT 0,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `last_login_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_username` (`username`),
  UNIQUE KEY `uk_email` (`email`),
  KEY `idx_level` (`level`),
  KEY `idx_main_class` (`main_class`),
  KEY `idx_is_online` (`is_online`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Conquer Online player accounts';

-- =============================================
-- WALLETS TABLE - CPs, Gold, Silver, Bound CPs
-- =============================================
CREATE TABLE `wallets` (
  `user_id` INT UNSIGNED NOT NULL,
  `cps` BIGINT NOT NULL DEFAULT 0 COMMENT 'Conquer Points - premium currency',
  `bound_cps` BIGINT NOT NULL DEFAULT 0 COMMENT 'Bound CPs - non-tradable',
  `gold` BIGINT NOT NULL DEFAULT 0 COMMENT 'Gold - trade currency',
  `silver` BIGINT NOT NULL DEFAULT 0 COMMENT 'Silver - basic currency',
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`user_id`),
  CONSTRAINT `fk_wallet_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Player wallets with 4 currencies';

-- =============================================
-- INVENTORY ITEMS - Conquer Online items
-- Dragon Balls, Gems, Weapons +12, etc.
-- =============================================
CREATE TABLE `inventory_items` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL,
  `name` VARCHAR(100) NOT NULL COMMENT 'e.g. Dragon Blade +12',
  `description` TEXT NOT NULL,
  `type` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=Weapon,1=Armor,2=Headgear,3=Necklace,4=Ring,5=Boots,6=Garment,7=Mount,8=Consumable,9=Gem,10=Material',
  `rarity` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=Common,1=Rare,2=Elite,3=Super,4=Epic,5=Legendary,6=Mythic',
  `level_required` SMALLINT UNSIGNED NOT NULL DEFAULT 1,
  `class_restriction` TINYINT UNSIGNED DEFAULT NULL COMMENT 'NULL=any, otherwise ConquerClass',
  `plus` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '+1 to +12',
  `socket_count` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0-2 sockets',
  `socket_gems` VARCHAR(255) NOT NULL DEFAULT '' COMMENT 'e.g. Super Dragon Gem, Refined Phoenix Gem',
  `is_bound` TINYINT(1) NOT NULL DEFAULT 0,
  `is_locked` TINYINT(1) NOT NULL DEFAULT 0,
  `durability` SMALLINT UNSIGNED NOT NULL DEFAULT 100,
  `quantity` INT UNSIGNED NOT NULL DEFAULT 1 COMMENT 'For stackable items like DragonBalls',
  `icon_path` VARCHAR(255) NOT NULL DEFAULT 'Assets/item_dragonball.png',
  `image_path` VARCHAR(255) NOT NULL DEFAULT 'Assets/card_trojan.png',
  `is_tradable` TINYINT(1) NOT NULL DEFAULT 1,
  `is_sellable` TINYINT(1) NOT NULL DEFAULT 1,
  `acquired_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_user_id` (`user_id`),
  KEY `idx_type` (`type`),
  KEY `idx_rarity` (`rarity`),
  KEY `idx_is_tradable` (`is_tradable`),
  CONSTRAINT `fk_inventory_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Conquer Online inventory - weapons, gems, Dragon Balls';

-- =============================================
-- MARKETPLACE LISTINGS - Twin City Market (178,182)
-- =============================================
CREATE TABLE `marketplace_listings` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `seller_id` INT UNSIGNED NOT NULL,
  `seller_name` VARCHAR(50) NOT NULL,
  `item_id` INT UNSIGNED NOT NULL,
  `price` BIGINT NOT NULL COMMENT 'Price in selected currency',
  `currency` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=Cps,1=Gold,2=Silver,3=BoundCps',
  `status` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=Active,1=Sold,2=Cancelled,3=Expired',
  `buyer_id` INT UNSIGNED DEFAULT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `expires_at` TIMESTAMP NULL DEFAULT NULL,
  `sold_at` TIMESTAMP NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_seller_id` (`seller_id`),
  KEY `idx_item_id` (`item_id`),
  KEY `idx_status` (`status`),
  KEY `idx_currency` (`currency`),
  KEY `idx_created_at` (`created_at`),
  CONSTRAINT `fk_marketplace_seller` FOREIGN KEY (`seller_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_marketplace_item` FOREIGN KEY (`item_id`) REFERENCES `inventory_items` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Twin City Marketplace listings';

CREATE TABLE `marketplace_transactions` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `listing_id` INT UNSIGNED NOT NULL,
  `seller_id` INT UNSIGNED NOT NULL,
  `buyer_id` INT UNSIGNED NOT NULL,
  `price` BIGINT NOT NULL,
  `currency` TINYINT UNSIGNED NOT NULL DEFAULT 0,
  `completed_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_listing_id` (`listing_id`),
  KEY `idx_seller_id` (`seller_id`),
  KEY `idx_buyer_id` (`buyer_id`),
  KEY `idx_completed_at` (`completed_at`),
  CONSTRAINT `fk_transaction_listing` FOREIGN KEY (`listing_id`) REFERENCES `marketplace_listings` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Marketplace transaction history';

-- =============================================
-- AUCTION LOTS - Auction House
-- =============================================
CREATE TABLE `auction_lots` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `seller_id` INT UNSIGNED NOT NULL,
  `seller_name` VARCHAR(50) NOT NULL,
  `item_id` INT UNSIGNED NOT NULL,
  `starting_price` BIGINT NOT NULL,
  `current_bid` BIGINT NOT NULL DEFAULT 0,
  `current_bidder_id` INT UNSIGNED DEFAULT NULL,
  `current_bidder_name` VARCHAR(50) DEFAULT NULL,
  `buyout_price` BIGINT NOT NULL DEFAULT 0 COMMENT '0=no buyout',
  `currency` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=Cps,1=Gold',
  `bid_count` INT UNSIGNED NOT NULL DEFAULT 0,
  `status` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=Active,1=Sold,2=Expired,3=Cancelled',
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ends_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `sold_at` TIMESTAMP NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_seller_id` (`seller_id`),
  KEY `idx_item_id` (`item_id`),
  KEY `idx_status` (`status`),
  KEY `idx_ends_at` (`ends_at`),
  KEY `idx_current_bidder` (`current_bidder_id`),
  CONSTRAINT `fk_auction_seller` FOREIGN KEY (`seller_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_auction_item` FOREIGN KEY (`item_id`) REFERENCES `inventory_items` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Auction house lots';

CREATE TABLE `auction_bids` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `lot_id` INT UNSIGNED NOT NULL,
  `bidder_id` INT UNSIGNED NOT NULL,
  `bidder_name` VARCHAR(50) NOT NULL,
  `amount` BIGINT NOT NULL,
  `currency` TINYINT UNSIGNED NOT NULL DEFAULT 0,
  `placed_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_lot_id` (`lot_id`),
  KEY `idx_bidder_id` (`bidder_id`),
  KEY `idx_placed_at` (`placed_at`),
  CONSTRAINT `fk_bid_lot` FOREIGN KEY (`lot_id`) REFERENCES `auction_lots` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_bid_bidder` FOREIGN KEY (`bidder_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Auction bid history';

-- =============================================
-- FRIENDS SYSTEM - Friend list, add/remove/block
-- =============================================
CREATE TABLE `friendships` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL COMMENT 'Requester',
  `friend_id` INT UNSIGNED NOT NULL COMMENT 'Target',
  `status` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=Pending,1=Accepted,2=Blocked,3=Declined',
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `accepted_at` TIMESTAMP NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_user_friend` (`user_id`, `friend_id`),
  KEY `idx_friend_id` (`friend_id`),
  KEY `idx_status` (`status`),
  CONSTRAINT `fk_friendship_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_friendship_friend` FOREIGN KEY (`friend_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Friendships - bidirectional after acceptance';

CREATE TABLE `friend_requests` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `from_user_id` INT UNSIGNED NOT NULL,
  `from_username` VARCHAR(50) NOT NULL,
  `to_user_id` INT UNSIGNED NOT NULL,
  `message` VARCHAR(255) NOT NULL DEFAULT '',
  `sent_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_from_user` (`from_user_id`),
  KEY `idx_to_user` (`to_user_id`),
  KEY `idx_sent_at` (`sent_at`),
  CONSTRAINT `fk_request_from` FOREIGN KEY (`from_user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_request_to` FOREIGN KEY (`to_user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Pending friend requests';

CREATE TABLE `blocked_users` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL COMMENT 'Who blocked',
  `blocked_id` INT UNSIGNED NOT NULL COMMENT 'Who is blocked',
  `blocked_name` VARCHAR(50) NOT NULL,
  `reason` VARCHAR(255) NOT NULL DEFAULT '',
  `blocked_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_user_blocked` (`user_id`, `blocked_id`),
  KEY `idx_blocked_id` (`blocked_id`),
  CONSTRAINT `fk_block_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_block_blocked` FOREIGN KEY (`blocked_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Blocked players - prevents trade, voice, friend requests';

-- =============================================
-- VOICE SYSTEM - Voice calls between players
-- =============================================
CREATE TABLE `voice_channels` (
  `channel_id` VARCHAR(36) NOT NULL COMMENT 'UUID',
  `type` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=Direct,1=Party,2=Guild,3=Team',
  `name` VARCHAR(100) NOT NULL,
  `created_by` INT UNSIGNED NOT NULL,
  `participant_ids` TEXT NOT NULL COMMENT 'Comma-separated user IDs - using TEXT not JSON for MySQL 5.6',
  `is_active` TINYINT(1) NOT NULL DEFAULT 1,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`channel_id`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_is_active` (`is_active`),
  KEY `idx_type` (`type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Voice channels - party, guild, direct calls';

CREATE TABLE `voice_calls` (
  `call_id` VARCHAR(36) NOT NULL COMMENT 'UUID',
  `channel_id` VARCHAR(36) NOT NULL,
  `caller_id` INT UNSIGNED NOT NULL,
  `caller_name` VARCHAR(50) NOT NULL,
  `callee_id` INT UNSIGNED NOT NULL,
  `callee_name` VARCHAR(50) NOT NULL,
  `status` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '0=Initiating,1=Ringing,2=Active,3=Ended,4=Declined,5=Failed',
  `started_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `answered_at` TIMESTAMP NULL DEFAULT NULL,
  `ended_at` TIMESTAMP NULL DEFAULT NULL,
  `end_reason` VARCHAR(100) DEFAULT NULL,
  PRIMARY KEY (`call_id`),
  KEY `idx_channel_id` (`channel_id`),
  KEY `idx_caller_id` (`caller_id`),
  KEY `idx_callee_id` (`callee_id`),
  KEY `idx_status` (`status`),
  KEY `idx_started_at` (`started_at`),
  CONSTRAINT `fk_call_caller` FOREIGN KEY (`caller_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_call_callee` FOREIGN KEY (`callee_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Voice call history';

-- =============================================
-- SEED DATA - Conquer Online themed
-- =============================================

-- Insert default admin user (password: admin123 - hashed with PBKDF2)
-- Hash for admin123 is example - real hash should be generated by PasswordHasher
INSERT INTO `users` (`id`, `username`, `email`, `display_name`, `password_hash`, `main_class`, `level`, `reborn_stage`, `is_online`) VALUES
(1, 'admin', 'admin@conquer.local', 'ConquerAdmin', '$2a$11$examplehashforadmin123', 0, 130, 2, 0),
(2, 'DragonLord', 'dragon@conquer.local', 'DragonLord', '', 0, 130, 2, 1),
(3, 'FireQueen', 'fire@conquer.local', 'FireQueen', '', 3, 125, 1, 1),
(4, 'ShadowNinja', 'ninja@conquer.local', 'ShadowNinja', '', 5, 120, 2, 0),
(5, 'HolyMonk', 'monk@conquer.local', 'HolyMonk', '', 6, 115, 1, 1);

INSERT INTO `wallets` (`user_id`, `cps`, `bound_cps`, `gold`, `silver`) VALUES
(1, 50000, 1000, 10000000, 5000000),
(2, 25000, 500, 5000000, 2000000),
(3, 15000, 300, 3000000, 1500000),
(4, 10000, 200, 2000000, 1000000),
(5, 8000, 100, 1500000, 800000);

-- Sample inventory items
INSERT INTO `inventory_items` (`id`, `user_id`, `name`, `description`, `type`, `rarity`, `level_required`, `plus`, `socket_count`, `socket_gems`, `quantity`, `is_tradable`, `is_bound`) VALUES
(1, 1, 'Dragon Blade', 'Legendary blade forged in Twin City, +12 with 2 sockets', 0, 5, 120, 12, 2, 'Super Dragon Gem, Super Phoenix Gem', 1, 1, 0),
(2, 1, 'Super Dragon Gem', 'Increases attack power significantly', 9, 3, 1, 0, 0, '', 5, 1, 0),
(3, 1, 'Dragon Ball', 'Mysterious orb containing dragon power, used for rebirth and upgrades', 8, 4, 1, 0, 0, '', 27, 1, 0),
(4, 2, 'Heaven Fan', 'Water Taoist fan, +9 with healing boost', 0, 4, 110, 9, 1, 'Super Moon Gem', 1, 1, 0),
(5, 2, 'Meteor Scroll', 'Fire Taoist spell scroll - Meteor', 8, 1, 40, 0, 0, '', 10, 1, 0),
(6, 3, 'Ninja Katana', 'Twin katanas, +8, poison effect', 0, 2, 100, 8, 2, '', 1, 1, 0),
(7, 4, 'Super Armor - Trojan', 'Super Conquer Armor for Trojan, +12, 2 sockets', 1, 3, 120, 12, 2, '', 1, 1, 0);

-- Sample friendships
INSERT INTO `friendships` (`user_id`, `friend_id`, `status`, `accepted_at`) VALUES
(1, 2, 1, CURRENT_TIMESTAMP),
(2, 1, 1, CURRENT_TIMESTAMP),
(1, 3, 1, CURRENT_TIMESTAMP),
(3, 1, 1, CURRENT_TIMESTAMP),
(1, 4, 1, CURRENT_TIMESTAMP),
(4, 1, 1, CURRENT_TIMESTAMP);

SET FOREIGN_KEY_CHECKS = 1;

-- =============================================
-- NOTES FOR C# SERVER IMPLEMENTATION
-- =============================================
-- 1. Use MySqlConnector library (compatible with MySQL 5.6)
--    Connection string: Server=localhost;Port=3306;Database=nauralauncher;Uid=root;Pwd=;CharSet=utf8;
--
-- 2. Password hashing: Use PBKDF2 (Rfc2898DeriveBytes) - NOT bcrypt which may need newer libs
--    Salt size 16, hash size 32, iterations 10000
--
-- 3. For voice calls: WebSocket server on port 8081, API on 8080
--    Signaling protocol: JSON { type, channelId, callId, fromUserId, toUserId, payload }
--    Types: offer, answer, ice-candidate, call-incoming, call-accepted, etc.
--
-- 4. Currency handling: Always use BIGINT for CPs/Gold to prevent overflow
--    CPs can go up to 2B+ in Conquer Online
--
-- 5. MySQL 5.6 specific fixes applied:
--    - No JSON columns, using TEXT for participant_ids and payloads
--    - TIMESTAMP instead of DATETIME for default CURRENT_TIMESTAMP
--    - No generated columns
--    - utf8 charset instead of utf8mb4 for max compatibility (can upgrade to utf8mb4 if MySQL 5.6.5+)
--    - All tables InnoDB
--
-- 6. For production: Add indexes on frequently queried columns (already done)
--    Consider partitioning for large tables like auction_bids, marketplace_transactions
-- =============================================
