using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models
{
    public class LoaiTinTucModel
    {
        [Key]
        public int MaLoaiTinTuc { get; set ; }

        [Required]
        [StringLength(100)]
        public string TenLoaiTinTuc { get; set; }

        public ICollection<TinTucModel> TinTucs { get; set; }

    }
}
