using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModel
{
    public class DangNhapRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "ReturnUrl")]
        public string? ReturnUrl { get; set; }

        [Display(Name = "Captcha")]
        public string? Captcha { get; set; }
    }
}