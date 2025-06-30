using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Services;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models.Order;
using QuanLy_PhongTro.Models.Vnpay;
using QuanLy_PhongTro.Services.Momo;
using QuanLy_PhongTro.Services.Vnpay;
using System.Security.Claims;
using QuanLy_PhongTro.Repositories;
using QuanLy_PhongTro.Repository;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace QuanLy_PhongTro.Controllers
{
    public class PayBillController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        private readonly DataContext _context;
        private readonly IEmailService _emailService;

        public PayBillController(DataContext context, IVnPayService vnPayService, IMomoService momoService, IEmailService emailService)
        {
            _context = context;
            _vnPayService = vnPayService;
            _momoService = momoService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> PayBill(int maHoaDon)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            var hoaDon = await _context.HoaDon
                .Include(h => h.HopDong)
                .ThenInclude(hd => hd.PhongTro)// Đảm bảo tải HopDong để truy cập MaNguoiDung
                .FirstOrDefaultAsync(h => h.MaHoaDon == maHoaDon && h.HopDong.MaNguoiDung == maNguoiDung);

            if (hoaDon == null)
            {
                TempData["ErrorMessage"] = "Hóa đơn không tồn tại hoặc không thuộc về bạn.";
                return RedirectToAction("TrangCaNhan", "TrangCaNhan", new { activeTab = "days-receipt" });
            }

            if (hoaDon.TrangThai == "Đã thanh toán")
            {
                TempData["ErrorMessage"] = "Hóa đơn đã được thanh toán.";
                return RedirectToAction("TrangCaNhan", "TrangCaNhan", new { activeTab = "days-receipt" });
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
            if (nguoiDung == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("TrangCaNhan", "TrangCaNhan", new { activeTab = "days-receipt" });
            }

            TempData.Keep("SuccessMessage");
            TempData.Keep("ErrorMessage");


            TempData["PendingHoaDon"] = maHoaDon;
            ViewBag.MaHoaDon = maHoaDon;
            ViewBag.TongTien = hoaDon.TongTien;
            ViewBag.NguoiDung = nguoiDung;
            ViewBag.TenPhong = hoaDon.HopDong?.PhongTro?.TenPhong ?? "Không xác định";

            return View("~/Views/Payment/PayBill.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> PayBill(int maHoaDon, string paymentMethod, decimal soTien)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            var hoaDon = await _context.HoaDon
                .Include(h => h.HopDong) // Đảm bảo tải HopDong để truy cập MaNguoiDung
                .FirstOrDefaultAsync(h => h.MaHoaDon == maHoaDon && h.HopDong.MaNguoiDung == maNguoiDung);

            if (hoaDon == null)
            {
                TempData["ErrorMessage"] = "Hóa đơn không tồn tại hoặc không thuộc về bạn.";
                return RedirectToAction("TrangCaNhan", "TrangCaNhan", new { activeTab = "days-receipt" });
            }

            if (hoaDon.TrangThai == "Đã thanh toán")
            {
                TempData["ErrorMessage"] = "Hóa đơn đã được thanh toán.";
                return RedirectToAction("TrangCaNhan", "TrangCaNhan", new { activeTab = "days-receipt" });
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
            if (nguoiDung == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("TrangCaNhan", "TrangCaNhan", new { activeTab = "days-receipt" });
            }

            if (soTien != hoaDon.TongTien)
            {
                TempData["ErrorMessage"] = "Số tiền không khớp với hóa đơn.";
                return RedirectToAction("PayBill", new { maHoaDon });
            }

            try
            {
                if (paymentMethod == "AccountPaid")
                {
                    if (nguoiDung.SoDu == null || nguoiDung.SoDu < soTien)
                    {
                        decimal soTienConThieu = soTien - (nguoiDung.SoDu);
                        return Json(new
                        {
                            success = false,
                            error = true,
                            message = $"Số dư tài khoản hiện tại ({(nguoiDung.SoDu.ToString("N0") ?? "0")} VNĐ) không đủ để thanh toán {soTien.ToString("N0")} VNĐ.",
                            redirectUrl = Url.Action("Index", "TrangCaNhan", new { tab = "deposit-history" }),
                            returnToUrl = Url.Action("PayBill", new { maHoaDon }),
                            soTienConThieu = soTienConThieu
                        });
                    }

                    try
                    {
                        if (nguoiDung.SoDu < 0) // Kiểm tra số dư âm
                        {
                            return Json(new { success = false, message = "Số dư không thể âm sau khi thanh toán." });
                        }
                        var transactionId = Guid.NewGuid().ToString();
                        nguoiDung.SoDu -= soTien;
                        _context.NguoiDungs.Update(nguoiDung);

                        hoaDon.TrangThai = "Đã thanh toán";
                        _context.HoaDon.Update(hoaDon);

                        var transaction = new AccountPaidModel
                        {
                            TransactionId = Guid.NewGuid().ToString(),
                            MaNguoiDung = maNguoiDung,
                            MaHopDong = hoaDon.MaHopDong,
                            MaHoaDon = hoaDon.MaHoaDon,
                            OrderDescription = $"Thanh toán hóa đơn #{maHoaDon} bằng tài khoản",
                            PaymentMethod = paymentMethod,
                            Amount = (double)hoaDon.TongTien,
                            DateCreated = DateTime.Now,
                        };
                        _context.AccountPaidInfor.Add(transaction);

                        await _context.SaveChangesAsync();
                        var email = TempData["PendingEmail"] as string;
                        if (!string.IsNullOrEmpty(email))
                        {
                            var subject = "Xác nhận thanh toán hóa đơn thành công";
                            var body = $@"<h2>Thanh toán thành công!</h2>
                        <p>Cảm ơn bạn đã thanh toán hóa đơn. Dưới đây là thông tin giao dịch:</p>
                        <ul>
                            <li><strong>Mã giao dịch:</strong> {transactionId}</li>
                            <li><strong>Số tiền:</strong> {soTien.ToString("N0")} VNĐ</li>
                            <li><strong>Phương thức thanh toán:</strong> Thanh toán bằng tài khoản</li>
                            <li><strong>Mã hóa đơn:</strong> {maHoaDon}</li>
                            <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                        </ul>
                        <p>Số dư hiện tại: {nguoiDung.SoDu.ToString("N0")} VNĐ</p>
                        <p>Chúc bạn có trải nghiệm tuyệt vời!</p>";

                            try
                            {
                                // Giả sử bạn có IEmailService
                                await _emailService.SendEmailAsync(email, subject, body);
                            }
                            catch (Exception ex)
                            {
                                TempData["PayBillErrorMessage"] = $"Gửi email thất bại: {ex.Message}";
                            }
                        }

                        return Json(new
                        {
                            success = true,
                            message = "Thanh toán hóa đơn thành công!",
                            redirectUrl = Url.Action("Index", "TrangCaNhan", new { tab = "days-receipt" })
                        });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = $"Có lỗi xảy ra khi xử lý thanh toán: {ex.Message}" });
                    }
                }
                else if (paymentMethod == "VNPay")
                {
                    var paymentInfo = new PaymentInformationModel
                    {
                        Name = nguoiDung.HoTen,
                        Amount = (double)hoaDon.TongTien,
                        OrderDescription = $"Thanh toán hóa đơn #{maHoaDon} qua VNPay",
                        CreatedDate = DateTime.Now,
                        OrderType = "billpayment"
                    };
                    var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext, "PayBill");
                    TempData["PendingHoaDon"] = maHoaDon;
                    return Json(new
                    {
                        success = true,
                        redirect = paymentUrl,
                        message = "Đang chuyển hướng đến VNPay..."
                    });
                }
                else if (paymentMethod == "MoMo")
                {
                    var paymentInfor = new OrderInfoModel
                    {
                        FullName = nguoiDung.HoTen,
                        Amount = (double)hoaDon.TongTien,
                        OrderInfo = $"Thanh toán hóa đơn #{maHoaDon} qua MoMo",
                        OrderId = Guid.NewGuid().ToString()
                    };
                    var paymentUrlMomo = await _momoService.CreatePaymentMomo(paymentInfor, "PayBill");
                    TempData["PendingHoaDon"] = maHoaDon;
                    return Json(new
                    {
                        success = true,
                        redirect = paymentUrlMomo.PayUrl,
                        message = "Đang chuyển hướng đến MoMo..."
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Hình thức thanh toán không hợp lệ." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra khi xử lý thanh toán: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ClearTempData()
        {
            TempData.Clear(); // Xóa toàn bộ TempData sau khi hiển thị thông báo
            return new EmptyResult();
        }
    }
}