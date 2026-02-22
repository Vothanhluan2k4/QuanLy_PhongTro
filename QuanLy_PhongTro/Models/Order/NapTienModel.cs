using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models.Order
{
    public class NapTienModel
    {
        [Required]
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Số tiền không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền nạp phải lớn hơn 0")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Phương thức thanh toán không được để trống")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(100)]
        public string OrderId { get; set; } = string.Empty;

        [StringLength(255)]
        public string? OrderInfo { get; set; }

        [StringLength(500)]
        public string? OrderDescription { get; set; }

        [StringLength(50)]
        public string? OrderType { get; set; }
    }
}
