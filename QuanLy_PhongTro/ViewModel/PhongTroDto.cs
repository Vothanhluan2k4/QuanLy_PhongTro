public class PhongTroDto
{
    public int MaPhong { get; set; }
    public string TenPhong { get; set; }
    public int MaLoaiPhong { get; set; }
    public string TenLoaiPhong { get; set; }
    public string DiaChi { get; set; }
    public float DienTich { get; set; }
    public decimal GiaThue { get; set; }
    public decimal TienDien { get; set; }
    public decimal TienNuoc { get; set; }
    public string TrangThai { get; set; }
    public string MoTa { get; set; }
    public List<PhongTroThietBiDto> PhongTroThietBis { get; set; }
    public List<AnhPhongTroDto> AnhPhongTros { get; set; }
}

public class PhongTroThietBiDto
{
    public int MaThietBi { get; set; }
    public string TenThietBi { get; set; }
    public int SoLuong { get; set; }
}

public class AnhPhongTroDto
{
    public string DuongDan { get; set; }
    public bool LaAnhDaiDien { get; set; }
}