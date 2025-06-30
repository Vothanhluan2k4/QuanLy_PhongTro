using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models
{
    [Table("HopDong")]
    public class HopDongModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHopDong { get; set; }

        [Required]
        [ForeignKey("PhongTro")]
        public int MaPhong { get; set; }

        [Required]
        [ForeignKey("NguoiDung")]
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Tiền cọc không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền cọc phải lớn hơn 0")]
        public decimal TienCoc { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Đang hiệu lực";

        public DateTime NgayKy { get; set; } = DateTime.Now;

        [StringLength(2000, ErrorMessage = "Ghi chú không quá 2000 ký tự")]
        public string GhiChu { get; set; }

        public string PhuongThucThanhToan { get; set; }

        // Navigation properties
        public virtual PhongTroModel? PhongTro { get; set; }
        public virtual NguoiDungModel? NguoiDung { get; set; }

        
        // Validation method
        public bool IsValid()
        {
            return NgayKetThuc > NgayBatDau;
        }

        public virtual ICollection<HoaDonModel> HoaDons { get; set; } = new List<HoaDonModel>();
        public virtual ICollection<ChiSoDienNuocModel> ChiSoDienNuocs { get; set; } = new List<ChiSoDienNuocModel>();
        public virtual ICollection<MomoModel> MomoPayments { get; set; } = new List<MomoModel>();
        public virtual ICollection<VnpayModel> VnpayPayments { get; set; } = new List<VnpayModel>();
        public virtual ICollection<AccountPaidModel> AccountPaid { get; set; } = new List<AccountPaidModel>();

    }
}
