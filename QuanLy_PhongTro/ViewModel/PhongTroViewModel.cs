namespace QuanLy_PhongTro.ViewModel
{
    public class PhongTroViewModel
    {
        public string TenPhong { get; set; }
        public int MaLoaiPhong { get; set; }
        public DiaChiViewModel DiaChi { get; set; } // Đối tượng địa chỉ
        public float DienTich { get; set; }
        public decimal GiaThue { get; set; }
        public decimal TienDien { get; set; }
        public decimal TienNuoc { get; set; }
        public string TrangThai { get; set; }
        public string MoTa { get; set; }
        public List<ThietBiViewModel> ThietBiData { get; set; } // Danh sách thiết bị
        public List<AnhViewModel> AnhData { get; set; } // Danh sách ảnh
    }

    public class DiaChiViewModel
    {
        public string Province { get; set; }
        public string District { get; set; }
        public string HouseNumber { get; set; }
    }

    public class ThietBiViewModel
    {
        public int ThietBiId { get; set; }
        public string ThietBiName { get; set; }
        public int SoLuong { get; set; }
    }

    public class AnhViewModel
    {
        public string Name { get; set; }
        public bool IsDaiDien { get; set; }
        public bool IsExisting { get; set; }
    }
}
