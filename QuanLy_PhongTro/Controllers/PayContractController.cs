using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.Services.Momo;
using QuanLy_PhongTro.Services.Vnpay;
using QuanLy_PhongTro.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models.Order;
using QuanLy_PhongTro.Models.Vnpay;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Migrations;

namespace QuanLy_PhongTro.Controllers
{
    public class PayContractController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        private readonly DataContext _context;
        private readonly IEmailService _emailService;

        public PayContractController(DataContext context, IVnPayService vnPayService, IMomoService momoService, IEmailService emailService)
        {
            _context = context;
            _vnPayService = vnPayService;
            _momoService = momoService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> PayContract(int maHopDong)
        {

            var hopDong = await _context.HopDongs
                .Include(h => h.PhongTro)
                .Include(h => h.NguoiDung)
                .FirstOrDefaultAsync(h => h.MaHopDong == maHopDong);

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
            if (hopDong == null)
            {
                TempData["ErrorMessage"] = "Hợp đồng không tồn tại.";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
            }
            var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
            if (nguoiDung == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("TrangCaNhan", "TrangCaNhan", new { activeTab = "days-receipt" });
            }
            ViewBag.MaHopDong = hopDong.MaHopDong;
            ViewBag.TongTien = hopDong.TienCoc; // Giả sử TienCoc là số tiền cần thanh toán
            ViewBag.TenPhong = hopDong.PhongTro?.TenPhong ?? "Không xác định";
            ViewBag.NguoiDung = nguoiDung;

            return View("~/Views/Payment/PayContract.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> PayContract(int maHopDong, string paymentMethod, decimal soTien)
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

            var hopDong = await _context.HopDongs
                .Include(h => h.NguoiDung)
                .Include(h => h.PhongTro)
                .FirstOrDefaultAsync(h => h.MaHopDong == maHopDong && h.MaNguoiDung == maNguoiDung);

            if (hopDong == null)
            {
                TempData["ErrorMessage"] = "Hợp đồng không tồn tại hoặc không thuộc về bạn.";
                return RedirectToAction("TrangCaNhan", "TrangCaNhan", new { activeTab = "room-book" });
            }

            if (hopDong.TrangThai == "Đã thanh toán")
            {
                TempData["ErrorMessage"] = "Hợp đồng đã được thanh toán.";
                return RedirectToAction("TrangCaNhan", "TrangCaNhan", new { activeTab = "room-book" });
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
            if (nguoiDung == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("TrangCaNhan", "TrangCaNhan", new { activeTab = "room-book" });
            }

            if (soTien != hopDong.TienCoc) // Giả sử TienCoc là số tiền cần thanh toán cho hợp đồng
            {
                TempData["ErrorMessage"] = "Số tiền không khớp với tiền cọc hợp đồng.";
                return RedirectToAction("PayContract", new { maHopDong });
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
                            returnToUrl = Url.Action("PayContract", new { maHopDong }),
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

                        hopDong.TrangThai = "Đã thanh toán";
                        hopDong.PhuongThucThanhToan = "AccountPaid "+ transactionId;
                        _context.HopDongs.Update(hopDong);

                        var transaction = new AccountPaidModel
                        {
                            TransactionId = Guid.NewGuid().ToString(),
                            MaNguoiDung = maNguoiDung,
                            MaHopDong = hopDong.MaHopDong,
                            MaHoaDon = null, // Không áp dụng cho hợp đồng
                            OrderDescription = $"Thanh toán hợp đồng #{maHopDong} bằng tài khoản",
                            PaymentMethod = paymentMethod,
                            Amount = (double)hopDong.TienCoc,
                            DateCreated = DateTime.Now,
                        };
                        _context.AccountPaidInfor.Add(transaction);

                        await _context.SaveChangesAsync();
                        var email = TempData["PendingEmail"] as string ?? nguoiDung.Email;
                        if (!string.IsNullOrEmpty(email))
                        {
                            var subject = "Xác nhận thanh toán hợp đồng thành công";
                            var body = $@"<h2>Thanh toán thành công!</h2>
                                    <p>Cảm ơn bạn đã thanh toán hợp đồng. Dưới đây là thông tin giao dịch:</p>
                                    <ul>
                                        <li><strong>Mã giao dịch:</strong> {transactionId}</li>
                                        <li><strong>Số tiền:</strong> {soTien.ToString("N0")} VNĐ</li>
                                        <li><strong>Phương thức thanh toán:</strong> Thanh toán bằng tài khoản</li>
                                        <li><strong>Mã hợp đồng:</strong> {maHopDong}</li>
                                        <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                                    </ul>
                                    <p>Số dư hiện tại: {nguoiDung.SoDu.ToString("N0")} VNĐ</p>
                                    <p>Chúc bạn có trải nghiệm tuyệt vời!</p>";

                            try
                            {
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
                            message = "Thanh toán hợp đồng thành công!",
                            redirectUrl = Url.Action("Index", "TrangCaNhan", new { tab = "room-book" })
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
                        Amount = (double)hopDong.TienCoc,
                        OrderDescription = $"Thanh toán hợp đồng #{maHopDong} qua VNPay",
                        CreatedDate = DateTime.Now,
                        OrderType = "contractpayment"
                    };
                    var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext, "PayContract");
                    TempData["PendingHopDongN"] = maHopDong; 
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
                        Amount = (double)hopDong.TienCoc,
                        OrderInfo = $"Thanh toán hợp đồng #{maHopDong} qua MoMo",
                        OrderId = Guid.NewGuid().ToString()
                    };
                    var paymentUrlMomo = await _momoService.CreatePaymentMomo(paymentInfor, "PayContract");
                    TempData["PendingHopDongN"] = maHopDong; 
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
