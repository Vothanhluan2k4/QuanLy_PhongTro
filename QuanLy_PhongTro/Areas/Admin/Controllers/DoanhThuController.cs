using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.ViewModel;
using System.Data;

namespace QuanLy_PhongTro.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DoanhThuController : Controller
    {
        private readonly DataContext _context;

        public DoanhThuController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string method = "revenue-booking")
        {
            try
            {
                var start = startDate ?? new DateTime(2025, 1, 1); // Mặc định từ đầu năm 2025
                var end = endDate ?? DateTime.Now.Date; // Mặc định đến ngày hiện tại

                // Đảm bảo endDate không nhỏ hơn startDate
                if (start > end)
                {
                    return BadRequest("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
                }

                var viewModel = new PhongTroNguoiDungViewModel
                {
                    Revenue = new RevenueViewModel()
                };

                var filteredHopDongs = startDate.HasValue && endDate.HasValue
                    ? await _context.HopDongs
                        .Where(h => h.NgayKy >= startDate.Value && h.NgayKy <= endDate.Value)
                        .ToListAsync()
                    : await _context.HopDongs.ToListAsync();

                var filteredPhongTros = startDate.HasValue && endDate.HasValue
                    ? await _context.PhongTros
                        .Where(p => p.NgayTao >= startDate.Value && p.NgayTao <= endDate.Value)
                        .ToListAsync()
                    : await _context.PhongTros.ToListAsync();

                var filteredNguoiDungs = startDate.HasValue && endDate.HasValue
                    ? await _context.NguoiDungs
                        .Where(n => n.NgayTao >= startDate.Value && n.NgayTao <= endDate.Value)
                        .ToListAsync()
                    : await _context.NguoiDungs.ToListAsync();

                var filteredHoaDons = startDate.HasValue && endDate.HasValue
                    ? await _context.HoaDon
                        .Where(hd => hd.NgayPhatHanh >= startDate.Value && hd.NgayPhatHanh <= endDate.Value)
                        .ToListAsync()
                    : await _context.HoaDon.ToListAsync();

                viewModel.Revenue.TotalCustomers = filteredNguoiDungs
                    .Where(n => n.IsDeleted == false)
                    .Select(n => n.MaNguoiDung)
                    .Distinct()
                    .Count();

                viewModel.Revenue.TotalRooms = filteredPhongTros
                    .Where(p => p.TrangThai == "Đã thuê")
                    .Select(p => p.MaPhong)
                    .Distinct()
                    .Count();

                viewModel.Revenue.TotalContracts = filteredHopDongs
                    .Where(h => h.TrangThai == "Đang hiệu lực")
                    .Count();

                var chartData = new List<object>();
                var roomTypeData = new List<object>();

                // Tạo chartData dựa trên method (chỉ cho biểu đồ cột)
                if (method == "revenue-booking")
                {
                    var contractDeposits = await _context.HopDongs
                        .Where(h => h.NgayKy >= start && h.NgayKy <= end)
                        .GroupBy(h => new { h.NgayKy.Year, h.NgayKy.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            TotalDeposit = g.Sum(h => h.TienCoc)
                        })
                        .OrderBy(g => g.Year).ThenBy(g => g.Month)
                        .ToListAsync();

                    viewModel.Revenue.TotalDepositAmount = filteredHopDongs.Sum(h => h.TienCoc);
                    chartData = contractDeposits.Select(cd => new
                    {
                        label = $"{cd.Month}/{cd.Year}",
                        value = cd.TotalDeposit
                    }).Cast<object>().ToList();
                }
                else if (method == "revenue-receipt")
                {
                    var invoiceTotals = await _context.HoaDon
                        .Where(hd => hd.NgayPhatHanh >= start && hd.NgayPhatHanh <= end)
                        .GroupBy(hd => new { hd.NgayPhatHanh.Year, hd.NgayPhatHanh.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            TotalInvoice = g.Sum(hd => hd.TongTien)
                        })
                        .OrderBy(g => g.Year).ThenBy(g => g.Month)
                        .ToListAsync();

                    viewModel.Revenue.TotalInvoiceAmount = filteredHoaDons.Sum(hd => hd.TongTien);
                    chartData = invoiceTotals.Select(it => new
                    {
                        label = $"{it.Month}/{it.Year}",
                        value = it.TotalInvoice
                    }).Cast<object>().ToList();
                }
                else
                {
                    // Nếu method không hợp lệ, vẫn trả về chartData rỗng nhưng không làm thất bại toàn bộ yêu cầu
                    chartData = new List<object>();
                }

                // Tạo roomTypeData (cho biểu đồ hình tròn) không phụ thuộc vào method
                var roomTypeBookings = await _context.LoaiPhongs
                    .GroupJoin(_context.PhongTros,
                        lp => lp.MaLoaiPhong,
                        pt => pt.MaLoaiPhong,
                        (lp, pts) => new { LoaiPhong = lp, PhongTros = pts })
                    .SelectMany(
                        x => x.PhongTros.DefaultIfEmpty(),
                        (lp, pt) => new { lp.LoaiPhong, PhongTro = pt })
                    .GroupJoin(_context.HopDongs
                        .Where(h => h.TrangThai == "Đang hiệu lực" && h.NgayKy >= start && h.NgayKy <= end),
                        x => x.PhongTro != null ? x.PhongTro.MaPhong : 0,
                        h => h.MaPhong,
                        (x, hds) => new { x.LoaiPhong, HopDongs = hds })
                    .GroupBy(x => new { x.LoaiPhong.MaLoaiPhong, x.LoaiPhong.TenLoaiPhong })
                    .Select(g => new
                    {
                        RoomTypeId = g.Key.MaLoaiPhong,
                        RoomTypeName = g.Key.TenLoaiPhong,
                        BookingCount = g.Sum(x => x.HopDongs.Count())
                    })
                    .OrderBy(g => g.RoomTypeName)
                    .ToListAsync();

                roomTypeData = roomTypeBookings.Select(rt => new
                {
                    label = rt.RoomTypeName,
                    value = rt.BookingCount
                }).Cast<object>().ToList();

                // Ghi log để kiểm tra dữ liệu roomTypeData
                Console.WriteLine($"roomTypeData: {System.Text.Json.JsonSerializer.Serialize(roomTypeData)}");

                var result = new
                {
                    revenue = viewModel.Revenue,
                    chartData,
                    roomTypeData
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}