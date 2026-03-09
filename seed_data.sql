-- =====================================================
-- Seed Data for CycleTrust MVP
-- =====================================================

USE cycle_trust_db;

-- Insert Brands
INSERT INTO brands (name, is_active) VALUES
('Giant', 1),
('Trek', 1),
('Specialized', 1),
('Canyon', 1),
('Cannondale', 1),
('Scott', 1),
('Merida', 1);

-- Insert Bike Categories
INSERT INTO bike_categories (name, is_active) VALUES
('Road Bike', 1),
('Mountain Bike', 1),
('Gravel Bike', 1),
('Touring Bike', 1),
('Hybrid Bike', 1),
('BMX', 1);

-- Insert Size Options
INSERT INTO size_options (label, is_active) VALUES
('XS (44-47cm)', 1),
('S (48-51cm)', 1),
('M (52-55cm)', 1),
('L (56-58cm)', 1),
('XL (59-62cm)', 1),
('XXL (63-65cm)', 1);

-- Insert Default Deposit Policy
INSERT INTO deposit_policies (is_active, policy_name, mode, percent_value, fixed_amount, min_amount, max_amount, note)
VALUES (1, 'Default 10% Policy', 'PERCENT', 10.00, NULL, 500000, 5000000, 'Đặt cọc 10% giá trị xe, tối thiểu 500k, tối đa 5 triệu');

-- Insert Sample Users
INSERT INTO users (email, phone, password_hash, role, full_name, is_active, approval_status) VALUES
-- password: 123456
('admin@cycletrust.com', '0900000001', '$2a$11$A3CCBmPuvL5btcMYacfe.OVlAelYMzW9ZQhn7lkXxvxJDrIHHo5Fi', 'ADMIN', 'Admin User', 1, 1),
('inspector@cycletrust.com', '0900000002', '$2a$11$A3CCBmPuvL5btcMYacfe.OVlAelYMzW9ZQhn7lkXxvxJDrIHHo5Fi', 'INSPECTOR', 'Inspector User', 1, 1),
('seller1@test.com', '0900000101', '$2a$11$A3CCBmPuvL5btcMYacfe.OVlAelYMzW9ZQhn7lkXxvxJDrIHHo5Fi', 'SELLER', 'Nguyễn Văn A', 1, 1),
('buyer1@test.com', '0900000201', '$2a$11$A3CCBmPuvL5btcMYacfe.OVlAelYMzW9ZQhn7lkXxvxJDrIHHo5Fi', 'BUYER', 'Trần Thị B', 1, 1);

-- Sample Listing (seller1)
INSERT INTO listings (seller_id, title, description, usage_history, location_text, brand_id, category_id, size_option_id, price_amount, currency, condition_note, year_model, status)
VALUES (3, 'Giant TCR Advanced Pro 2022', 'Xe đua đường trường cao cấp với khung carbon full', 'Đã sử dụng 1 năm, đi khoảng 2000km chủ yếu tập luyện', 'Hà Nội', 1, 1, 3, 25000000, 'VND', 'Tình trạng tốt, không tai nạn, bảo dưỡng định kỳ', 2022, 'PENDING_APPROVAL');

-- Sample Media for listing
INSERT INTO listing_media (listing_id, type, url, sort_order) VALUES
(1, 'IMAGE', 'https://cdn.chotot.com/CnEPEcghf5_v75qBUq0ZM9nDuKsYjshvAD1G5BY2jYQ/preset:listing/plain/35129bdf71d678b64f45d45209662f82-2972970484124723730.jpg', 0),
(1, 'IMAGE', 'https://danhgiathuonghieu.vn/wp-content/uploads/2025/05/xe-dap-the-thao-bien-hoa-5.jpg', 1);

COMMIT;
