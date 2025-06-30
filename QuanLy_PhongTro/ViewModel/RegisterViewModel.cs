using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModels // Hoặc namespace phù hợp
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ và Tên là bắt buộc.")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số Điện Thoại là bắt buộc.")]
        [RegularExpression(@"^(0[1-9]\d{8})$", ErrorMessage = "Số điện thoại phải có 10 chữ số và bắt đầu bằng 0.")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp.")] // Quan trọng!
        public string ConfirmPassword { get; set; } = string.Empty;

        public int MaQuyen { get; set; } // Lấy từ hidden field
    }
}