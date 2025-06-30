using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models
{
    [Table("ChiSoDienNuoc")]
    public class ChiSoDienNuocModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaChiSo { get; set; }

        [Required]
        [ForeignKey("HopDong")]
        public int MaHopDong { get; set; }

        public virtual HopDongModel HopDong { get; set; }

        [Required]
        public int Thang { get; set; }

        [Required]
        public int Nam { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Chỉ số điện cũ phải lớn hơn hoặc bằng 0")]
        public decimal ChiSoDienCu { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Chỉ số điện mới phải lớn hơn hoặc bằng chỉ số điện cũ")]
        public decimal ChiSoDienMoi { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Chỉ số nước cũ phải lớn hơn hoặc bằng 0")]
        public decimal ChiSoNuocCu { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Chỉ số nước mới phải lớn hơn hoặc bằng chỉ số nước cũ")]
        public decimal ChiSoNuocMoi { get; set; }

        [Required]
        public DateTime NgayGhi { get; set; } = DateTime.Now;
    }
    
}
