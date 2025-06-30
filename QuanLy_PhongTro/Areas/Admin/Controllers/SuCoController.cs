using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.ViewModel;

namespace QuanLy_PhongTro.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SuCoController : Controller
    {
        private readonly DataContext _context;

        public SuCoController(DataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IActionResult Index(string sortOrder = "latest", string statusFilter = "all")
        {
            ViewData["SortOrder"] = sortOrder;
            ViewData["StatusFilter"] = statusFilter;

            try
            {
                var model = new PhongTroNguoiDungViewModel
                {
                    SuCos = new List<SuCoModel>() // Initial empty list for first load
                };

                return View("~/Areas/Admin/Views/PhongTro/index.cshtml", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading SuCo data: {ex.Message}");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải dữ liệu: " + ex.Message;

                var emptyModel = new PhongTroNguoiDungViewModel
                {
                    SuCos = new List<SuCoModel>()
                };

                return View("~/Areas/Admin/Views/PhongTro/index.cshtml", emptyModel);
            }
        }

        [HttpGet]
        public IActionResult GetAll(string sortOrder = "latest", string statusFilter = "all", string searchQuery = "")
        {
            try
            {
                IQueryable<SuCoModel> query = _context.SuCos
                    .Include(s => s.NguoiBaoCao)
                    .Include(s => s.Phong)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                    query = query.Where(s =>
                        (s.Phong != null && s.Phong.TenPhong.ToLower().Contains(searchQuery)) || // Chỉ tìm theo TenPhong
                        s.LoaiSuCo.ToLower().Contains(searchQuery) ||
                        s.MoTa.ToLower().Contains(searchQuery) ||
                        (s.NguoiBaoCao != null && s.NguoiBaoCao.HoTen.ToLower().Contains(searchQuery))
                    );
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
                {
                    query = query.Where(s => s.TrangThai == statusFilter);
                }

                // Apply sorting
                if (sortOrder == "latest")
                {
                    query = query.OrderByDescending(s => s.NgayTao);
                }
                else if (sortOrder == "earliest")
                {
                    query = query.OrderBy(s => s.NgayTao);
                }
                else
                {
                    query = query.OrderByDescending(s => s.NgayTao);
                }

                var suCos = query.ToList();
                var result = suCos.Select(s => new
                {
                    s.Id,
                    TenPhong = s.Phong?.TenPhong ?? "N/A",
                    HoTen = s.NguoiBaoCao?.HoTen ?? "N/A",
                    s.LoaiSuCo,
                    s.MoTa,
                    MucDo = s.MucDo.ToString(),
                    s.TrangThai,
                    s.NgayTao,
                    s.MaPhong 
                }).ToList();
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetMedia(int id)
        {
            try
            {
                var suCo = _context.SuCos.Find(id);
                if (suCo?.MediaUrl != null && !string.IsNullOrEmpty(suCo.MediaUrl))
                {
                    var mediaUrls = suCo.MediaUrl.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    return Json(new { success = true, mediaUrls = mediaUrls });
                }
                return Json(new { success = false, message = "Không tìm thấy media" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Update([FromBody] SuCoModel model)
        {
            try
            {
                var suCo = _context.SuCos.Find(model.Id);
                if (suCo == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sự cố" });
                }

                suCo.TrangThai = model.TrangThai;
                _context.SuCos.Update(suCo);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult TestData()
        {
            try
            {
                var count = _context.SuCos.Count();
                var allSuCos = _context.SuCos
                    .Include(s => s.NguoiBaoCao)
                    .Include(s => s.Phong)
                    .ToList();

                return Json(new
                {
                    success = true,
                    totalCount = count,
                    data = allSuCos.Select(s => new
                    {
                        s.Id,
                        s.MaPhong,
                        TenPhong = s.Phong?.TenPhong ?? "N/A",
                        NguoiBaoCao = s.NguoiBaoCao?.HoTen ?? "N/A",
                        s.LoaiSuCo,
                        s.MoTa,
                        MucDo = s.MucDo.ToString(),
                        s.TrangThai,
                        s.NgayTao
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}