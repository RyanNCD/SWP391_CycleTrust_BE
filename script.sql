-- =====================================================
-- MySQL 8+ Schema - MVP Used Sports Bike Marketplace
-- =====================================================

SET NAMES utf8mb4;
SET time_zone = '+00:00';

-- -------------------------
-- 1) USERS
-- -------------------------
CREATE TABLE users (
  id            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  email         VARCHAR(255) UNIQUE,
  phone         VARCHAR(30) UNIQUE,
  password_hash TEXT NULL,

  role          ENUM('BUYER','SELLER','ADMIN','INSPECTOR') NOT NULL,

  full_name     VARCHAR(255) NOT NULL,
  avatar_url    TEXT NULL,

  is_active     TINYINT(1) NOT NULL DEFAULT 1,

  rating_avg    DECIMAL(3,2) NOT NULL DEFAULT 0.00,
  rating_count  INT NOT NULL DEFAULT 0,

  created_at    TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  INDEX idx_users_role (role)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------
-- 2) CATALOG (Admin-managed)
-- -------------------------
CREATE TABLE brands (
  id         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  name       VARCHAR(120) NOT NULL UNIQUE,
  is_active  TINYINT(1) NOT NULL DEFAULT 1,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE bike_categories (
  id         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  name       VARCHAR(120) NOT NULL UNIQUE,  -- Road/MTB/Gravel...
  is_active  TINYINT(1) NOT NULL DEFAULT 1,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE size_options (
  id        BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  label     VARCHAR(50) NOT NULL UNIQUE,    -- 48/50/52 or S/M/L...
  is_active TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------
-- 3) LISTINGS
-- -------------------------
CREATE TABLE listings (
  id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  seller_id       BIGINT UNSIGNED NOT NULL,

  title           VARCHAR(255) NOT NULL,
  description     TEXT NOT NULL,
  usage_history   TEXT NULL,
  location_text   VARCHAR(255) NULL,

  brand_id        BIGINT UNSIGNED NULL,
  category_id     BIGINT UNSIGNED NULL,
  size_option_id  BIGINT UNSIGNED NULL,

  price_amount    BIGINT NOT NULL CHECK (price_amount >= 0),
  currency        VARCHAR(10) NOT NULL DEFAULT 'VND',

  condition_note  VARCHAR(255) NULL,
  year_model      INT NULL,

  status          ENUM(
    'DRAFT','PENDING_APPROVAL','APPROVED','REJECTED',
    'UNDER_INSPECTION','VERIFIED','SOLD','ARCHIVED'
  ) NOT NULL DEFAULT 'DRAFT',

  approved_by     BIGINT UNSIGNED NULL, -- admin user id
  approved_at     TIMESTAMP NULL,
  rejected_reason TEXT NULL,

  is_deleted      TINYINT(1) NOT NULL DEFAULT 0,

  created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT fk_listings_seller   FOREIGN KEY (seller_id) REFERENCES users(id),
  CONSTRAINT fk_listings_brand    FOREIGN KEY (brand_id) REFERENCES brands(id),
  CONSTRAINT fk_listings_category FOREIGN KEY (category_id) REFERENCES bike_categories(id),
  CONSTRAINT fk_listings_size     FOREIGN KEY (size_option_id) REFERENCES size_options(id),
  CONSTRAINT fk_listings_approved_by FOREIGN KEY (approved_by) REFERENCES users(id),

  INDEX idx_listings_seller (seller_id),
  INDEX idx_listings_status (status),
  INDEX idx_listings_brand (brand_id),
  INDEX idx_listings_category (category_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- Listing media (links only)
CREATE TABLE listing_media (
  id         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  listing_id BIGINT UNSIGNED NOT NULL,
  type       ENUM('IMAGE','VIDEO') NOT NULL,
  url        TEXT NOT NULL,
  sort_order INT NOT NULL DEFAULT 0,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CONSTRAINT fk_listing_media_listing FOREIGN KEY (listing_id) REFERENCES listings(id) ON DELETE CASCADE,
  INDEX idx_listing_media_listing (listing_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------
-- 4) INSPECTIONS (Inspector)
-- -------------------------
CREATE TABLE inspections (
  id             BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  listing_id     BIGINT UNSIGNED NOT NULL,
  inspector_id   BIGINT UNSIGNED NOT NULL,

  summary        TEXT NOT NULL,
  checklist_json JSON NULL,
  report_url     TEXT NULL,

  created_at     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT uq_inspections_listing UNIQUE (listing_id),
  CONSTRAINT fk_inspections_listing FOREIGN KEY (listing_id) REFERENCES listings(id) ON DELETE CASCADE,
  CONSTRAINT fk_inspections_inspector FOREIGN KEY (inspector_id) REFERENCES users(id),

  INDEX idx_inspections_inspector (inspector_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------
-- 5) WISHLIST
-- -------------------------
CREATE TABLE wishlists (
  buyer_id   BIGINT UNSIGNED NOT NULL,
  listing_id BIGINT UNSIGNED NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  PRIMARY KEY (buyer_id, listing_id),
  CONSTRAINT fk_wishlists_buyer   FOREIGN KEY (buyer_id) REFERENCES users(id) ON DELETE CASCADE,
  CONSTRAINT fk_wishlists_listing FOREIGN KEY (listing_id) REFERENCES listings(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------
-- 6) DEPOSIT POLICY (Admin config)
-- -------------------------
CREATE TABLE deposit_policies (
  id            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  is_active     TINYINT(1) NOT NULL DEFAULT 1,
  policy_name   VARCHAR(120) NOT NULL,

  mode          ENUM('PERCENT','FIXED') NOT NULL,
  percent_value DECIMAL(5,2) NULL,   -- 0..100
  fixed_amount  BIGINT NULL,

  min_amount    BIGINT NOT NULL DEFAULT 0,
  max_amount    BIGINT NULL,

  note          TEXT NULL,
  created_at    TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CHECK (percent_value IS NULL OR (percent_value >= 0 AND percent_value <= 100)),
  CHECK (fixed_amount IS NULL OR fixed_amount >= 0),
  CHECK (min_amount >= 0),
  CHECK (max_amount IS NULL OR max_amount >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------
-- 7) ORDERS + PAYMENTS
-- -------------------------
CREATE TABLE orders (
  id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  listing_id      BIGINT UNSIGNED NOT NULL,
  buyer_id        BIGINT UNSIGNED NOT NULL,
  seller_id       BIGINT UNSIGNED NOT NULL,

  status          ENUM(
    'PLACED','DEPOSIT_PENDING','DEPOSIT_PAID',
    'CONFIRMED','SHIPPING','DELIVERED','COMPLETED',
    'CANCELED','DISPUTED'
  ) NOT NULL DEFAULT 'PLACED',

  -- pricing snapshot
  price_amount    BIGINT NOT NULL CHECK (price_amount >= 0),
  currency        VARCHAR(10) NOT NULL DEFAULT 'VND',

  -- deposit snapshot
  deposit_required TINYINT(1) NOT NULL DEFAULT 1,
  deposit_amount   BIGINT NOT NULL DEFAULT 0 CHECK (deposit_amount >= 0),
  deposit_due_at   TIMESTAMP NULL,
  deposit_paid_at  TIMESTAMP NULL,

  -- optional: giữ chỗ sau khi đã cọc (khuyến nghị)
  reserve_expires_at TIMESTAMP NULL,

  -- fulfillment
  shipping_note   TEXT NULL,
  delivered_at    TIMESTAMP NULL,
  completed_at    TIMESTAMP NULL,
  canceled_reason TEXT NULL,

  created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT fk_orders_listing FOREIGN KEY (listing_id) REFERENCES listings(id),
  CONSTRAINT fk_orders_buyer   FOREIGN KEY (buyer_id) REFERENCES users(id),
  CONSTRAINT fk_orders_seller  FOREIGN KEY (seller_id) REFERENCES users(id),

  INDEX idx_orders_buyer (buyer_id),
  INDEX idx_orders_seller (seller_id),
  INDEX idx_orders_status (status),
  INDEX idx_orders_listing (listing_id),

  -- Trick MySQL: chặn 1 listing có nhiều order "đang active" cùng lúc
  -- active_listing_id sẽ = listing_id nếu status là active, ngược lại NULL
  active_listing_id BIGINT UNSIGNED
    GENERATED ALWAYS AS (
      CASE
        WHEN status IN ('PLACED','DEPOSIT_PENDING','DEPOSIT_PAID','CONFIRMED','SHIPPING','DISPUTED')
        THEN listing_id
        ELSE NULL
      END
    ) STORED,

  UNIQUE KEY uq_one_active_order_per_listing (active_listing_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE payments (
  id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  order_id        BIGINT UNSIGNED NOT NULL,

  type            ENUM('DEPOSIT','FULL','REFUND') NOT NULL,
  status          ENUM('PENDING','PAID','FAILED','REFUNDED') NOT NULL DEFAULT 'PENDING',

  amount          BIGINT NOT NULL CHECK (amount >= 0),
  currency        VARCHAR(10) NOT NULL DEFAULT 'VND',

  provider        VARCHAR(50) NULL,   -- vnpay/momo/stripe/cash...
  provider_txn_id VARCHAR(120) NULL,
  paid_at         TIMESTAMP NULL,

  created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CONSTRAINT fk_payments_order FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
  INDEX idx_payments_order (order_id),
  INDEX idx_payments_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------
-- 8) REVIEWS
-- -------------------------
CREATE TABLE reviews (
  id         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  order_id   BIGINT UNSIGNED NOT NULL,
  buyer_id   BIGINT UNSIGNED NOT NULL,
  seller_id  BIGINT UNSIGNED NOT NULL,

  rating     INT NOT NULL CHECK (rating BETWEEN 1 AND 5),
  comment    TEXT NULL,

  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CONSTRAINT uq_reviews_order UNIQUE (order_id),
  CONSTRAINT fk_reviews_order  FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
  CONSTRAINT fk_reviews_buyer  FOREIGN KEY (buyer_id) REFERENCES users(id),
  CONSTRAINT fk_reviews_seller FOREIGN KEY (seller_id) REFERENCES users(id),

  INDEX idx_reviews_seller (seller_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- -------------------------
-- 9) REPORTS & DISPUTES
-- -------------------------
CREATE TABLE violation_reports (
  id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  reporter_id     BIGINT UNSIGNED NOT NULL,
  listing_id      BIGINT UNSIGNED NULL,
  reported_user_id BIGINT UNSIGNED NULL,

  reason          VARCHAR(255) NOT NULL,
  details         TEXT NULL,

  status          ENUM('OPEN','IN_REVIEW','RESOLVED','REJECTED') NOT NULL DEFAULT 'OPEN',

  handled_by      BIGINT UNSIGNED NULL, -- admin
  handled_at      TIMESTAMP NULL,
  resolution_note TEXT NULL,

  created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT fk_reports_reporter FOREIGN KEY (reporter_id) REFERENCES users(id),
  CONSTRAINT fk_reports_listing  FOREIGN KEY (listing_id) REFERENCES listings(id),
  CONSTRAINT fk_reports_reported_user FOREIGN KEY (reported_user_id) REFERENCES users(id),
  CONSTRAINT fk_reports_handled_by FOREIGN KEY (handled_by) REFERENCES users(id),

  INDEX idx_reports_status (status),
  INDEX idx_reports_listing (listing_id),
  INDEX idx_reports_reported_user (reported_user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE disputes (
  id                   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  order_id             BIGINT UNSIGNED NOT NULL,
  opened_by            BIGINT UNSIGNED NOT NULL,

  status               ENUM('OPEN','ASSIGNED','IN_PROGRESS','RESOLVED','CLOSED') NOT NULL DEFAULT 'OPEN',

  assigned_inspector_id BIGINT UNSIGNED NULL,
  assigned_admin_id     BIGINT UNSIGNED NULL,

  summary              TEXT NOT NULL,
  resolution           TEXT NULL,
  resolved_at          TIMESTAMP NULL,

  created_at           TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at           TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  CONSTRAINT uq_disputes_order UNIQUE (order_id),
  CONSTRAINT fk_disputes_order FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
  CONSTRAINT fk_disputes_opened_by FOREIGN KEY (opened_by) REFERENCES users(id),
  CONSTRAINT fk_disputes_inspector FOREIGN KEY (assigned_inspector_id) REFERENCES users(id),
  CONSTRAINT fk_disputes_admin FOREIGN KEY (assigned_admin_id) REFERENCES users(id),

  INDEX idx_disputes_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE dispute_events (
  id         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  dispute_id BIGINT UNSIGNED NOT NULL,
  actor_id   BIGINT UNSIGNED NULL,
  message    TEXT NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  CONSTRAINT fk_dispute_events_dispute FOREIGN KEY (dispute_id) REFERENCES disputes(id) ON DELETE CASCADE,
  CONSTRAINT fk_dispute_events_actor FOREIGN KEY (actor_id) REFERENCES users(id),

  INDEX idx_dispute_events_dispute (dispute_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;