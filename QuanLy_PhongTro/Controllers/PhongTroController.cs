using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using System.Security.Claims;
using System.Text.RegularExpressions;
using QuanLy_PhongTro.ViewModel;
using X.PagedList;
using X.PagedList.Extensions;
[Route("phong-tro")]
public class PhongTroController : Controller
{
    private readonly DataContext _context;

    public PhongTroController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index(int page = 1)
    {
        ViewBag.CurrentPage = page;
        int pageSize = 5;
        int pageNumber = page;

        // Lấy danh sách phòng trọ với phân trang
        var rooms = _context.PhongTros
            .Include(r => r.AnhPhongTros)
            .Include(r => r.PhongTroThietBis)
                .ThenInclude(tb => tb.ThietBi)
            .Include(r => r.LoaiPhong)
            .OrderBy(r => r.MaPhong)
            .ToPagedList(pageNumber, pageSize);

        var totalItems = _context.PhongTros.Count(); ;
        ViewBag.TotalItems = totalItems;
        ViewBag.Rooms = rooms;

        // Lấy danh sách phòng đã lưu của người dùng hiện tại
        if (User.Identity.IsAuthenticated)
        {
            var userId = GetCurrentUserId();
            var savedRooms = _context.LuuPhongs
                .Where(lp => lp.MaNguoiDung == userId)
                .Select(lp => lp.MaPhong)
                .ToList();

            // Đặt ViewData cho tất cả phòng (đã lưu và chưa lưu)
            foreach (var room in rooms)
            {
                ViewData[$"IsSaved_{room.MaPhong}"] = savedRooms.Contains(room.MaPhong);
            }
        }
        else
        {
            // Nếu chưa đăng nhập, tất cả phòng đều chưa lưu
            foreach (var room in rooms)
            {
                ViewData[$"IsSaved_{room.MaPhong}"] = false;
            }
        }

        // Nhóm phòng theo LoaiPhong
        var groupedRooms = rooms
            .GroupBy(r => r.LoaiPhong)
            .ToList();

        return View("~/Views/PhongTro/PhongTro.cshtml", groupedRooms);
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

    [HttpGet("loai-phong")]
    public async Task<IActionResult> GetLoaiPhong()
    {
        var loaiPhong = await _context.LoaiPhongs.ToListAsync();
        return Ok(loaiPhong);
    }

    [HttpGet("api/rooms")]
    public async Task<IActionResult> GetRooms()
    {
        try
        {
            int? maNguoiDung = User.Identity.IsAuthenticated ?
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) : null;

            var rooms = await _context.PhongTros
                .Include(r => r.AnhPhongTros)
                .Include(r => r.PhongTroThietBis)
                    .ThenInclude(tb => tb.ThietBi)
                .Include(r => r.LoaiPhong)
                .ToListAsync();

            var result = rooms.Select(r => new
            {
                Room = r,
                IsSaved = maNguoiDung != null && _context.LuuPhongs.Any(lp =>
                    lp.MaPhong == r.MaPhong && lp.MaNguoiDung == maNguoiDung)
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi lấy danh sách phòng: {ex}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("chi-tiet/{id}")]
    public async Task<IActionResult> ChiTietPhong(int id)
    {
        try
        {
            var phongTro = await _context.PhongTros
                .Include(p => p.LoaiPhong)
                .Include(p => p.AnhPhongTros)
                .Include(p => p.PhongTroThietBis)
                    .ThenInclude(pt => pt.ThietBi)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phongTro == null)
            {
                return NotFound();
            }
            phongTro.TrangThai = phongTro.TrangThai?.Trim().Replace("\n", "").Replace("\r", "") ?? "Không xác định";
            var relatedRooms = await _context.PhongTros
                .Where(p => p.MaPhong != id && p.LoaiPhong.TenLoaiPhong == phongTro.LoaiPhong.TenLoaiPhong)
                .Where(p => p.TrangThai == "Còn trống") // Chỉ lấy phòng còn trống
                .Where(p => p.PhongTroThietBis.Any(pt => phongTro.PhongTroThietBis.Select(ptt => ptt.ThietBi.TenThietBi).Contains(pt.ThietBi.TenThietBi)))
                .Include(p => p.AnhPhongTros)
                .Include(p => p.LoaiPhong)
                .ToListAsync();

            phongTro.RelatedRooms = relatedRooms;
            return View(phongTro);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

 

    [HttpPost("luu-phong/{id}")]
    public IActionResult LuuPhong(int id)
    {
        try
        {
            int maNguoiDung = GetCurrentUserId();
            var phongTro = _context.PhongTros.FirstOrDefault(p => p.MaPhong == id);

            if (phongTro == null)
            {
                return Json(new { success = false, message = "Phòng không tồn tại." });
            }

            var existing = _context.LuuPhongs
                .FirstOrDefault(lp => lp.MaPhong == id && lp.MaNguoiDung == maNguoiDung);

            if (existing != null)
            {
                _context.LuuPhongs.Remove(existing);
                _context.SaveChanges();
                return Json(new { success = true, message = "Đã bỏ lưu phòng", isSaved = false });
            }
            else
            {
                _context.LuuPhongs.Add(new LuuPhongModel
                {
                    MaPhong = id,
                    MaNguoiDung = maNguoiDung,
                    NgayLuu = DateTime.Now
                });
                _context.SaveChanges();
                return Json(new { success = true, message = "Đã lưu phòng", isSaved = true });
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
        }
    }

    [HttpGet("danh-sach-luu-phong")]
    public async Task<IActionResult> GetSavedRooms()
    {
        try
        {
            int maNguoiDung = GetCurrentUserId(); // Lấy MaNguoiDung từ HttpContext.User

            var savedRoomIds = _context.LuuPhongs
                .Where(lp => lp.MaNguoiDung == maNguoiDung)
                .Select(lp => lp.MaPhong)
                .ToList();

            var rooms = await _context.PhongTros
                .Include(r => r.AnhPhongTros)
                .Include(r => r.PhongTroThietBis)
                    .ThenInclude(tb => tb.ThietBi)
                .Include(r => r.LoaiPhong)
                .Where(r => savedRoomIds.Contains(r.MaPhong))
                .ToListAsync();

            return Json(new { success = true, data = rooms });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
        }
    }

    [HttpPost("xoa-tat-ca-luu-phong")]
    public IActionResult ClearSavedRooms()
    {
        try
        {
            int maNguoiDung = GetCurrentUserId(); 

            var savedRooms = _context.LuuPhongs.Where(lp => lp.MaNguoiDung == maNguoiDung);
            _context.LuuPhongs.RemoveRange(savedRooms);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã xóa tất cả phòng đã lưu." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
        }
    }

    [HttpPost("filter")]
    public async Task<IActionResult> Filter([FromBody] FilterModel filter)
    {
        try
        {
            var query = _context.PhongTros
             .Include(r => r.AnhPhongTros)
             .Include(r => r.PhongTroThietBis)
                 .ThenInclude(tb => tb.ThietBi)
             .Include(r => r.LoaiPhong)
             .AsQueryable();
            // Lấy tổng số phòng sau khi lọc
            var totalCount = await query.CountAsync();
            // Lọc theo loại phòng
            if (!string.IsNullOrEmpty(filter.RoomType))
            {
                var roomTypeIds = filter.RoomType.Split(',')
                    .Select(id => int.TryParse(id, out int result) ? result : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (roomTypeIds.Any())
                {
                    query = query.Where(r => roomTypeIds.Contains(r.MaLoaiPhong));
                }
            }

            // Lọc theo mức giá
            if (!string.IsNullOrEmpty(filter.Price))
            {
                switch (filter.Price)
                {
                    case "under-2":
                        query = query.Where(r => r.GiaThue < 2000000);
                        break;
                    case "2-3":
                        query = query.Where(r => r.GiaThue >= 2000000 && r.GiaThue <= 3000000);
                        break;
                    case "3-5":
                        query = query.Where(r => r.GiaThue >= 3000000 && r.GiaThue <= 5000000);
                        break;
                    case "5-7":
                        query = query.Where(r => r.GiaThue >= 5000000 && r.GiaThue <= 7000000);
                        break;
                    case "over":
                        query = query.Where(r => r.GiaThue > 7000000);
                        break;
                }
            }

            // Lọc theo diện tích
            if (!string.IsNullOrEmpty(filter.Area))
            {
                switch (filter.Area)
                {
                    case "under-20":
                        query = query.Where(r => r.DienTich < 20);
                        break;
                    case "20-30":
                        query = query.Where(r => r.DienTich >= 20 && r.DienTich <= 30);
                        break;
                    case "30-50":
                        query = query.Where(r => r.DienTich >= 30 && r.DienTich <= 50);
                        break;
                    case "over-50":
                        query = query.Where(r => r.DienTich > 50);
                        break;
                }
            }

            // Lọc theo tiện nghi (Sửa logic: chỉ cần có ít nhất một thiết bị được chọn)
            if (!string.IsNullOrEmpty(filter.Amenities))
            {
                var amenities = filter.Amenities.Split(',').Select(int.Parse).ToList();
                Console.WriteLine($"Amenities received: {string.Join(", ", amenities)}");

                // Kiểm tra các MaThietBi hợp lệ trong database
                var validAmenities = _context.ThietBis
                    .Where(tb => amenities.Contains(tb.MaThietBi))
                    .Select(tb => tb.MaThietBi)
                    .ToList();
                Console.WriteLine($"Valid amenities in database: {string.Join(", ", validAmenities)}");

                if (validAmenities.Any())
                {
                    query = query.Where(r => r.PhongTroThietBis.Any(pt => amenities.Contains(pt.MaThietBi)));
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy tiện nghi hợp lệ." });
                }
            }

            // Sắp xếp
            switch (filter.Sort)
            {
                case "default-sort":
                    query = query.OrderBy(r => r.MaPhong);
                    break;
                case "price-asc":
                    query = query.OrderBy(r => r.GiaThue);
                    break;
                case "price-des":
                    query = query.OrderByDescending(r => r.GiaThue);
                    break;
                case "area-asc":
                    query = query.OrderBy(r => r.DienTich);
                    break;
                case "area-des":
                    query = query.OrderByDescending(r => r.DienTich);
                    break;
                default:
                    query = query.OrderBy(r => r.MaPhong);
                    break;
            }

            // Phân trang
            var rooms = await query.ToListAsync();
            var groupedQuery = rooms
                .GroupBy(r => new { r.MaLoaiPhong, r.LoaiPhong.TenLoaiPhong })
                .OrderBy(g => g.Key.MaLoaiPhong);

            // Get saved room IDs for current user
            var savedRoomIds = new List<int>();
            if (User.Identity.IsAuthenticated)
            {
                var userId = GetCurrentUserId();
                savedRoomIds = await _context.LuuPhongs
                    .Where(lp => lp.MaNguoiDung == userId)
                    .Select(lp => lp.MaPhong)
                    .ToListAsync();
            }

            const int pageSize = 6;
            var groupedRooms = new List<object>();

            foreach (var group in groupedQuery)
            {
                var roomsInGroup = group.ToList();
                int totalRoomsInGroup = roomsInGroup.Count;
                int totalPages = (int)Math.Ceiling((double)totalRoomsInGroup / pageSize);

                // Retrieve page number from filter.Page dictionary, default to 1
                int page = 1;
                if (filter.Page != null && filter.Page.TryGetValue(group.Key.MaLoaiPhong, out var pageNum))
                {
                    page = pageNum;
                    Console.WriteLine($"Found page {pageNum} for loaiPhong {group.Key.MaLoaiPhong}");
                }

                // Ensure page is valid (between 1 and totalPages)
                page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

                Console.WriteLine($"Group: {group.Key.TenLoaiPhong}, Processing Page: {page}, Total Pages: {totalPages}");

                // Calculate skip/take values for pagination
                int skip = (page - 1) * pageSize;
                int take = pageSize;

                // Get rooms for current page
                var pagedRooms = roomsInGroup
                    .Skip(skip)
                    .Take(take)
                    .Select(r => new
                    {
                        r.MaPhong,
                        r.TenPhong,
                        r.DiaChi,
                        r.GiaThue,
                        r.DienTich,
                        r.TrangThai,
                        r.MaLoaiPhong,
                        AnhDaiDien = r.AnhPhongTros?.FirstOrDefault(a => a.LaAnhDaiDien)?.DuongDan,
                        ThietBis = r.PhongTroThietBis?.Select(pt => new { pt.ThietBi.MaThietBi, pt.ThietBi.TenThietBi }).ToList(),
                        IsSaved = savedRoomIds.Contains(r.MaPhong)
                    })
                    .ToList();

                // Add group data with pagination info
                groupedRooms.Add(new
                {
                    key = group.Key.TenLoaiPhong,
                    maLoaiPhong = group.Key.MaLoaiPhong,
                    rooms = pagedRooms,
                    pagination = new
                    {
                        currentPage = page,
                        totalPages = totalPages,
                        totalRooms = totalRoomsInGroup
                    }
                });
            }

            // Calculate total count across all groups
            int totalRooms = rooms.Count();

            // Return JSON result with grouped data and pagination info
            return Json(new
            {
                success = true,
                data = groupedRooms,
                totalCount = totalRooms
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Filter: {ex.Message}");
            return Json(new { success = false, message = $"Lỗi: {ex.Message}" });



        }
    } 
}