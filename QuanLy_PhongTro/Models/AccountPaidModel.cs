using Humanizer;
using QuanLy_PhongTro.Migrations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLy_PhongTro.Models
{
    public class AccountPaidModel
    {
        [Key]
        public string TransactionId { get; set; }
        public string OrderDescription { get; set; }
        public string PaymentMethod { get; set; }
        public int MaNguoiDung { get; set; }
        public int? MaHopDong { get; set; }
        public int? MaHoaDon { get; set; }

        public double Amount { get; set; }
        public DateTime DateCreated { get; set; }

        [ForeignKey("MaNguoiDung")]
        public NguoiDungModel NguoiDung { get; set; } // Liên kết với NguoiDungModel

        [ForeignKey("MaHopDong")]
        public HopDongModel HopDong { get; set; }
        [ForeignKey("MaHoaDon")]
        public HoaDonModel HoaDon { get; set; }
    }
}
