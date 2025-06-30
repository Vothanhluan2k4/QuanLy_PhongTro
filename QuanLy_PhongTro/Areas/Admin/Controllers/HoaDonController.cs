using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Repositories;
using QuanLy_PhongTro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.ViewModel;
using System.Security.Claims;
using QuanLy_PhongTro.Services;

namespace QuanLy_PhongTro.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HoaDonController : Controller
    {
        private readonly DataContext _context;
        private readonly IEmailService _emailService;

        public HoaDonController(DataContext context,IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string searchTerm = "")
        {
            try
            {
                var invoices = await _context.HoaDon
                    .Include(hd => hd.HopDong)
                    .ThenInclude(hd => hd.PhongTro)
                    .Include(hd => hd.HopDong)
                    .ThenInclude(hd => hd.NguoiDung)
                    .Select(hd => new InvoiceViewModel
                    {
                        MaHoaDon = hd.MaHoaDon,
                        MaHopDong = hd.MaHopDong,
                        Thang = hd.Thang != 0 ? hd.Thang : 1, // Gán mặc định nếu Thang là 0 hoặc null
                        Nam = hd.Nam != 0 ? hd.Nam : DateTime.Now.Year, // Gán mặc định nếu Nam là 0 hoặc null
                        TienDien = (double)hd.TienDien,
                        TienNuoc = (double)hd.TienNuoc,
                        TienRac = (double)hd.TienRac,
                        TongTien = (double)hd.TongTien,
                        NgayPhatHanh = hd.NgayPhatHanh.ToString("dd/MM/yyyy"),
                        TrangThai = hd.TrangThai ?? "Chưa thanh toán" // Gán mặc định nếu TrangThai là null
                    })
                    .ToListAsync();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    invoices = invoices
                        .Where(i => i.MaHoaDon.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                    i.MaHopDong.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                    i.Thang.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                    i.Nam.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return Json(new { success = true, data = invoices });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi lấy danh sách hóa đơn: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChiSoDienNuoc(string maHopDong, int thang, int nam)
        {
            try
            {
                // Kiểm tra và chuyển đổi maHopDong từ string sang int
                if (!int.TryParse(maHopDong, out int maHopDongInt))
                {
                    return Json(new { success = false, message = "Mã hợp đồng không hợp lệ." });
                }

                var hopDong = await _context.HopDongs
                    .Include(hd => hd.PhongTro)
                    .FirstOrDefaultAsync(hd => hd.MaHopDong == maHopDongInt);

                if (hopDong == null)
                {
                    return Json(new { success = false, message = "Hợp đồng không tồn tại." });
                }

                // Lấy chỉ số điện/nước cũ từ bản ghi gần nhất trước tháng/năm hiện tại
                var chiSoTruoc = await _context.ChiSoDienNuoc
                    .Where(cs => cs.MaHopDong == maHopDongInt && (cs.Nam < nam || (cs.Nam == nam && cs.Thang < thang)))
                    .OrderByDescending(cs => cs.Nam)
                    .ThenByDescending(cs => cs.Thang)
                    .FirstOrDefaultAsync();

                decimal chiSoDienCu = chiSoTruoc?.ChiSoDienMoi ?? 0;
                decimal chiSoNuocCu = chiSoTruoc?.ChiSoNuocMoi ?? 0;

                // Lấy chỉ số hiện tại (nếu có) cho tháng/năm được chọn
                var chiSoHienTai = await _context.ChiSoDienNuoc
                    .FirstOrDefaultAsync(cs => cs.MaHopDong == maHopDongInt && cs.Thang == thang && cs.Nam == nam);

                decimal chiSoDienMoi = chiSoHienTai?.ChiSoDienMoi ?? chiSoDienCu;
                decimal chiSoNuocMoi = chiSoHienTai?.ChiSoNuocMoi ?? chiSoNuocCu;

                // Lấy đơn giá từ PhongTro
                decimal tienDienUnit = hopDong.PhongTro.TienDien;
                decimal tienNuocUnit = hopDong.PhongTro.TienNuoc;

                // Tính tiền điện/nước nếu có chỉ số hiện tại
                decimal tienDien = chiSoHienTai != null ? (chiSoDienMoi - chiSoDienCu) * tienDienUnit : 0;
                decimal tienNuoc = chiSoHienTai != null ? (chiSoNuocMoi - chiSoNuocCu) * tienNuocUnit : 0;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        ChiSoDienCu = chiSoDienCu,
                        ChiSoDienMoi = chiSoDienMoi,
                        ChiSoNuocCu = chiSoNuocCu,
                        ChiSoNuocMoi = chiSoNuocMoi,
                        TienDien = tienDien,
                        TienNuoc = tienNuoc,
                        TienDienUnit = tienDienUnit,
                        TienNuocUnit = tienNuocUnit
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Add([FromForm] HoaDonInputDto input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var hopDong = await _context.HopDongs
                        .Include(hd => hd.PhongTro)
                        .FirstOrDefaultAsync(hd => hd.MaHopDong == input.MaHopDong);

                    if (hopDong == null)
                    {
                        return Json(new { success = false, message = "Hợp đồng không tồn tại." });
                    }

                    // Kiểm tra hóa đơn đã tồn tại chưa
                    var existingHoaDon = await _context.HoaDon
                        .FirstOrDefaultAsync(hd => hd.MaHopDong == input.MaHopDong && hd.Thang == input.Thang && hd.Nam == input.Nam);

                    if (existingHoaDon != null)
                    {
                        return Json(new { success = false, message = "Hóa đơn cho hợp đồng này trong tháng/năm này đã tồn tại." });
                    }

                    // Kiểm tra chỉ số điện/nước hợp lệ
                    if (input.CSDienMoi < input.CSDienCu || input.CSNuocMoi < input.CSNuocCu)
                    {
                        return Json(new { success = false, message = "Chỉ số mới phải lớn hơn hoặc bằng chỉ số cũ." });
                    }

                    // Tính tiền điện/nước dựa trên đơn giá từ PhongTro
                    var phongTro = await _context.PhongTros
                        .FirstOrDefaultAsync(p => p.MaPhong == hopDong.MaPhong);
                    decimal tienDien = (input.CSDienMoi - input.CSDienCu) * phongTro.TienDien;
                    decimal tienNuoc = (input.CSNuocMoi - input.CSNuocCu) * phongTro.TienNuoc;

                    // Tạo và lưu HoaDon
                    var hoaDon = new HoaDonModel
                    {
                        MaHopDong = input.MaHopDong,
                        Thang = input.Thang,
                        Nam = input.Nam,
                        TienDien = tienDien,
                        TienNuoc = tienNuoc,
                        TienRac = input.TienRac,
                        TongTien = input.TongTien,
                        NgayPhatHanh = DateTime.Now,
                        TrangThai = input.TrangThai
                    };

                    _context.HoaDon.Add(hoaDon);

                    // Tạo và lưu ChiSoDienNuoc
                    var chiSo = new ChiSoDienNuocModel
                    {
                        MaHopDong = input.MaHopDong,
                        Thang = input.Thang,
                        Nam = input.Nam,
                        ChiSoDienCu = input.CSDienCu,
                        ChiSoDienMoi = input.CSDienMoi,
                        ChiSoNuocCu = input.CSNuocCu,
                        ChiSoNuocMoi = input.CSNuocMoi,
                        NgayGhi = DateTime.Now
                    };

                    _context.ChiSoDienNuoc.Add(chiSo);

                    await _context.SaveChangesAsync();

                    // Gửi email thông báo cho người dùng
                    var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(nd => nd.MaNguoiDung == hopDong.MaNguoiDung);

                    if (nguoiDung != null && !string.IsNullOrEmpty(nguoiDung.Email))
                    {
                        var subject = "Thông báo phát hành hóa đơn mới";
                        var body = $@"<h2>Thông báo hóa đơn mới!</h2>
                        <p>Kính gửi {nguoiDung.HoTen},</p>
                        <p>Hóa đơn mới cho hợp đồng {hoaDon.MaHopDong} đã được phát hành. Dưới đây là chi tiết:</p>
                        <ul>
                            <li><strong>Mã hóa đơn:</strong> {hoaDon.MaHoaDon}</li>
                            <li><strong>Tháng/Năm:</strong> {hoaDon.Thang}/{hoaDon.Nam}</li>
                            <li><strong>Tiền điện:</strong> {hoaDon.TienDien.ToString("N0")} VNĐ</li>
                            <li><strong>Tiền nước:</strong> {hoaDon.TienNuoc.ToString("N0")} VNĐ</li>
                            <li><strong>Tiền rác:</strong> {hoaDon.TienRac.ToString("N0")} VNĐ</li>
                            <li><strong>Tổng tiền:</strong> {hoaDon.TongTien.ToString("N0")} VNĐ</li>
                            <li><strong>Trạng thái:</strong> {hoaDon.TrangThai}</li>
                            <li><strong>Ngày phát hành:</strong> {hoaDon.NgayPhatHanh:dd/MM/yyyy HH:mm:ss}</li>
                        </ul>
                        <p>Vui lòng thanh toán trước ngày đáo hạn. Truy cập website để xem chi tiết và thực hiện thanh toán.</p>
                        <p>Trân trọng,</p>
                        <p>Quản lý phòng trọ</p>";

                        try
                        {
                            await _emailService.SendEmailAsync(nguoiDung.Email, subject, body);
                            Console.WriteLine($"Email sent successfully to {nguoiDung.Email}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send email to {nguoiDung.Email}: {ex.Message}");
                            TempData["ErrorMessage"] = $"Thêm hóa đơn thành công nhưng gửi email thất bại: {ex.Message}";
                        }
                    }
                    else
                    {
                        Console.WriteLine("No email address found for the user.");
                        TempData["WarningMessage"] = "Thêm hóa đơn thành công nhưng không thể gửi email vì thiếu thông tin email.";
                    }

                    var invoiceData = new
                    {
                        MaHoaDon = hoaDon.MaHoaDon,
                        MaHopDong = hoaDon.MaHopDong,
                        Thang = hoaDon.Thang,
                        Nam = hoaDon.Nam,
                        TienDien = (double)hoaDon.TienDien,
                        TienNuoc = (double)hoaDon.TienNuoc,
                        TienRac = (double)hoaDon.TienRac,
                        TongTien = (double)hoaDon.TongTien,
                        NgayPhatHanh = hoaDon.NgayPhatHanh.ToString("dd/MM/yyyy"),
                        TrangThai = hoaDon.TrangThai
                    };

                    return Json(new { success = true, message = "Thêm hóa đơn thành công!", data = invoiceData });
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi thêm hóa đơn: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] HoaDonInputDto input)
        {
            try
            {
                var existingHoaDon = await _context.HoaDon.FindAsync(input.MaHoaDon);
                if (existingHoaDon == null)
                {
                    return Json(new { success = false, message = "Hóa đơn không tồn tại." });
                }

                var hopDong = await _context.HopDongs
                    .Include(hd => hd.PhongTro)
                    .FirstOrDefaultAsync(hd => hd.MaHopDong == input.MaHopDong);

                if (hopDong == null)
                {
                    return Json(new { success = false, message = "Hợp đồng không tồn tại." });
                }

                // Kiểm tra chỉ số điện/nước hợp lệ
                if (input.CSDienMoi < input.CSDienCu)
                {
                    return Json(new { success = false, message = "Chỉ số điện mới không thể nhỏ hơn chỉ số điện cũ." });
                }

                if (input.CSNuocMoi < input.CSNuocCu)
                {
                    return Json(new { success = false, message = "Chỉ số nước mới không thể nhỏ hơn chỉ số nước cũ." });
                }

                var chiSo = await _context.ChiSoDienNuoc
                    .FirstOrDefaultAsync(c => c.MaHopDong == input.MaHopDong && c.Thang == input.Thang && c.Nam == input.Nam);

                var phongTro = await _context.PhongTros.FirstOrDefaultAsync(p => p.MaPhong == hopDong.MaPhong);

                // Cập nhật hoặc tạo mới ChiSoDienNuoc
                if (chiSo != null)
                {
                    chiSo.ChiSoDienCu = input.CSDienCu;
                    chiSo.ChiSoDienMoi = input.CSDienMoi;
                    chiSo.ChiSoNuocCu = input.CSNuocCu;
                    chiSo.ChiSoNuocMoi = input.CSNuocMoi;
                    _context.ChiSoDienNuoc.Update(chiSo);
                }
                else
                {
                    chiSo = new ChiSoDienNuocModel
                    {
                        MaHopDong = input.MaHopDong,
                        Thang = input.Thang,
                        Nam = input.Nam,
                        ChiSoDienCu = input.CSDienCu,
                        ChiSoDienMoi = input.CSDienMoi,
                        ChiSoNuocCu = input.CSNuocCu,
                        ChiSoNuocMoi = input.CSNuocMoi,
                        NgayGhi = DateTime.Now
                    };
                    _context.ChiSoDienNuoc.Add(chiSo);
                }

                // Tính lại TienDien và TienNuoc dựa trên chỉ số mới
                existingHoaDon.TienDien = (input.CSDienMoi - input.CSDienCu) * phongTro.TienDien;
                existingHoaDon.TienNuoc = (input.CSNuocMoi - input.CSNuocCu) * phongTro.TienNuoc;

                existingHoaDon.MaHopDong = input.MaHopDong;
                existingHoaDon.Thang = input.Thang;
                existingHoaDon.Nam = input.Nam;
                existingHoaDon.TienRac = input.TienRac;
                existingHoaDon.NgayPhatHanh = DateTime.Now;
                existingHoaDon.TrangThai = input.TrangThai;

                // Tính lại TongTien
                existingHoaDon.TongTien = existingHoaDon.TienDien + existingHoaDon.TienNuoc + existingHoaDon.TienRac;

                _context.HoaDon.Update(existingHoaDon);
                await _context.SaveChangesAsync();

                // Kiểm tra lại chiSo sau khi lưu để xác nhận
                var updatedChiSo = await _context.ChiSoDienNuoc
                    .FirstOrDefaultAsync(c => c.MaHopDong == input.MaHopDong && c.Thang == input.Thang && c.Nam == input.Nam);
                if (updatedChiSo != null && (updatedChiSo.ChiSoDienMoi != input.CSDienMoi || updatedChiSo.ChiSoNuocMoi != input.CSNuocMoi))
                {
                    return Json(new { success = false, message = "Lỗi khi cập nhật chỉ số điện nước trong cơ sở dữ liệu." });
                }

                var invoiceData = new
                {
                    MaHoaDon = existingHoaDon.MaHoaDon,
                    MaHopDong = existingHoaDon.MaHopDong,
                    Thang = existingHoaDon.Thang,
                    Nam = existingHoaDon.Nam,
                    TienDien = (double)existingHoaDon.TienDien,
                    TienNuoc = (double)existingHoaDon.TienNuoc,
                    TienRac = (double)existingHoaDon.TienRac,
                    TongTien = (double)existingHoaDon.TongTien,
                    NgayPhatHanh = existingHoaDon.NgayPhatHanh.ToString("dd/MM/yyyy"),
                    TrangThai = existingHoaDon.TrangThai
                };

                return Json(new { success = true, message = "Cập nhật hóa đơn thành công!", data = invoiceData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi cập nhật hóa đơn: {ex.Message}" });
            }
        }
        [HttpDelete]
        [Route("Admin/HoaDon/Delete/{maHoaDon}")]
        public async Task<IActionResult> Delete(int maHoaDon)
        {
            try
            {
                var hoaDon = await _context.HoaDon
                    .Include(hd => hd.HopDong)
                    .FirstOrDefaultAsync(hd => hd.MaHoaDon == maHoaDon);

                if (hoaDon == null)
                {
                    return Json(new { success = false, message = "Hóa đơn không tồn tại." });
                }

                var chiSoDienNuoc = await _context.ChiSoDienNuoc
                    .Where(cs => cs.MaHopDong == hoaDon.MaHopDong && cs.Thang == hoaDon.Thang && cs.Nam == hoaDon.Nam)
                    .ToListAsync();

                if (chiSoDienNuoc.Any())
                {
                    _context.ChiSoDienNuoc.RemoveRange(chiSoDienNuoc);
                }

                _context.HoaDon.Remove(hoaDon);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa hóa đơn thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa hóa đơn: {ex.Message}" });
            }
        }
    }
}