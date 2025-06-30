using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace QuanLy_PhongTro.Models
{
    public class PhanQuyenModel
    {
        public int MaQuyen { get; set; }
        public string TenQuyen { get; set; } = string.Empty;
        public string? MoTa { get; set; }

        // Navigation property: Một quyền có thể được gán cho nhiều người dùng
        public List<NguoiDungModel> NguoiDungs { get; set; } = new List<NguoiDungModel>();
    }
}