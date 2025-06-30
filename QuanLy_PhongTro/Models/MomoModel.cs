using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLy_PhongTro.Models
{
    public class MomoModel
    {
        [Key]
        public long MomoId { get; set; }
        public string OrderInfo { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
       
        public string PaymentMethod { get; set; }

        public int CustomerId { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        

        // Liên kết với HopDong (nullable)
        public int? MaHopDong { get; set; }
        [ForeignKey("MaHopDong")]
        public virtual HopDongModel HopDong { get; set; }
    }
}
