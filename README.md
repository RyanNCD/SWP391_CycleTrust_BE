# CycleTrust Backend API

API cho Website kết nối mua bán xe đạp thể thao cũ - Clean Architecture với .NET 8

## 📋 Yêu cầu

1. **.NET 8 SDK** - Download tại: https://dotnet.microsoft.com/download/dotnet/8.0
2. **MySQL 8+** đang chạy trên `localhost:3306`
3. **Database** `cycle_trust_db` đã tạo và import schema SQL đã cung cấp

## 🚀 Cài đặt & Chạy

### 1. Cài đặt .NET 8 SDK

```bash
# Kiểm tra version sau khi cài
dotnet --version
# Output: 8.0.x
```

### 2. Tạo Database

```sql
CREATE DATABASE cycle_trust_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

Sau đó import SQL schema đã được cung cấp.

### 3. Restore packages & Build

```bash
cd d:\SupportCode\Spring2026\SWP391_CycleTrust\SWP391_CycleTrust_BE
dotnet restore
dotnet build
```

### 4. Chạy API

```bash
cd src/CycleTrust.API
dotnet run
```

API sẽ chạy tại: **http://localhost:5000**  
Swagger UI: **http://localhost:5000/swagger**

---

## 📚 API Documentation

### 🔐 Authentication

#### 1. Đăng ký (Register)
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "seller1@test.com",
  "phone": "0123456789",
  "password": "123456",
  "fullName": "Nguyễn Văn A",
  "role": "SELLER"
}
```

**Roles**: `BUYER`, `SELLER`, `ADMIN`, `INSPECTOR`

**Response**:
```json
{
  "success": true,
  "message": "Đăng ký thành công",
  "data": {
    "userId": 1,
    "fullName": "Nguyễn Văn A",
    "role": "SELLER",
    "token": "eyJhbGciOiJIUzI1NiIs..."
  }
}
```

#### 2. Đăng nhập (Login)
```http
POST /api/auth/login
Content-Type: application/json

{
  "emailOrPhone": "seller1@test.com",
  "password": "123456"
}
```

#### 3. Lấy thông tin user hiện tại
```http
GET /api/auth/me
Authorization: Bearer {token}
```

---

### 🚴 Listings (Tin đăng)

#### 1. Tạo listing mới (SELLER)
```http
POST /api/listings
Authorization: Bearer {seller_token}
Content-Type: application/json

{
  "title": "Giant TCR Advanced Pro 2022",
  "description": "Xe đua đường trường cao cấp, khung carbon",
  "usageHistory": "Đã sử dụng 1 năm, đi 2000km",
  "locationText": "Hà Nội",
  "brandId": 1,
  "categoryId": 1,
  "sizeOptionId": 2,
  "priceAmount": 25000000,
  "conditionNote": "Tình trạng tốt, không tai nạn",
  "yearModel": 2022,
  "media": [
    {
      "type": "IMAGE",
      "url": "https://res.cloudinary.com/.../bike1.jpg",
      "sortOrder": 0
    },
    {
      "type": "VIDEO",
      "url": "https://res.cloudinary.com/.../bike_video.mp4",
      "sortOrder": 1
    }
  ]
}
```

**Lưu ý**: URL ảnh/video đã upload lên Cloudinary từ FE

#### 2. Lấy danh sách listings
```http
GET /api/listings?status=APPROVED&categoryId=1
```

**Query params**:
- `status`: DRAFT, PENDING_APPROVAL, APPROVED, VERIFIED, SOLD, ...
- `categoryId`: Filter theo category

#### 3. Lấy chi tiết 1 listing
```http
GET /api/listings/{id}
```

#### 4. Cập nhật listing (SELLER - chỉ DRAFT/REJECTED)
```http
PUT /api/listings/{id}
Authorization: Bearer {seller_token}
Content-Type: application/json

{
  "title": "Giant TCR Advanced Pro 2022 - Updated",
  "priceAmount": 24000000
}
```

