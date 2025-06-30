using Microsoft.AspNetCore.Mvc;
using QuanLy_PhongTro.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Repository;

namespace QuanLy_PhongTro.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TinTucController : Controller
    {
        private readonly DataContext _context;

        public TinTucController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tinTucs = await _context.TinTucs
                .Include(t => t.LoaiTinTuc)
                .Include(t => t.NguoiDung)
                .Select(t => new
                {
                    t.MaTinTuc,
                    t.TieuDe,
                    t.DuongDanHinhAnh,
                    t.MoTa,
                    LoaiTinTuc = t.LoaiTinTuc.TenLoaiTinTuc,
                    NguoiDung = t.NguoiDung.HoTen,
                    t.MaLoaiTinTuc, // Thêm để sử dụng khi edit
                    t.MaNguoiDung,  // Thêm để sử dụng khi edit
                    t.LuotXem,
                    NgayDang = t.NgayDang.ToString("dd/MM/yyyy HH:mm:ss")
                })
                .ToListAsync();
            return Json(tinTucs);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var tinTuc = await _context.TinTucs
                .Include(t => t.LoaiTinTuc)
                .Include(t => t.NguoiDung)
                .FirstOrDefaultAsync(t => t.MaTinTuc == id);
            if (tinTuc == null)
            {
                return NotFound();
            }
            return Json(new
            {
                tinTuc.MaTinTuc,
                tinTuc.TieuDe,
                tinTuc.DuongDanHinhAnh,
                tinTuc.MoTa,
                tinTuc.MaLoaiTinTuc,
                tinTuc.MaNguoiDung,
                tinTuc.LuotXem,
                tinTuc.NgayDang
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetLoaiTinTucs()
        {
            var loaiTinTucs = await _context.LoaiTinTucs
                .Select(l => new { l.MaLoaiTinTuc, l.TenLoaiTinTuc })
                .ToListAsync();
            return Json(loaiTinTucs);
        }

        [HttpPost]
        public async Task<IActionResult> Add(IFormFile imageFile, string TieuDe, string MoTa, int MaLoaiTinTuc, int MaNguoiDung)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(TieuDe))
                {
                    return BadRequest("Tiêu đề không được để trống");
                }
                if (string.IsNullOrWhiteSpace(MoTa))
                {
                    return BadRequest("Mô tả không được để trống");
                }
                if (MaLoaiTinTuc <= 0)
                {
                    return BadRequest("Vui lòng chọn loại tin tức");
                }
                if (MaNguoiDung <= 0)
                {
                    return BadRequest("Vui lòng chọn người dùng");
                }

                // Create new TinTuc object
                var tinTuc = new TinTucModel
                {
                    TieuDe = TieuDe.Trim(),
                    MoTa = MoTa.Trim(),
                    MaLoaiTinTuc = MaLoaiTinTuc,
                    MaNguoiDung = MaNguoiDung,
                    NgayDang = DateTime.Now,
                    LuotXem = 0
                };

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Validate image file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest("Chỉ chấp nhận file ảnh định dạng: " + string.Join(", ", allowedExtensions));
                    }

                    if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                    {
                        return BadRequest("Kích thước file ảnh không được vượt quá 5MB");
                    }

                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/asset/news");

                    // Create directory if not exists
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var filePath = Path.Combine(uploadFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    tinTuc.DuongDanHinhAnh = "news/" + fileName; // Store relative path
                }
                else
                {
                    return BadRequest("Vui lòng chọn hình ảnh cho tin tức");
                }

                _context.TinTucs.Add(tinTuc);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Thêm tin tức thành công", id = tinTuc.MaTinTuc });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Add: {ex.Message}");
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, IFormFile imageFile, string TieuDe, string MoTa, int MaLoaiTinTuc, int MaNguoiDung)
        {
            try
            {
                var tinTuc = await _context.TinTucs.FindAsync(id);
                if (tinTuc == null)
                {
                    return NotFound("Không tìm thấy tin tức");
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(TieuDe))
                {
                    return BadRequest("Tiêu đề không được để trống");
                }
                if (string.IsNullOrWhiteSpace(MoTa))
                {
                    return BadRequest("Mô tả không được để trống");
                }
                if (MaLoaiTinTuc <= 0)
                {
                    return BadRequest("Vui lòng chọn loại tin tức");
                }
                if (MaNguoiDung <= 0)
                {
                    return BadRequest("Vui lòng chọn người dùng");
                }

                // Update fields
                tinTuc.TieuDe = TieuDe.Trim();
                tinTuc.MoTa = MoTa.Trim();
                tinTuc.MaLoaiTinTuc = MaLoaiTinTuc;
                tinTuc.MaNguoiDung = MaNguoiDung;

                // Handle image update
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Validate image file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest("Chỉ chấp nhận file ảnh định dạng: " + string.Join(", ", allowedExtensions));
                    }

                    if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                    {
                        return BadRequest("Kích thước file ảnh không được vượt quá 5MB");
                    }

                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/asset/news");

                    // Create directory if not exists
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var filePath = Path.Combine(uploadFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(tinTuc.DuongDanHinhAnh))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/asset", tinTuc.DuongDanHinhAnh);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    tinTuc.DuongDanHinhAnh = "news/" + fileName;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật tin tức thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Update: {ex.Message}");
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tinTuc = await _context.TinTucs.FindAsync(id);
                if (tinTuc == null)
                {
                    return NotFound("Không tìm thấy tin tức");
                }

                // Delete image file if exists
                if (!string.IsNullOrEmpty(tinTuc.DuongDanHinhAnh))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/asset", tinTuc.DuongDanHinhAnh);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.TinTucs.Remove(tinTuc);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Xóa tin tức thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Delete: {ex.Message}");
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }
    }
}