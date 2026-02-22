using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModel
{
    public class HopDongDto
    {
        public int MaHopDong { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        public int MaPhong { get; set; }
        public int? MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Tiền cọc không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền cọc phải lớn hơn hoặc bằng 0")]
        public decimal TienCoc { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày ký không được để trống")]
        public DateTime NgayKy { get; set; }

        [StringLength(2000, ErrorMessage = "Ghi chú không quá 2000 ký tự")]
        public string? GhiChu { get; set; }
        public NewUserDto? NewUser { get; set; }
    }

    public class NewUserDto
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15)]
        public string SoDienThoai { get; set; } = string.Empty;
    }
}