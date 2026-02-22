using System.ComponentModel.DataAnnotations;

namespace QuanLy_PhongTro.ViewModel
{
    public class PhongTroDto
    {
        public int MaPhong { get; set; }

        [Required(ErrorMessage = "Tên phòng không được để trống")]
        [StringLength(50, ErrorMessage = "Tên phòng không quá 50 ký tự")]
        public string TenPhong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại phòng")]
        public int MaLoaiPhong { get; set; }
        public string? TenLoaiPhong { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(255, ErrorMessage = "Địa chỉ không quá 255 ký tự")]
        public string DiaChi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Diện tích không được để trống")]
        [Range(0.1f, float.MaxValue, ErrorMessage = "Diện tích phải lớn hơn 0")]
        public float DienTich { get; set; }

        [Required(ErrorMessage = "Giá thuê không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn hoặc bằng 0")]
        public decimal GiaThue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tiền điện không được âm")]
        public decimal TienDien { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tiền nước không được âm")]
        public decimal TienNuoc { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [StringLength(50)]
        public string TrangThai { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự")]
        public string? MoTa { get; set; }

        public List<PhongTroThietBiDto>? PhongTroThietBis { get; set; }
        public List<AnhPhongTroDto>? AnhPhongTros { get; set; }
    }

    public class PhongTroThietBiDto
    {
        public int MaThietBi { get; set; }
        public string? TenThietBi { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }
    }

    public class AnhPhongTroDto
    {
        [Required(ErrorMessage = "Đường dẫn ảnh không được để trống")]
        [StringLength(255, ErrorMessage = "Đường dẫn ảnh không quá 255 ký tự")]
        public string DuongDan { get; set; } = string.Empty;
        public bool LaAnhDaiDien { get; set; }
    }
}