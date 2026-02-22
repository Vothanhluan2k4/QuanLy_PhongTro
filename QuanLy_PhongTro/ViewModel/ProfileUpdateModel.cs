using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModel
{
    public class ProfileUpdateModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không quá 15 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;
    }
}
