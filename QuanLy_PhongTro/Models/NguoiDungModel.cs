using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models
{
    public class NguoiDungModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không quá 15 ký tự")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Đường dẫn avatar không quá 500 ký tự")]
        public string? AvatarUrl { get; set; }

        [Required]
        [ForeignKey("PhanQuyen")]
        public int MaQuyen { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số dư không được âm")]
        public decimal SoDu { get; set; } = 0m;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; }
        // Navigation property: Liên kết với PhanQuyen
        public PhanQuyenModel PhanQuyen { get; set; }
        

        // Navigation property: Một người dùng có thể lưu nhiều phòng
        public List<LuuPhongModel> LuuPhongs { get; set; } = new List<LuuPhongModel>();
        public List<HopDongModel> HopDongs { get; set; } = new List<HopDongModel>();
        public virtual ICollection<AccountPaidModel> AccountPaid { get; set; } = new List<AccountPaidModel>();
    }
}