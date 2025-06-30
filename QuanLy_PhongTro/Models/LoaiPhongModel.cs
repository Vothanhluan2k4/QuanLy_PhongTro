using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models
{
    [Table("LoaiPhong")]
    public class LoaiPhongModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLoaiPhong { get; set; }

        [Required(ErrorMessage = "Tên loại phòng không được để trống")]
        [StringLength(100, ErrorMessage = "Tên loại phòng không quá 100 ký tự")]
        public string TenLoaiPhong { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự")]
        public string MoTa { get; set; }

        // Navigation property
        public virtual ICollection<PhongTroModel>? PhongTros { get; set; }
    }
}
