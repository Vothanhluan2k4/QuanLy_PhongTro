using Microsoft.AspNetCore.Mvc;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Newtonsoft.Json;
using QuanLy_PhongTro.Models.Vnpay;
using QuanLy_PhongTro.Services.Vnpay;
using QuanLy_PhongTro.Services.Momo;
using QuanLy_PhongTro.Models.Order;
using System.Reflection.Metadata.Ecma335;
using QuanLy_PhongTro.ViewModel;
using QuanLy_PhongTro.Services;
using QuanLy_PhongTro.Migrations;

namespace QuanLy_PhongTro.Controllers
{
    public class DatPhongController : Controller
    {
        private readonly DataContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        private readonly IEmailService _emailService;
        public DatPhongController(DataContext context, IVnPayService vnPayService,IMomoService momoService,IEmailService emailService)
        {
            _context = context;
            _vnPayService = vnPayService;
            _momoService = momoService;
            _emailService = emailService;
        }

        // Phương thức kiểm tra và cập nhật trạng thái phòng
        private void CapNhatTrangThaiPhong(int maPhong)
        {
            var phongTro = _context.PhongTros.FirstOrDefault(p => p.MaPhong == maPhong);
            if (phongTro == null)
            {
                return;
            }

            var hopDongMoiNhat = _context.HopDongs
                .Where(hd => hd.MaPhong == maPhong && hd.TrangThai == "Đang hiệu lực")
                .OrderByDescending(hd => hd.NgayKy)
                .FirstOrDefault();

            if (hopDongMoiNhat == null)
            {
                phongTro.TrangThai = "Còn trống";
            }
            else
            {
                if (hopDongMoiNhat.NgayKetThuc < DateTime.Now)
                {
                    hopDongMoiNhat.TrangThai = "Hết hiệu lực";
                    phongTro.TrangThai = "Còn trống";
                }
                else
                {
                    phongTro.TrangThai = "Đã thuê";
                }
            }

            _context.SaveChanges();
        }

        // Action để hiển thị trang đặt phòng
        [HttpGet]
        public IActionResult DatPhong(int maPhong)
        {
            // Cập nhật trạng thái phòng trước khi hiển thị
            CapNhatTrangThaiPhong(maPhong);

            var phongTro = _context.PhongTros
                .Include(p => p.LoaiPhong)
                .Include(p => p.PhongTroThietBis)
                .ThenInclude(pttb => pttb.ThietBi)
                .Include(p => p.AnhPhongTros)
                .FirstOrDefault(p => p.MaPhong == maPhong);

            if (phongTro == null)
            {
                return NotFound();
            }

            
            // Kiểm tra người dùng đã đăng nhập chưa bằng cookie authentication
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Chưa đăng nhập để đặt phòng";
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }

            // Lấy MaNguoiDung từ Claims
            var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
            {
                TempData["ErrorMessage"] = "Không phải người dùng";
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }

            var nguoiDung = _context.NguoiDungs.FirstOrDefault(n => n.MaNguoiDung == maNguoiDung);
            if (nguoiDung == null)
            {
                TempData["ErrorMessage"] = "Không phải người dùng";
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }

            ViewData["NguoiDung"] = nguoiDung;
            return View(phongTro);
        }

