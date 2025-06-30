using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLy_PhongTro.Models
{
    [Table("HoaDon")]
    public class HoaDonModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHoaDon { get; set; }

        [Required]
        [ForeignKey("HopDong")]
        public int MaHopDong { get; set; }

        public virtual HopDongModel HopDong { get; set; }

        [Required]
        public int Thang { get; set; }

        [Required]
        public int Nam { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền điện phải lớn hơn hoặc bằng 0")]
        public decimal TienDien { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền nước phải lớn hơn hoặc bằng 0")]
        public decimal TienNuoc { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền rác phải lớn hơn hoặc bằng 0")]
        public decimal TienRac { get; set; }

        [Required]
        public DateTime NgayPhatHanh { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Chưa thanh toán";

        // Tổng tiền (tính toán từ các khoản phí)
        public decimal TongTien { get; set; }

        // Liên kết với giao dịch thanh toán
        public long? MomoId { get; set; } 
        public long? VnpayId { get; set; } 

        [ForeignKey("MomoId")]
        public virtual MomoModel Momo { get; set; }

        [ForeignKey("VnpayId")]
        public virtual VnpayModel Vnpay { get; set; }
        public virtual ICollection<AccountPaidModel> AccountPaid { get; set; } = new List<AccountPaidModel>();
    }
}