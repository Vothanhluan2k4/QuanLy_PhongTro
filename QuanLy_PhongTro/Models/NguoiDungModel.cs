using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models
{
    public class NguoiDungModel
    {
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int MaQuyen { get; set; }

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