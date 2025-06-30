using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models.ViewModels
{
    public class HopDongViewModel
    {
        public int MaHopDong { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng.")]
        public int MaPhong { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn người dùng.")]
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống.")]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống.")]
        public DateTime NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Tiền cọc không được để trống.")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền cọc phải lớn hơn 0.")]
        public decimal TienCoc { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [StringLength(50, ErrorMessage = "Trạng thái không quá 50 ký tự.")]
        public string TrangThai { get; set; }
        public string PhuongThucThanhToan { get; set; }

        [Required(ErrorMessage = "Ngày ký không được để trống.")]
        public DateTime NgayKy { get; set; }

        [StringLength(2000, ErrorMessage = "Ghi chú không quá 2000 ký tự.")]
        public string GhiChu { get; set; }

        // Thuộc tính bổ sung cho người dùng mới
        [Required(ErrorMessage = "Vui lòng chọn cách nhập người dùng.")]
        public string UserInputType { get; set; } // "existing" hoặc "new"

        // Thông tin người dùng mới (không bắt buộc trong ModelState)
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự.")]
        public string NewUserHoTen { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự.")]
        public string NewUserEmail { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15, ErrorMessage = "Số điện thoại không quá 15 ký tự.")]
        public string NewUserSoDienThoai { get; set; }

        // Thuộc tính hiển thị (không bắt buộc)
        public string TenPhong { get; set; }
        public string HoTenNguoiDung { get; set; }
    }
}