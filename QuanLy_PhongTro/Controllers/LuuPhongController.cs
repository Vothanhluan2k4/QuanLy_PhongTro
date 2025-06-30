using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using System.Security.Claims;

namespace QuanLy_PhongTro.Controllers
{
    [Route("luu-phong")]
    public class LuuPhongController : Controller
    {
        private readonly DataContext _context;

        public LuuPhongController(DataContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng");
            }
            return userId;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                int maNguoiDung = GetCurrentUserId();

                // Lấy danh sách phòng ĐÃ LƯU của người dùng
                var rooms = await _context.LuuPhongs
                    .Where(lp => lp.MaNguoiDung == maNguoiDung)
                    .Include(lp => lp.PhongTro)
                        .ThenInclude(p => p.AnhPhongTros)
                    .Include(lp => lp.PhongTro)
                        .ThenInclude(p => p.PhongTroThietBis)
                            .ThenInclude(pt => pt.ThietBi)
                    .Include(lp => lp.PhongTro.LoaiPhong)
                    .Select(lp => lp.PhongTro)
                    .ToListAsync();

                // Nhóm theo loại phòng
                var grouped = rooms.GroupBy(r => r.LoaiPhong ?? new LoaiPhongModel { TenLoaiPhong = "Không xác định" });

                return View("~/Views/PhongTro/LuuPhong.cshtml", grouped);
            }
            catch (UnauthorizedAccessException)
            {
                return View("~/Views/PhongTro/LuuPhong.cshtml", Enumerable.Empty<IGrouping<LoaiPhongModel, PhongTroModel>>());
            }
        }

      

        [HttpPost("delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                int maNguoiDung = GetCurrentUserId();

                var luuPhong = _context.LuuPhongs
                    .FirstOrDefault(lp => lp.MaPhong == id && lp.MaNguoiDung == maNguoiDung);

                if (luuPhong == null)
                {
                    return Json(new { success = false, message = "Phòng không tồn tại trong danh sách đã lưu." });
                }

                _context.LuuPhongs.Remove(luuPhong);
                _context.SaveChanges();

                return Json(new { success = true, message = "Đã xóa phòng khỏi danh sách đã lưu." });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}