using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLy_PhongTro.Models
{
    public class TinTucModel
    {
        [Key]
        public int MaTinTuc { get ; set ; }

        [Required]
        [StringLength(255)]
        public string TieuDe { get; set ; }

        [Required]
        [StringLength(400)]
        public string DuongDanHinhAnh { get; set;  }

        [Required]
        public string MoTa { get; set; }

        [Required]
        public int MaLoaiTinTuc { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        [ForeignKey("MaLoaiTinTuc")]
        public LoaiTinTucModel LoaiTinTuc { get; set; }

        [ForeignKey("MaNguoiDung")]
        public NguoiDungModel NguoiDung { get; set; }

        public int LuotXem { get; set; } = 0;

        public DateTime NgayDang { get; set; } = DateTime.Now;

        
    }
}
