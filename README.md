# H? Th?ng Qu?n Lý Phòng Tr?

?ng d?ng qu?n lý phòng tr? tr?c tuy?n, h? tr? ch? tr? và ngu?i thuê trong vi?c qu?n lý phòng, h?p d?ng, hóa don và thanh toán.

## Công Ngh? S? D?ng

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

### Thu Vi?n & D?ch V?
- BCrypt.Net-Next – Mã hóa m?t kh?u
- MailKit – G?i email
- VNPay, MoMo – Thanh toán tr?c tuy?n
- Google Authentication – Ðang nh?p Google
- reCAPTCHA – B?o m?t
- X.PagedList – Phân trang

## Ch?c Nang Chính

### Ngu?i Dùng
- Ðang ký / Ðang nh?p (Email, Google)
- Qu?n lý thông tin cá nhân
- Tìm ki?m và l?c phòng tr?
- Xem chi ti?t và d?t phòng
- Thanh toán ti?n c?c (VNPay, MoMo)
- Qu?n lý h?p d?ng và hóa don
- Xem l?ch s? giao d?ch
- Luu phòng yêu thích
- G?i báo cáo s? c?

### Qu?n Tr? Viên
- Qu?n lý phòng tr?, lo?i phòng, thi?t b?
- Qu?n lý ngu?i dùng, h?p d?ng, hóa don
- Qu?n lý tin t?c
- X? lý báo cáo s? c?
- Th?ng kê doanh thu
- Báo cáo b?ng bi?u d? (c?t, tròn)

## C?u Trúc D? Án

```
QuanLy_PhongTro/
+-- Areas/
¦   +-- Admin/              # Khu v?c qu?n tr?
¦       +-- Controllers/    # Controllers admin
¦       +-- Views/          # Views admin
+-- Controllers/            # Controllers chính
+-- Models/                 # Các model 
+-- Repository/             # Data context
+-- Services/               # Các d?ch v? (Email, Payment)
+-- ViewModel/              # View models
+-- Views/                  # Views ngu?i dùng
+-- wwwroot/                # Static files
¦   +-- css/               # Style sheets
¦   +-- js/                # JavaScript files
¦   +-- asset/             # Hình ?nh, media
+-- Migrations/             # Database migrations
```

## Tác Gi?
- Võ Thành Lu?n
