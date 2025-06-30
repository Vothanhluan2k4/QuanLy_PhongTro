using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models
{
    public class LuuPhongModel
    {
        public int MaLuuPhong { get; set; }
        public int MaPhong { get; set; }
        public int MaNguoiDung { get; set; }
        public DateTime NgayLuu { get; set; } = DateTime.Now;

        // Navigation property: Liên kết với PhongTro
        public PhongTroModel PhongTro { get; set; } = null!;

        // Navigation property: Liên kết với NguoiDung
        public NguoiDungModel NguoiDung { get; set; } = null!;
    }
}