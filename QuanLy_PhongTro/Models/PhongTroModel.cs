using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLy_PhongTro.Models
{
    [Table("PhongTro")]
    public class PhongTroModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaPhong { get; set; }

        [Required(ErrorMessage = "Tên phòng không được để trống")]
        [StringLength(50, ErrorMessage = "Tên phòng không quá 50 ký tự")]
        public string TenPhong { get; set; }

        [Required]
        [ForeignKey("LoaiPhong")]
        public int MaLoaiPhong { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(255, ErrorMessage = "Địa chỉ không quá 255 ký tự")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Diện tích không được để trống")]
        [Range(0, float.MaxValue, ErrorMessage = "Diện tích phải lớn hơn 0")]
        public float DienTich { get; set; }

        [Required(ErrorMessage = "Giá thuê không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0")]
        public decimal GiaThue { get; set; }
        public decimal TienDien { get; set; } 
        public decimal TienNuoc { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Còn trống";

        [StringLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự")]
        public string MoTa { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual LoaiPhongModel? LoaiPhong { get; set; } 
        public virtual ICollection<AnhPhongTroModel> ?AnhPhongTros { get; set; } = new List<AnhPhongTroModel>();
        public virtual ICollection<PhongTro_ThietBiModel> PhongTroThietBis { get; set; } = new List<PhongTro_ThietBiModel>();

        public List<LuuPhongModel> LuuPhongs { get; set; } = new List<LuuPhongModel>();

        public virtual ICollection<HopDongModel>? HopDongs { get; set; } = new List<HopDongModel>();

        [NotMapped]
        public List<PhongTroModel> RelatedRooms { get; set; } = new List<PhongTroModel>();


    }
}
