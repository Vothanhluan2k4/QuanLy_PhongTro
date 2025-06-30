using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models
{
    [Table("PhongTro_ThietBi")]
    public class PhongTro_ThietBiModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhongThietBi { get; set; }

        [Required]
        [ForeignKey("PhongTro")]
        public int MaPhong { get; set; }

        [Required]
        [ForeignKey("ThietBi")]
        public int MaThietBi { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; } = 1;

        // Navigation properties
        public virtual PhongTroModel? PhongTro { get; set; }
        public virtual ThietBiModel? ThietBi { get; set; }
    }
}
