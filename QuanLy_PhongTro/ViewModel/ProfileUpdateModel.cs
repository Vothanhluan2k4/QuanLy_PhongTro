using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModel
{
    public class ProfileUpdateModel
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
