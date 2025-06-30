using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModel
{
    public class DangNhapRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ReturnUrl { get; set; }

        public string Captcha { get; set; }
    }
}