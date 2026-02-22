using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.Models.Vnpay
{
    public class PaymentInformationModel
    {
        [Required(ErrorMessage = "Loại đơn hàng không được để trống")]
        [StringLength(50)]
        public string OrderType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số tiền không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public double Amount { get; set; }

        [StringLength(500)]
        public string? OrderDescription { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
