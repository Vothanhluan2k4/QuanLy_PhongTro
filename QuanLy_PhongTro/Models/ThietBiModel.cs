using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models
{
    [Table("ThietBi")]
    public class ThietBiModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaThietBi { get; set; }

        [Required(ErrorMessage = "Tên thiết bị không được để trống")]
        [StringLength(100, ErrorMessage = "Tên thiết bị không quá 100 ký tự")]
        public string TenThietBi { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự")]
        public string MoTa { get; set; }

        [StringLength(100, ErrorMessage = "Tình trạng không quá 100 ký tự")]
        public string TinhTrang { get; set; }

        [StringLength(20, ErrorMessage = "Đơn vị tính không quá 20 ký tự")]
        public string DonViTinh { get; set; }

        // Navigation property
        public virtual ICollection<PhongTro_ThietBiModel>? PhongTroThietBis { get; set; }
    }
}
