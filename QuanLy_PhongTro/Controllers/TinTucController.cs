using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;

namespace QuanLy_PhongTro.Controllers
{
    public class TinTucController : Controller
    {
        private readonly DataContext _context;
        private const int VIEW_COUNT_INTERVAL_MINUTES = 1; // Thời gian tối thiểu giữa các lượt xem

        public TinTucController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string tab = "TinNoiBat", string search = "")
        {
            ViewBag.ActiveTab = tab;

            // Lấy danh sách loại tin tức
            var loaiTinTucs = await _context.LoaiTinTucs.ToListAsync();
            ViewBag.LoaiTinTucs = loaiTinTucs;

            // Xây dựng query cơ bản
            IQueryable<TinTucModel> query = _context.TinTucs
                .Include(t => t.LoaiTinTuc)
                .Include(t => t.NguoiDung);

            // Áp dụng tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.TieuDe.Contains(search) || t.MoTa.Contains(search));
            }

            // Lọc theo tab
            switch (tab)
            {
                case "CamNang":
                    query = query.Where(t => t.LoaiTinTuc.TenLoaiTinTuc == "Cẩm nang");
                    break;
                case "HuongDan":
                    query = query.Where(t => t.LoaiTinTuc.TenLoaiTinTuc == "Hướng dẫn");
                    break;
                case "TinNoiBat":
                    query = query.OrderByDescending(t => t.NgayDang);
                    break;
            }

            var tinTucs = await query.ToListAsync();

            // Phân chia dữ liệu
            ViewBag.MainNews = tinTucs.FirstOrDefault();
            ViewBag.SidebarNews = tinTucs.Skip(1).Take(4).ToList();
            ViewBag.SecondaryNews = tinTucs.Skip(5).Take(2).ToList();

            // Gửi thông báo nếu không có kết quả tìm kiếm
            if (!string.IsNullOrEmpty(search) && !tinTucs.Any())
            {
                ViewBag.Message = $"Không tìm thấy kết quả cho '{search}'.";
            }

            return View();
        }

        public async Task<IActionResult> Detail(int id)
        {
            var tinTuc = await _context.TinTucs
                .Include(t => t.LoaiTinTuc)
                .Include(t => t.NguoiDung)
                .FirstOrDefaultAsync(t => t.MaTinTuc == id);

            if (tinTuc == null)
            {
                return NotFound();
            }

            // KHÔNG tự động tăng lượt xem ở đây
            // Chỉ tăng qua JavaScript sau 1 phút đọc main-content

            // Lấy danh sách tin tức cho sidebar (chỉ hiển thị)
            var topViewedNews = await _context.TinTucs
                .Include(t => t.LoaiTinTuc)
                .Where(t => t.MaTinTuc != id) // Loại bỏ tin hiện tại
                .OrderByDescending(t => t.LuotXem)
                .Take(5)
                .ToListAsync();

            ViewBag.TopViewedNews = topViewedNews;

            return View(tinTuc);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateView([FromBody] int id)
        {
            var tinTuc = await _context.TinTucs.FindAsync(id);
            if (tinTuc == null)
            {
                return Json(new { success = false, message = "Tin tức không tồn tại." });
            }

            // Kiểm tra thời gian lần xem cuối để tránh spam
            string lastViewKey = $"LastView_{id}";
            var lastViewTime = Request.Cookies[lastViewKey];
            DateTime lastAccessTime = DateTime.MinValue;

            if (!string.IsNullOrEmpty(lastViewTime))
            {
                DateTime.TryParse(lastViewTime, out lastAccessTime);
            }

            DateTime currentTime = DateTime.Now;

            // Chỉ tăng lượt xem nếu:
            // 1. Chưa từng xem (lastAccessTime == MinValue)
            // 2. Hoặc đã qua đủ thời gian interval
            if (lastAccessTime == DateTime.MinValue ||
                (currentTime - lastAccessTime).TotalMinutes >= VIEW_COUNT_INTERVAL_MINUTES)
            {
                tinTuc.LuotXem += 1;
                _context.TinTucs.Update(tinTuc);
                await _context.SaveChangesAsync();

                // Lưu thời gian xem cuối
                var cookieOptions = new CookieOptions
                {
                    Expires = currentTime.AddHours(24),
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                };
                Response.Cookies.Append(lastViewKey, currentTime.ToString("o"), cookieOptions);

                return Json(new { success = true, luotXem = tinTuc.LuotXem });
            }

            return Json(new { success = false, message = "Chưa đủ thời gian để tính lượt xem mới." });
        }

        // Action để reset view tracking (dùng cho debug)
        [HttpPost]
        public IActionResult ResetViewTracking()
        {
            // Xóa tất cả cookies related đến view tracking
            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie.StartsWith("LastView_"))
                {
                    Response.Cookies.Delete(cookie);
                }
            }

            return Json(new { success = true, message = "Đã reset view tracking." });
        }
    }
}