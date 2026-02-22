using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModel
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string newPassword { get; set; } = string.Empty;
    }
}
