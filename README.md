# Hệ Thống Quản Lý Phòng Trọ

Ứng dụng quản lý phòng trọ trực tuyến, hỗ trợ chủ trọ và người thuê trong việc quản lý phòng, hợp đồng, hóa đơn và thanh toán.

## Công Nghệ Sử Dụng

### Backend
- ASP.NET Core 8.0
- Entity Framework Core 8.0.11
- SQL Server
- C# 12

### Frontend
- Razor Pages
- HTML5 / CSS3
- JavaScript
- Bootstrap 5.3
- Chart.js
- Font Awesome

### Thư Viện & Dịch Vụ
- BCrypt.Net-Next – Mã hóa mật khẩu
- MailKit – Gửi email
- VNPay, MoMo – Thanh toán trực tuyến
- Google Authentication – Đăng nhập Google
- reCAPTCHA – Bảo mật
- X.PagedList – Phân trang

## Chức Năng Chính

### Người Dùng
- Đăng ký / Đăng nhập (Email, Google)
- Quản lý thông tin cá nhân
- Tìm kiếm và lọc phòng trọ
- Xem chi tiết và đặt phòng
- Thanh toán tiền cọc (VNPay, MoMo)
- Quản lý hợp đồng và hóa đơn
- Xem lịch sử giao dịch
- Lưu phòng yêu thích
- Gửi báo cáo sự cố

### Quản Trị Viên
- Quản lý phòng trọ, loại phòng, thiết bị
- Quản lý người dùng, hợp đồng, hóa đơn
- Quản lý tin tức
- Xử lý báo cáo sự cố
- Thống kê doanh thu
- Báo cáo bằng biểu đồ (cột, tròn)

## Cấu Trúc Dự Án

```
QuanLy_PhongTro/
+-- Areas/
¦   +-- Admin/              # Khu vực quản trị
¦       +-- Controllers/    # Controllers admin
¦       +-- Views/          # Views admin
+-- Controllers/            # Controllers chính
+-- Models/                 # Các model 
+-- Repository/             # Data context
+-- Services/               # Các dịch vụ (Email, Payment)
+-- ViewModel/              # View models
+-- Views/                  # Views nguời dùng
+-- wwwroot/                # Static files
¦   +-- css/               # Style sheets
¦   +-- js/                # JavaScript files
¦   +-- asset/             # Hình ảnh, media
+-- Migrations/             # Database migrations
```

## Tác Giả
- Võ Thanh Luận
