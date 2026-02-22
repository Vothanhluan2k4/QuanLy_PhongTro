using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace QuanLy_PhongTro.Models
{
    [Table("PhanQuyens")]
    public class PhanQuyenModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaQuyen { get; set; }

        [Required(ErrorMessage = "Tên quyền không được để trống")]
        [StringLength(50, ErrorMessage = "Tên quyền không quá 50 ký tự")]
        public string TenQuyen { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả không quá 255 ký tự")]
        public string? MoTa { get; set; }

        // Navigation property: Một quyền có thể được gán cho nhiều người dùng
        public List<NguoiDungModel> NguoiDungs { get; set; } = new List<NguoiDungModel>();
    }
}