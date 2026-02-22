using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModel
{
    public class HoaDonInputDto
    {
        public int MaHoaDon { get; set; }
        [Required]
        public int MaHopDong { get; set; }

        [Required(ErrorMessage = "Tháng không được để trống")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Thang { get; set; }

        [Required(ErrorMessage = "Năm không được để trống")]
        [Range(2000, 2100, ErrorMessage = "Năm không hợp lệ")]
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

        public decimal TongTien {  get; set; }  

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; }

        // Thông tin chỉ số điện nước
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Chỉ số điện cũ phải lớn hơn hoặc bằng 0")]
        public decimal CSDienCu { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Chỉ số điện mới phải lớn hơn hoặc bằng chỉ số điện cũ")]
        public decimal CSDienMoi { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Chỉ số nước cũ phải lớn hơn hoặc bằng 0")]
        public decimal CSNuocCu { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Chỉ số nước mới phải lớn hơn hoặc bằng chỉ số nước cũ")]
        public decimal CSNuocMoi { get; set; }
    }
}
