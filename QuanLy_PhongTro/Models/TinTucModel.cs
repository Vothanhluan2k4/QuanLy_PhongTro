using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLy_PhongTro.Models
{
    public class TinTucModel
    {
        [Key]
        public int MaTinTuc { get ; set ; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(255, ErrorMessage = "Tiêu đề không quá 255 ký tự")]
        public string TieuDe { get; set ; } = string.Empty;

        [Required(ErrorMessage = "Đường dẫn hình ảnh không được để trống")]
        [StringLength(400, ErrorMessage = "Đường dẫn hình ảnh không quá 400 ký tự")]
        public string DuongDanHinhAnh { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [StringLength(4000, ErrorMessage = "Mô tả không quá 4000 ký tự")]
        public string MoTa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại tin tức")]
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
