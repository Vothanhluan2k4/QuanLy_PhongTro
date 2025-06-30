using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models
{
    public class AnhPhongTroModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaAnh { get; set; }

        [Required]
        [ForeignKey("PhongTro")]
        public int MaPhong { get; set; }

        [Required(ErrorMessage = "Đường dẫn ảnh không được để trống")]
        [StringLength(255, ErrorMessage = "Đường dẫn ảnh không quá 255 ký tự")]
        public string DuongDan { get; set; }

        public bool LaAnhDaiDien { get; set; } = false;

        public PhongTroModel PhongTro { get; set; }
    }
}
