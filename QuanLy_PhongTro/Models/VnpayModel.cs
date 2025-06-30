using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLy_PhongTro.Models
{
    public class VnpayModel
    {
        [Key]
        public long VnpayId { get; set; }

        public string OrderDescription { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentId { get; set; }
        public int CustomerId { get; set; }
        public double Amount { get; set; }
        public DateTime DateCreated { get; set; }
        

        // Liên kết với HopDong (nullable)
        public int? MaHopDong { get; set; }
        [ForeignKey("MaHopDong")]
        public virtual HopDongModel HopDong { get; set; }
    }
}
