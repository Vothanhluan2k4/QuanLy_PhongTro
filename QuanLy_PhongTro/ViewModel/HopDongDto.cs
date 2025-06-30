using System.ComponentModel.DataAnnotations;

public class HopDongDto
{
    public int MaHopDong { get; set; }
    [Required]
    public int MaPhong { get; set; }
    public int? MaNguoiDung { get; set; }
    [Required]
    public DateTime NgayBatDau { get; set; }
    [Required]
    public DateTime NgayKetThuc { get; set; }
    [Required]
    [Range(0, double.MaxValue)]
    public decimal TienCoc { get; set; }
    [Required]
    public string TrangThai { get; set; }
    [Required]
    public DateTime NgayKy { get; set; }
    public string? GhiChu { get; set; }
    public NewUserDto? NewUser { get; set; }
}

public class NewUserDto
{
    [Required]
    public string HoTen { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string SoDienThoai { get; set; }
}