#### 5. Submit listing để duyệt (SELLER)
```http
POST /api/listings/{id}/submit
Authorization: Bearer {seller_token}
```

Chuyển trạng thái từ **DRAFT** → **PENDING_APPROVAL**

#### 6. Duyệt/Từ chối listing (ADMIN)
```http
POST /api/listings/{id}/approve
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "approved": true
}
```

Hoặc từ chối:
```json
{
  "approved": false,
  "reason": "Hình ảnh không rõ ràng"
}
```

#### 7. Tạo báo cáo kiểm định (INSPECTOR)
```http
POST /api/listings/{id}/inspection
Authorization: Bearer {inspector_token}
Content-Type: application/json

{
  "summary": "Xe đạt tiêu chuẩn, khung không bị nứt",
  "checklistJson": "{\"frame\": \"good\", \"brake\": \"excellent\"}",
  "reportUrl": "https://res.cloudinary.com/.../inspection_report.pdf"
}
```

Chuyển trạng thái từ **APPROVED** → **VERIFIED**

---

### 🛒 Orders (Đơn hàng)

#### 1. Tạo order (BUYER)
```http
POST /api/orders
Authorization: Bearer {buyer_token}
Content-Type: application/json

{
  "listingId": 1,
  "depositRequired": true,
  "shippingNote": "Giao tại Hà Nội"
}
```

**Note**: 
- `depositRequired: true` → Order cần đặt cọc trước (status = DEPOSIT_PENDING)
- `depositRequired: false` → Mua ngay (status = PLACED)

#### 2. Lấy danh sách orders của mình
```http
GET /api/orders
Authorization: Bearer {token}
```

- **BUYER**: Thấy orders mình đã mua
- **SELLER**: Thấy orders người khác mua xe của mình

#### 3. Lấy chi tiết order
```http
GET /api/orders/{id}
Authorization: Bearer {token}
```

#### 4. Tạo payment (BUYER)
```http
POST /api/orders/payment
Authorization: Bearer {buyer_token}
Content-Type: application/json

{
  "orderId": 1,
  "paymentType": "DEPOSIT",
  "provider": "mock"
}
```

**PaymentType**: 
- `DEPOSIT`: Thanh toán cọc
- `FULL`: Thanh toán toàn bộ (hoặc phần còn lại)

**Response**:
```json
{
  "success": true,
  "message": "Tạo payment thành công. URL thanh toán: /mock-payment/1",
  "data": {
    "id": 1,
    "type": "DEPOSIT",
    "status": "PENDING",
    "amount": 2500000
  }
}
```

#### 5. Mock payment callback (Giả lập thanh toán thành công)
```http
POST /api/orders/payment/callback
Content-Type: application/json

{
  "paymentId": 1,
  "status": "PAID",
  "providerTxnId": "MOCK_TXN_123456"
}
```

Sau khi thanh toán:
- **DEPOSIT** paid → Order status = DEPOSIT_PAID
- **FULL** paid → Order status = CONFIRMED

#### 6. Cập nhật trạng thái order

**Seller chuyển sang SHIPPING:**
```http
PUT /api/orders/{id}/status
Authorization: Bearer {seller_token}
Content-Type: application/json

{
  "status": "SHIPPING",
  "note": "Đã gửi hàng qua vận chuyển"
}
```

**Buyer xác nhận DELIVERED:**
```http
PUT /api/orders/{id}/status
Authorization: Bearer {buyer_token}
Content-Type: application/json

{
  "status": "DELIVERED"
}
```

**Buyer xác nhận COMPLETED:**
```http
PUT /api/orders/{id}/status
Authorization: Bearer {buyer_token}
Content-Type: application/json

{
  "status": "COMPLETED"
}
```

Khi COMPLETED, listing sẽ chuyển sang **SOLD**

---

## 🔄 Luồng chính (Main Flows)

### Flow 1: Seller đăng tin → Admin duyệt → Inspector kiểm định

