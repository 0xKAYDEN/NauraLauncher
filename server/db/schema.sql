-- =============================================================================
-- NauraLauncher Production MySQL Database Schema
-- Version: 2.4.0
-- Charset: utf8mb4, Collation: utf8mb4_unicode_ci
-- =============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- -----------------------------------------------------------------------------
-- 1. USERS & AUTHENTICATION
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `users` (
    `id` VARCHAR(36) NOT NULL,
    `username` VARCHAR(50) NOT NULL,
    `email` VARCHAR(255) NOT NULL,
    `password_hash` VARCHAR(255) NOT NULL,
    `salt` VARCHAR(64) NOT NULL,
    `role` VARCHAR(20) NOT NULL DEFAULT 'USER',
    `credits` DECIMAL(12, 2) NOT NULL DEFAULT 150.00,
    `avatar_url` VARCHAR(255) DEFAULT 'Assets/avatar.png',
    `status` VARCHAR(20) NOT NULL DEFAULT 'ONLINE',
    `is_banned` TINYINT(1) NOT NULL DEFAULT 0,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `last_login_at` TIMESTAMP NULL DEFAULT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_username` (`username`),
    UNIQUE KEY `uk_email` (`email`),
    INDEX `idx_users_role` (`role`),
    INDEX `idx_users_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 2. USER SESSIONS & REFRESH TOKENS (Secure token rotation)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `user_sessions` (
    `id` VARCHAR(36) NOT NULL,
    `user_id` VARCHAR(36) NOT NULL,
    `refresh_token_hash` VARCHAR(64) NOT NULL,
    `ip_address` VARCHAR(45) DEFAULT NULL,
    `user_agent` VARCHAR(255) DEFAULT NULL,
    `expires_at` TIMESTAMP NOT NULL,
    `revoked` TINYINT(1) NOT NULL DEFAULT 0,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_sessions_user` (`user_id`),
    INDEX `idx_sessions_token_hash` (`refresh_token_hash`),
    INDEX `idx_sessions_expires` (`expires_at`),
    CONSTRAINT `fk_sessions_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 3. GAMES CATALOG
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `games` (
    `id` VARCHAR(36) NOT NULL,
    `slug` VARCHAR(100) NOT NULL,
    `title` VARCHAR(150) NOT NULL,
    `studio` VARCHAR(100) NOT NULL,
    `description` TEXT,
    `category` VARCHAR(50) NOT NULL DEFAULT 'Tactical RPG',
    `release_tag` VARCHAR(50) NOT NULL DEFAULT 'NEW RELEASE',
    `ribbon` VARCHAR(50) DEFAULT '96% POSITIVE',
    `ribbon_is_accent` TINYINT(1) NOT NULL DEFAULT 1,
    `price` DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    `discount_pct` INT NOT NULL DEFAULT 0,
    `image_path` VARCHAR(255) NOT NULL,
    `hero_image_path` VARCHAR(255) DEFAULT NULL,
    `executable_name` VARCHAR(100) NOT NULL DEFAULT 'game.exe',
    `current_version` VARCHAR(30) NOT NULL DEFAULT '1.0.0',
    `install_size_bytes` BIGINT NOT NULL DEFAULT 1073741824,
    `is_featured` TINYINT(1) NOT NULL DEFAULT 0,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_games_slug` (`slug`),
    INDEX `idx_games_category` (`category`),
    INDEX `idx_games_featured` (`is_featured`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 4. GAME MANIFESTS (Integrity verification & delta patching)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `game_manifests` (
    `id` VARCHAR(36) NOT NULL,
    `game_id` VARCHAR(36) NOT NULL,
    `version` VARCHAR(30) NOT NULL,
    `relative_path` VARCHAR(255) NOT NULL,
    `file_size_bytes` BIGINT NOT NULL,
    `sha256_hash` VARCHAR(64) NOT NULL,
    `download_url` VARCHAR(255) NOT NULL,
    `is_executable` TINYINT(1) NOT NULL DEFAULT 0,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_manifests_game_ver` (`game_id`, `version`),
    CONSTRAINT `fk_manifests_game` FOREIGN KEY (`game_id`) REFERENCES `games` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 5. USER LIBRARY & ENTITLEMENTS
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `user_library` (
    `id` VARCHAR(36) NOT NULL,
    `user_id` VARCHAR(36) NOT NULL,
    `game_id` VARCHAR(36) NOT NULL,
    `license_key` VARCHAR(64) NOT NULL,
    `status` VARCHAR(30) NOT NULL DEFAULT 'READY',
    `playtime_seconds` BIGINT NOT NULL DEFAULT 0,
    `campaign_progress` DECIMAL(4, 3) NOT NULL DEFAULT 0.000,
    `installed_path` VARCHAR(255) DEFAULT NULL,
    `installed_version` VARCHAR(30) DEFAULT NULL,
    `is_favorite` TINYINT(1) NOT NULL DEFAULT 0,
    `last_played_at` TIMESTAMP NULL DEFAULT NULL,
    `acquired_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_user_game` (`user_id`, `game_id`),
    INDEX `idx_library_user` (`user_id`),
    INDEX `idx_library_game` (`game_id`),
    CONSTRAINT `fk_library_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_library_game` FOREIGN KEY (`game_id`) REFERENCES `games` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 6. CLOUD SAVES
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `cloud_saves` (
    `id` VARCHAR(36) NOT NULL,
    `user_id` VARCHAR(36) NOT NULL,
    `game_id` VARCHAR(36) NOT NULL,
    `slot_index` INT NOT NULL DEFAULT 0,
    `save_data_base64` LONGTEXT NOT NULL,
    `sha256_checksum` VARCHAR(64) NOT NULL,
    `size_bytes` INT NOT NULL,
    `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_user_game_slot` (`user_id`, `game_id`, `slot_index`),
    CONSTRAINT `fk_cloud_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_cloud_game` FOREIGN KEY (`game_id`) REFERENCES `games` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 7. AUCTION LOTS (Real-time Floor)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `auction_lots` (
    `id` VARCHAR(36) NOT NULL,
    `lot_code` VARCHAR(50) NOT NULL,
    `title` VARCHAR(150) NOT NULL,
    `serial_tag` VARCHAR(50) NOT NULL,
    `rarity` VARCHAR(30) NOT NULL DEFAULT 'OBSIDIAN',
    `tone_hex` VARCHAR(10) NOT NULL DEFAULT '#34D399',
    `image_path` VARCHAR(255) NOT NULL,
    `cert_label` VARCHAR(50) NOT NULL DEFAULT 'CERTIFIED',
    `cert_code` VARCHAR(50) NOT NULL DEFAULT 'AV-9F2C-0042',
    `provenance` TEXT NOT NULL,
    `floor_price` DECIMAL(12, 2) NOT NULL DEFAULT 38000.00,
    `buyout_price` DECIMAL(12, 2) NOT NULL DEFAULT 78000.00,
    `current_bid` DECIMAL(12, 2) NOT NULL DEFAULT 52400.00,
    `bid_count` INT NOT NULL DEFAULT 137,
    `top_bidder_id` VARCHAR(36) DEFAULT NULL,
    `top_bidder_name` VARCHAR(50) NOT NULL DEFAULT 'SHOGUN_07',
    `top_bidder_meta` VARCHAR(100) NOT NULL DEFAULT 'LEVEL 42 · VERIFIED COLLECTOR',
    `starts_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ends_at` TIMESTAMP NOT NULL,
    `status` VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_auction_status` (`status`),
    INDEX `idx_auction_rarity` (`rarity`),
    INDEX `idx_auction_ends` (`ends_at`),
    CONSTRAINT `fk_auction_bidder` FOREIGN KEY (`top_bidder_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 8. AUCTION ATTRIBUTES
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `auction_attributes` (
    `id` INT AUTO_INCREMENT NOT NULL,
    `lot_id` VARCHAR(36) NOT NULL,
    `label` VARCHAR(100) NOT NULL,
    `value` VARCHAR(255) NOT NULL,
    `mono_value` VARCHAR(100) DEFAULT NULL,
    `is_accent` TINYINT(1) NOT NULL DEFAULT 0,
    `sort_order` INT NOT NULL DEFAULT 0,
    PRIMARY KEY (`id`),
    INDEX `idx_attr_lot` (`lot_id`),
    CONSTRAINT `fk_attr_lot` FOREIGN KEY (`lot_id`) REFERENCES `auction_lots` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 9. AUCTION BIDS (Audit Trail & Real-time History)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `auction_bids` (
    `id` VARCHAR(36) NOT NULL,
    `lot_id` VARCHAR(36) NOT NULL,
    `user_id` VARCHAR(36) NOT NULL,
    `bidder_name` VARCHAR(50) NOT NULL,
    `amount` DECIMAL(12, 2) NOT NULL,
    `status` VARCHAR(20) NOT NULL DEFAULT 'ACCEPTED',
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_bids_lot` (`lot_id`),
    INDEX `idx_bids_user` (`user_id`),
    INDEX `idx_bids_created` (`created_at`),
    CONSTRAINT `fk_bids_lot` FOREIGN KEY (`lot_id`) REFERENCES `auction_lots` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_bids_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 10. HAMMER PRICE SALES HISTORY
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `hammer_history` (
    `id` VARCHAR(36) NOT NULL,
    `item_name` VARCHAR(150) NOT NULL,
    `serial_tag` VARCHAR(50) NOT NULL,
    `price` DECIMAL(12, 2) NOT NULL,
    `delta_pct` VARCHAR(20) NOT NULL,
    `delta_is_positive` TINYINT(1) NOT NULL DEFAULT 1,
    `when_label` VARCHAR(50) NOT NULL,
    `tone_hex` VARCHAR(10) NOT NULL DEFAULT '#34D399',
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 11. VAULT DROPS
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `vault_drops` (
    `id` VARCHAR(36) NOT NULL,
    `name` VARCHAR(150) NOT NULL,
    `edition` VARCHAR(100) NOT NULL,
    `rarity` VARCHAR(30) NOT NULL,
    `tone_hex` VARCHAR(10) NOT NULL DEFAULT '#F87171',
    `drop_window` VARCHAR(50) NOT NULL,
    `progress` DECIMAL(4, 3) NOT NULL DEFAULT 0.000,
    `image_path` VARCHAR(255) NOT NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 12. NEWS & PATCH NOTES FEED
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `news_feed` (
    `id` VARCHAR(36) NOT NULL,
    `category` VARCHAR(50) NOT NULL,
    `title` VARCHAR(255) NOT NULL,
    `meta_tag` VARCHAR(50) NOT NULL,
    `tone_hex` VARCHAR(10) NOT NULL DEFAULT '#34D399',
    `is_live` TINYINT(1) NOT NULL DEFAULT 0,
    `content` TEXT,
    `published_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_news_published` (`published_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 13. USER SETTINGS (Cloud sync)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `user_settings` (
    `user_id` VARCHAR(36) NOT NULL,
    `settings_json` LONGTEXT NOT NULL,
    `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`user_id`),
    CONSTRAINT `fk_settings_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 14. SECURITY & AUDIT LOGS
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `security_audit_logs` (
    `id` VARCHAR(36) NOT NULL,
    `user_id` VARCHAR(36) DEFAULT NULL,
    `ip_address` VARCHAR(45) NOT NULL,
    `action` VARCHAR(50) NOT NULL,
    `status` VARCHAR(20) NOT NULL DEFAULT 'SUCCESS',
    `details` TEXT,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_audit_user` (`user_id`),
    INDEX `idx_audit_action` (`action`),
    INDEX `idx_audit_created` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SET FOREIGN_KEY_CHECKS = 1;