        // Action để xử lý form đặt phòng
        [HttpPost]
        public async Task<IActionResult> DatPhong(int maPhong, DateTime ngayBatDau, DateTime ngayKetThuc, decimal tienCoc, string ghiChu, string name, double amount, 
           string orderDescription, string orderType, string PaymentMethod,double Amount ,string OrderID, string OrderInfor , string Email)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = true;
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }

            // Lấy MaNguoiDung từ Claims
            var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
            {
                TempData["ErrorMessage"] = true;
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }

            // Cập nhật trạng thái phòng trước khi xử lý đặt phòng
            CapNhatTrangThaiPhong(maPhong);

            // Kiểm tra trạng thái phòng
            var phongTro = _context.PhongTros
                .Include(p => p.LoaiPhong)
                .Include(p => p.PhongTroThietBis)
                .ThenInclude(pttb => pttb.ThietBi)
                .Include(p => p.AnhPhongTros)
                .FirstOrDefault(p => p.MaPhong == maPhong);

            if (phongTro == null)
            {
                return NotFound();
            }

            if (phongTro.TrangThai != "Còn trống")
            {
                TempData["ErrorMessage"] = "Phòng này đã được thuê, không thể đặt.";
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }

            // Kiểm tra giá thuê
            if (phongTro.GiaThue <= 0)
            {
                TempData["ErrorMessage"] = "Giá thuê không hợp lệ. Vui lòng liên hệ quản trị viên.";
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }

            // Kiểm tra ngày hợp lệ
            if (ngayKetThuc <= ngayBatDau)
            {
                TempData["ErrorMessage"] = "Ngày kết thúc phải lớn hơn ngày bắt đầu.";
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }

            // Kiểm tra Amount
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }

            // Tạo hợp đồng tạm thời (chưa lưu vào database)
            var hopDong = new HopDongModel
            {
                MaPhong = maPhong,
                MaNguoiDung = maNguoiDung,
                NgayBatDau = ngayBatDau,
                NgayKetThuc = ngayKetThuc,
                TienCoc = tienCoc,
                GhiChu = ghiChu ?? "Không có ghi chú",
                TrangThai = "Đang hiệu lực",
                NgayKy = DateTime.Now,
                PhuongThucThanhToan = "Chuyển khoản"
            };

            hopDong.PhongTro = null;
            hopDong.NguoiDung = null;
            // Lưu thông tin hợp đồng tạm thời vào TempData (hoặc có thể dùng Session)
            TempData["PendingHopDong"] = JsonConvert.SerializeObject(hopDong);
            TempData["PendingEmail"] = Email;

            // Tạo thông tin thanh toán VNPay
            var paymentInfo = new PaymentInformationModel
            {
                Name = name,
                Amount = amount, // Amount đã là double
                OrderDescription = orderDescription,
                CreatedDate = DateTime.Now,
                OrderType = orderType
            };

            var paymentInfor = new OrderInfoModel
            {
                FullName = name,
                Amount = Amount,
                OrderInfo = OrderInfor,
                OrderId = OrderID
            };


            if (PaymentMethod == "VNPay")
            {
                var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);
                return Redirect(paymentUrl);
            }
            else if (PaymentMethod == "MoMo")
            {
                var paymentUrlMomo = await _momoService.CreatePaymentMomo(paymentInfor);
                return Redirect(paymentUrlMomo.PayUrl);
            }
            else if (PaymentMethod == "AccountPaid")
            {
                var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
                if (nguoiDung == null)
                {
                    return Json(new { success = false, message = "Không thể xác định người dùng." });
                }

                if (hopDong == null || phongTro == null)
                {
                    return Json(new { success = false, message = "Không thể xác định phòng trọ." });
                }

                decimal paymentAmount = (decimal)amount;

                if (nguoiDung.SoDu == null || nguoiDung.SoDu < (decimal)Amount)
                {
                    decimal soTienConThieu = (decimal)Amount - (nguoiDung.SoDu);
                    return Json(new
                    {
                        success = false,
                        error = true,
                        message = $"Số dư tài khoản hiện tại ({(nguoiDung.SoDu.ToString("N0") ?? "0")} VNĐ) không đủ để thanh toán {Amount.ToString("N0")} VNĐ.",
                        redirectUrl = Url.Action("Index", "TrangCaNhan", new { tab = "deposit-history" }),
                        returnToUrl = Url.Action("DatPhong", new { maPhong }),
                        soTienConThieu = soTienConThieu
                    });
                }
                try
                {
                    // Kiểm tra MaNguoiDung
                    var nguoiDungExists = await _context.NguoiDungs.AnyAsync(n => n.MaNguoiDung == maNguoiDung);
                    if (!nguoiDungExists)
                    {
                        return Json(new { success = false, message = "Người dùng không tồn tại." });
                    }
                    
                    // Cập nhật số dư người dùng
                    nguoiDung.SoDu -= paymentAmount;
                    _context.NguoiDungs.Update(nguoiDung);

                    var magiaodich = Guid.NewGuid().ToString();

                    // Lưu hợp đồng
                    hopDong.TrangThai = "Đang hiệu lực";
                    hopDong.PhuongThucThanhToan = " Tài khoản " + magiaodich;
                    _context.HopDongs.Add(hopDong);

                    // Lưu tất cả thay đổi trước khi kiểm tra
                    await _context.SaveChangesAsync();

                    // Cập nhật trạng thái phòng
                    phongTro.TrangThai = "Đã thuê";
                    _context.PhongTros.Update(phongTro);

                    // Lưu giao dịch vào bảng AccountPaidInfor
                    var newAccountPaid = new AccountPaidModel
                    {
                        TransactionId = magiaodich,
                        MaNguoiDung = maNguoiDung,
                        MaHopDong = hopDong.MaHopDong,
                        MaHoaDon = null,
                        OrderDescription = orderDescription ?? "Thanh toán đặt phòng bằng số dư tài khoản",
                        PaymentMethod = PaymentMethod ?? "AccountPaid",
                        Amount = (double)paymentAmount,
                        DateCreated = DateTime.Now,
                    };
                    _context.AccountPaidInfor.Add(newAccountPaid);

                    await _context.SaveChangesAsync();
                    // Gửi email thông báo
                    var email = TempData["PendingEmail"] as string;
                    if (string.IsNullOrEmpty(email))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy email người dùng để gửi thông báo.";
                    }
                    else
                    {
                        var subject = "Xác nhận thanh toán thành công - Đặt phòng trọ";
                        var body = $@"<h2>Thanh toán thành công!</h2>
                                <p>Cảm ơn bạn đã đặt phòng trọ. Dưới đây là thông tin thanh toán:</p>
                                <ul>
                                    <li><strong>Mã giao dịch:</strong> {magiaodich}</li>
                                    <li><strong>Số tiền:</strong> {(paymentAmount).ToString("N0")} VNĐ</li>
                                    <li><strong>Phương thức thanh toán:</strong> Thanh toán bằng tài khoản web</li>
                                    <li><strong>Mô tả đơn hàng:</strong> {orderDescription}</li>
                                    <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                                    <li><strong>Mã phòng:</strong> {hopDong.MaPhong}</li>
                                </ul>
                                <p>Chúc bạn có trải nghiệm tuyệt vời!</p>";

                        try
                        {
                            await _emailService.SendEmailAsync(email, subject, body);
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = $"Gửi email thất bại: {ex.Message}";
                        }
                    }

                    TempData["SuccessMessage"] = "Đặt phòng thành công!";
                    return Json(new
                    {
                        success = true,
                        message = "Đặt phòng thành công!",
                        redirectUrl = Url.Action("ChiTietPhong", "PhongTro", new { id = maPhong })
                    });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Đặt không thành công phòng thành công";
                    var innerException = ex.InnerException?.Message ?? "Không có thông tin chi tiết.";
                    return Json(new { success = false, message = $"Có lỗi xảy ra khi xử lý thanh toán: {ex.Message}. Chi tiết: {innerException}" });
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Đặt không thành công phòng thành công";
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }
        }
    }
}