```
1. SELLER Register: POST /api/auth/register (role: SELLER)
2. SELLER Login: POST /api/auth/login
3. SELLER Create listing: POST /api/listings (status: DRAFT)
4. SELLER Submit: POST /api/listings/{id}/submit (status: PENDING_APPROVAL)
5. ADMIN Login
6. ADMIN Approve: POST /api/listings/{id}/approve (status: APPROVED)
7. INSPECTOR Login
8. INSPECTOR Create inspection: POST /api/listings/{id}/inspection (status: VERIFIED)
```

### Flow 2: Buyer mua xe với đặt cọc

```
1. BUYER Register & Login
2. BUYER Browse: GET /api/listings?status=VERIFIED
3. BUYER Create order: POST /api/orders (depositRequired: true)
   → Order status: DEPOSIT_PENDING
4. BUYER Create deposit payment: POST /api/orders/payment (paymentType: DEPOSIT)
5. Mock payment success: POST /api/orders/payment/callback (status: PAID)
   → Order status: DEPOSIT_PAID
6. BUYER Create full payment: POST /api/orders/payment (paymentType: FULL)
   → Amount = PriceAmount - DepositAmount
7. Mock payment success: POST /api/orders/payment/callback
   → Order status: CONFIRMED
8. SELLER Update status: PUT /api/orders/{id}/status (status: SHIPPING)
9. BUYER Confirm delivered: PUT /api/orders/{id}/status (status: DELIVERED)
10. BUYER Complete: PUT /api/orders/{id}/status (status: COMPLETED)
    → Listing status: SOLD
```

### Flow 3: Buyer mua ngay không cọc

```
1-2. Same as Flow 2
3. BUYER Create order: POST /api/orders (depositRequired: false)
   → Order status: PLACED
4. BUYER Create full payment: POST /api/orders/payment (paymentType: FULL)
5. Mock payment success: POST /api/orders/payment/callback
   → Order status: CONFIRMED
6-10. Same as Flow 2
```

---

## 🧪 Test với Swagger

1. Mở Swagger UI: http://localhost:5000/swagger
2. Tạo user với role SELLER, BUYER, ADMIN, INSPECTOR
3. Login để lấy token
4. Click nút **Authorize** ở góc phải trên, paste token vào
5. Test các API theo flow trên

---

## 📂 Cấu trúc Project

```
SWP391_CycleTrust_BE/
├── CycleTrust.sln
└── src/
    ├── CycleTrust.Core/           # Entities, Enums, Interfaces
    ├── CycleTrust.Application/    # Services, DTOs, Validators, Mappings
    ├── CycleTrust.Infrastructure/ # DbContext, Repositories
    └── CycleTrust.API/            # Controllers, Program.cs, Swagger
```

---

## ⚙️ Configuration

File `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=cycle_trust_db;User=root;Password=123456;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "CycleTrustAPI",
    "Audience": "CycleTrustClient"
  }
}
```

**Lưu ý**: Đổi JWT Key trong production!

---

## 🛠️ Troubleshooting

### Lỗi "Connection refused" đến MySQL
- Kiểm tra MySQL đang chạy: `mysql -u root -p`
- Kiểm tra port 3306 đang mở
- Kiểm tra password trong `appsettings.json`

### Lỗi "No .NET SDKs were found"
- Tải và cài .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- Restart terminal sau khi cài

### Lỗi build: Package not found
```bash
dotnet restore
dotnet build
```

---

## 📝 Notes

- **File upload**: FE upload trực tiếp lên Cloudinary, BE chỉ nhận URL string
- **Payment**: Hiện tại dùng mock, chưa tích hợp VNPay/Momo
- **Email**: Chưa implement
- **CORS**: Đã enable cho tất cả origins (cần restrict trong production)

---

## 🔒 Security

- Password được hash bằng BCrypt
- JWT token hết hạn sau 7 ngày
- Authorization theo role-based
- Validate owner cho các action update/delete

---

## 📞 Contact

- Backend: .NET 8 + MySQL
- Frontend: (Chưa có)
- Database: MySQL 8

Nếu có vấn đề, check logs console khi chạy `dotnet run`
