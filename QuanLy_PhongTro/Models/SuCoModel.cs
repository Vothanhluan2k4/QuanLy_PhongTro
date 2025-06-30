using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLy_PhongTro.Models
{
   
    // Enum cho mức độ ưu tiên
    public enum MucDoUuTien
    {
        KhanCap = 1,         // Khẩn cấp
        QuanTrong = 2,       // Quan trọng
        BinhThuong = 3       // Bình thường
    }

    public class SuCoModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaPhong { get; set; }
        [ForeignKey("MaPhong")]
        public PhongTroModel Phong { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }
        [ForeignKey("MaNguoiDung")]
        public NguoiDungModel NguoiBaoCao { get; set; }

        [Required]
        public string LoaiSuCo { get; set; }  

        [Required]
        [StringLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự")]
        public string MoTa { get; set; }  

        [StringLength(255)]
        public string MediaUrl { get; set; }  

        [Required]
        public MucDoUuTien MucDo { get; set; } = MucDoUuTien.BinhThuong;  

        [StringLength(15)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string DienThoai { get; set; }  // Thêm validation số điện thoại

        [Required]
        public string TrangThai { get; set; } = "Chưa xử lý";  

        public DateTime NgayTao { get; set; } = DateTime.Now;  
    }
}