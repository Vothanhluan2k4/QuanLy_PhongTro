using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;

namespace QuanLy_PhongTro.Controllers
{
    public class TinTucController : Controller
    {
        public readonly DataContext _context;

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
            // Tăng lượt xem khi truy cập chi tiết
            var tinTuc = await _context.TinTucs
                .Include(t => t.LoaiTinTuc)
                .Include(t => t.NguoiDung)
                .FirstOrDefaultAsync(t => t.MaTinTuc == id);

            if (tinTuc == null)
            {
                return NotFound();
            }

            tinTuc.LuotXem += 1; // Tăng lượt xem
            _context.TinTucs.Update(tinTuc);
            await _context.SaveChangesAsync();

            // Lấy danh sách tin tức sắp xếp theo lượt xem (top 5)
            var topViewedNews = await _context.TinTucs
                .Include(t => t.LoaiTinTuc)
                .OrderByDescending(t => t.LuotXem)
                .Take(5)
                .ToListAsync();

            ViewBag.TopViewedNews = topViewedNews;

            return View(tinTuc);
        }
    }
}