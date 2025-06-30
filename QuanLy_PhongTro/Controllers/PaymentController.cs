using Microsoft.AspNetCore.Mvc;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Models.Vnpay;
using QuanLy_PhongTro.Services.Vnpay;
using Newtonsoft.Json;
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.Services.Momo;
using QuanLy_PhongTro.Models.Order;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Services;
using System;
using System.Security.Claims;

namespace QuanLy_PhongTro.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        private readonly DataContext _context;
        private readonly IEmailService _emailService;

        public PaymentController(IMomoService momoService,IVnPayService vnPayService,IEmailService emailService, DataContext context)
        {
            _momoService = momoService;
            _vnPayService = vnPayService;
            _emailService = emailService;
            _context = context;
        }   
        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model, string transactionType = "Booking")
        {
            Console.WriteLine($"CreatePaymentMomo called for {transactionType} with model: {JsonConvert.SerializeObject(model)}");
            var response = await _momoService.CreatePaymentMomo(model, transactionType);
            Console.WriteLine($"MoMo Response: {JsonConvert.SerializeObject(response)}");
            return Redirect(response.PayUrl);
        }

        [HttpGet]
        [Route("Payment/PaymentCallBackMomo")]
        public async Task<IActionResult> PaymentCallBackMomo()  
        {
            var response =  _momoService.PaymentExecuteAsync(Request.Query);

            if (response.Success && response.ErrorCode == 0) // Thanh toán thành công
            {
                var pendingHopDongJson = TempData["PendingHopDong"]?.ToString();
                if (string.IsNullOrEmpty(pendingHopDongJson))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin hợp đồng.";
                    return RedirectToAction("Index", "PhongTro");
                }
                var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng";
                    return RedirectToAction("ChiTietPhong", "PhongTro");
                }

                var hopDong = JsonConvert.DeserializeObject<HopDongModel>(pendingHopDongJson);
                hopDong.PhuongThucThanhToan = "Momo " + response.TransId;
                hopDong.TrangThai = "Đang hiệu lực";

                _context.HopDongs.Add(hopDong);
                await _context.SaveChangesAsync();

                var momoModel = new MomoModel
                {
                    OrderId = response.OrderId,
                    TransactionId = response.TransId,
                    Amount = double.Parse(response.Amount),
                    OrderInfo = response.OrderInfo,
                    PaymentMethod = "Momo",
                    CustomerId = maNguoiDung,
                    CreatedDate = DateTime.Now,
                    MaHopDong = hopDong.MaHopDong
                };

                _context.MomoInfor.Add(momoModel);
                await _context.SaveChangesAsync();

                var phongTro = await _context.PhongTros.FirstOrDefaultAsync(p => p.MaPhong == hopDong.MaPhong);
                if (phongTro != null)
                {
                    phongTro.TrangThai = "Đã thuê";
                    await _context.SaveChangesAsync();
                }

                
                // Lấy email từ TempData
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
                            <li><strong>Mã đơn hàng:</strong> {response.OrderId}</li>
                            <li><strong>Mã giao dịch:</strong> {response.TransId}</li>
                            <li><strong>Số tiền:</strong> {double.Parse(response.Amount).ToString("N0")} VNĐ</li>
                            <li><strong>Phương thức thanh toán:</strong> Momo</li>
                            <li><strong>Mô tả đơn hàng:</strong> {response.OrderInfo}</li>
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

                var paymentInfo = new
                {
                    OrderId = response.OrderId,
                    TransactionId = response.TransId,
                    Amount = double.Parse(response.Amount),
                    OrderInfo = response.OrderInfo,
                    PaymentMethod = "Momo",
                    CreatedDate = DateTime.Now,
                    MaPhong = hopDong.MaPhong
                };

                TempData["SuccessMessage"] = "Thanh toán và đặt phòng thành công!";
                return View("PaymentSuccessMomo", paymentInfo);
            }
            else
            {
                TempData["ErrorMessage"] = "Thanh toán thất bại. Vui lòng thử lại.";
                var pendingHopDongJson = TempData["PendingHopDong"]?.ToString();
                var maPhong = pendingHopDongJson != null ? JsonConvert.DeserializeObject<HopDongModel>(pendingHopDongJson)?.MaPhong : 0;
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }
        }

        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model, string transactionType = "Booking")
        {
            Console.WriteLine($"CreatePaymentUrlVnpay called for {transactionType} with model: {JsonConvert.SerializeObject(model)}");
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext, transactionType);
            Console.WriteLine($"VNPay PaymentUrl: {url}");
            return Redirect(url);
        }

        [HttpGet]
        [Route("Payment/PaymentCallbackVnpay")]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success && response.VnPayResponseCode == "00") // Thanh toán thành công
            {
                // Lấy thông tin hợp đồng từ TempData
                var pendingHopDongJson = TempData["PendingHopDong"]?.ToString();
                if (string.IsNullOrEmpty(pendingHopDongJson))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin hợp đồng.";
                    return RedirectToAction("Index", "PhongTro");
                }
                var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng";
                    return RedirectToAction("ChiTietPhong", "PhongTro");
                }

                var hopDong = JsonConvert.DeserializeObject<HopDongModel>(pendingHopDongJson);
                var Paymentmethod = response.PaymentMethod;
                var tranid = response.TransactionId;
                hopDong.PhuongThucThanhToan = Paymentmethod +" "+ tranid;

                // Cập nhật trạng thái hợp đồng
                hopDong.TrangThai = "Đang hiệu lực";

                // Lưu hợp đồng vào database
                _context.HopDongs.Add(hopDong);
                _context.SaveChanges();

                var newVnPay = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    Amount = response.Amount,
                    CustomerId = maNguoiDung,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now,
                    MaHopDong = hopDong.MaHopDong
                };

                _context.VnPayInfor.Add(newVnPay);
                _context.SaveChanges();

                // Cập nhật trạng thái phòng
                var phongTro = _context.PhongTros.FirstOrDefault(p => p.MaPhong == hopDong.MaPhong);
                if (phongTro != null)
                {
                    phongTro.TrangThai = "Đã thuê";
                    _context.SaveChanges();
                }

                // Lấy email từ TempData
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
                            <li><strong>Mã đơn hàng:</strong> {response.OrderId}</li>
                            <li><strong>Mã giao dịch:</strong> {response.TransactionId}</li>
                            <li><strong>Phương thức thanh toán:</strong> {response.PaymentMethod}</li>
                            <li><strong>Mô tả đơn hàng:</strong> {response.OrderDescription}</li>
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

                var paymentInfo = new
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now,
                    MaPhong = hopDong.MaPhong
                };
                TempData["SuccessMessage"] = "Thanh toán và đặt phòng thành công!";
                //return RedirectToAction("ChiTietPhong", "PhongTro", new { id = hopDong.MaPhong });
                return View("PaymentSuccessVnPay", paymentInfo);
            }
            else
            {
                TempData["ErrorMessage"] = "Thanh toán thất bại. Vui lòng thử lại.";
                var pendingHopDongJson = TempData["PendingHopDong"]?.ToString();
                var maPhong = pendingHopDongJson != null ? JsonConvert.DeserializeObject<HopDongModel>(pendingHopDongJson)?.MaPhong : 0;
                return RedirectToAction("ChiTietPhong", "PhongTro", new { id = maPhong });
            }
        }

        [HttpGet]
        [Route("Payment/NapTienCallbackVnpay")]
        public async Task<IActionResult> NapTienCallbackVnpay(string returnToUrl = null)
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success && response.VnPayResponseCode == "00") // Thanh toán thành công
            {
                returnToUrl = TempData["ReturnToUrl"] as string;
                // Lấy thông tin nạp tiền từ TempData
                var pendingNapTienJson = TempData["PendingNapTien"]?.ToString();
                if (string.IsNullOrEmpty(pendingNapTienJson))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin nạp tiền.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "deposit-history" });
                }

                var napTienInfo = JsonConvert.DeserializeObject<NapTienModel>(pendingNapTienJson);
                var nguoiDung = await _context.NguoiDungs.FindAsync(napTienInfo.MaNguoiDung);
                if (nguoiDung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "deposit-history" });
                }

                // Cập nhật số dư người dùng
                nguoiDung.SoDu += (decimal)napTienInfo.Amount;
                _context.Update(nguoiDung);

                // Lưu giao dịch VNPay
                var vnpayModel = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    Amount = response.Amount,
                    CustomerId = nguoiDung.MaNguoiDung,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now,
                };

                _context.VnPayInfor.Add(vnpayModel);
                await _context.SaveChangesAsync();

                // Gửi email xác nhận
                var email = TempData["PendingEmail"] as string;
                if (!string.IsNullOrEmpty(email))
                {
                    var subject = "Xác nhận nạp tiền thành công";
                    var body = $@"<h2>Nạp tiền thành công!</h2>
                <p>Cảm ơn bạn đã nạp tiền vào tài khoản. Dưới đây là thông tin giao dịch:</p>
                <ul>
                    <li><strong>Mã đơn hàng:</strong> {response.OrderId}</li>
                    <li><strong>Mã giao dịch:</strong> {response.TransactionId}</li>
                    <li><strong>Số tiền:</strong> {response.Amount.ToString("N0")} VNĐ</li>
                    <li><strong>Phương thức thanh toán:</strong> VNPay</li>
                    <li><strong>Mô tả đơn hàng:</strong> {response.OrderDescription}</li>
                    <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                </ul>
                <p>Số dư mới của bạn: {nguoiDung.SoDu.ToString("N0")} VNĐ</p>";

                    try
                    {
                        await _emailService.SendEmailAsync(email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Gửi email thất bại: {ex.Message}";
                    }
                }
                if (!string.IsNullOrEmpty(returnToUrl))
                {
                    TempData["SuccessMessage"] = "Nạp tiền thành công! Vui lòng tiếp tục thanh toán .";
                    return Redirect(returnToUrl);
                }

                TempData["SuccessMessage"] = "Nạp tiền thành công!";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "deposit-history" });
            }
            else
            {
                TempData["ErrorMessage"] = "Nạp tiền thất bại. Vui lòng thử lại.";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "deposit-history" });
            }
        }

        [HttpGet]
        [Route("Payment/NapTienCallbackMomo")]
        public async Task<IActionResult> NapTienCallbackMomo(string returnToUrl = null)
        {
            var response = _momoService.PaymentExecuteAsync(Request.Query);

            if (response.Success && response.ErrorCode == 0) // Thanh toán thành công
            {
                returnToUrl = TempData["ReturnToUrl"] as string;
                // Lấy thông tin nạp tiền từ TempData
                var pendingNapTienJson = TempData["PendingNapTien"]?.ToString();
                if (string.IsNullOrEmpty(pendingNapTienJson))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin nạp tiền.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "deposit-history" });
                }

                var napTienInfo = JsonConvert.DeserializeObject<NapTienModel>(pendingNapTienJson);
                var nguoiDung = await _context.NguoiDungs.FindAsync(napTienInfo.MaNguoiDung);
                if (nguoiDung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "deposit-history" });
                }

                // Cập nhật số dư người dùng
                nguoiDung.SoDu += (decimal)napTienInfo.Amount;
                _context.Update(nguoiDung);

                // Lưu giao dịch MoMo
                var momoModel = new MomoModel
                {
                    OrderId = response.OrderId,
                    TransactionId = response.TransId,
                    Amount = double.Parse(response.Amount),
                    OrderInfo = response.OrderInfo,
                    CustomerId = nguoiDung.MaNguoiDung,
                    PaymentMethod = "Momo",
                    CreatedDate = DateTime.Now,
                };

                _context.MomoInfor.Add(momoModel);
                await _context.SaveChangesAsync();

                // Gửi email xác nhận
                var email = TempData["PendingEmail"] as string;
                if (!string.IsNullOrEmpty(email))
                {
                    var subject = "Xác nhận nạp tiền thành công";
                    var body = $@"<h2>Nạp tiền thành công!</h2>
                <p>Cảm ơn bạn đã nạp tiền vào tài khoản. Dưới đây là thông tin giao dịch:</p>
                <ul>
                    <li><strong>Mã đơn hàng:</strong> {response.OrderId}</li>
                    <li><strong>Mã giao dịch:</strong> {response.TransId}</li>
                    <li><strong>Số tiền:</strong> {double.Parse(response.Amount).ToString("N0")} VNĐ</li>
                    <li><strong>Phương thức thanh toán:</strong> Momo</li>
                    <li><strong>Mô tả đơn hàng:</strong> {response.OrderInfo}</li>
                    <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                </ul>
                <p>Số dư mới của bạn: {nguoiDung.SoDu.ToString("N0")} VNĐ</p>";

                    try
                    {
                        await _emailService.SendEmailAsync(email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Gửi email thất bại: {ex.Message}";
                    }
                }       
                if (!string.IsNullOrEmpty(returnToUrl))
                {
                    TempData["SuccessMessage"] = "Nạp tiền thành công! Vui lòng tiếp tục thanh toán .";
                    return Redirect(returnToUrl);
                }

                TempData["SuccessMessage"] = "Nạp tiền thành công!";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "deposit-history" });
            }
            else
            {
                TempData["ErrorMessage"] = "Nạp tiền thất bại. Vui lòng thử lại.";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "deposit-history" });
            }
        }
            
        [HttpGet]
        [Route("Payment/PayBillCallbackMomo")]
        public async Task<IActionResult> PayBillCallbackMomo(string returnToUrl = null)
        {
            var response = _momoService.PaymentExecuteAsync(Request.Query);

            if (response.Success && response.ErrorCode == 0) // Thanh toán thành công
            {
                returnToUrl = TempData["ReturnToUrl"] as string ?? Url.Action("PayBill", "PayBill");
                var maHoaDon = TempData["PendingHoaDon"]?.ToString();
                if (string.IsNullOrEmpty(maHoaDon) || !int.TryParse(maHoaDon, out int hoaDonId))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin hóa đơn.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
                }

                // Lấy maNguoiDung trước và xử lý null
                var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
                {
                    TempData["ErrorMessage"] = "Không thể xác định người dùng.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
                }

                var hoaDon = await _context.HoaDon
                    .Include(h => h.HopDong)
                    .FirstOrDefaultAsync(h => h.MaHoaDon == hoaDonId && h.HopDong.MaNguoiDung == maNguoiDung);

                if (hoaDon == null)
                {
                    TempData["ErrorMessage"] = "Hóa đơn không tồn tại.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
                }

                var nguoiDung = await _context.NguoiDungs.FindAsync(hoaDon.HopDong.MaNguoiDung);
                if (nguoiDung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
                }

                

                var momoModel = new MomoModel
                {
                    OrderId = response.OrderId,
                    TransactionId = response.TransId,
                    Amount = double.Parse(response.Amount),
                    OrderInfo = response.OrderInfo,
                    CustomerId = nguoiDung.MaNguoiDung,
                    PaymentMethod = "Momo",
                    CreatedDate = DateTime.Now,
                    MaHopDong = null // Sửa lại để lưu MaHoaDon thay vì MaHopDong
                };

                _context.MomoInfor.Add(momoModel);
                await _context.SaveChangesAsync();

                hoaDon.MomoId = momoModel.MomoId;
                hoaDon.TrangThai = "Đã thanh toán";
                _context.HoaDon.Update(hoaDon);
                await _context.SaveChangesAsync();

                var email = TempData["PendingEmail"] as string;
                if (!string.IsNullOrEmpty(email))
                {
                    var subject = "Xác nhận thanh toán hóa đơn thành công";
                    var body = $@"<h2>Thanh toán thành công!</h2>
                        <p>Cảm ơn bạn đã thanh toán hóa đơn. Dưới đây là thông tin giao dịch:</p>
                        <ul>
                            <li><strong>Mã đơn hàng:</strong> {response.OrderId}</li>
                            <li><strong>Mã giao dịch:</strong> {response.TransId}</li>
                            <li><strong>Số tiền:</strong> {double.Parse(response.Amount).ToString("N0")} VNĐ</li>
                            <li><strong>Phương thức thanh toán:</strong> Momo</li>
                            <li><strong>Mô tả đơn hàng:</strong> {response.OrderInfo}</li>
                            <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                            <li><strong>Mã hóa đơn:</strong> {hoaDon.MaHoaDon}</li>
                        </ul>
                        <p>Chúc bạn có trải nghiệm tuyệt vời!</p>";

                    try
                    {
                        await _emailService.SendEmailAsync(email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Gửi email thất bại: {ex.Message}";
                        return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
                    }
                }

                TempData["SuccessMessage"] = "Thanh toán hóa đơn thành công!";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
            }
            else
            {
                TempData["ErrorMessage"] = "Thanh toán thất bại. Vui lòng thử lại.";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
            }
        }

        [HttpGet]
        [Route("Payment/PayBillCallbackVnpay")]
        public async Task<IActionResult> PayBillCallbackVnpay(string returnToUrl = null)
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success && response.VnPayResponseCode == "00") // Thanh toán thành công
            {
                returnToUrl = TempData["ReturnToUrl"] as string ?? Url.Action("PayBill", "PayBill");
                var maHoaDon = TempData["PendingHoaDon"]?.ToString();
                if (string.IsNullOrEmpty(maHoaDon) || !int.TryParse(maHoaDon, out int hoaDonId))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin hóa đơn.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
                }

                // Lấy maNguoiDung trước và xử lý null
                var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
                {
                    TempData["ErrorMessage"] = "Không thể xác định người dùng.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
                }

                var hoaDon = await _context.HoaDon
                    .Include(h => h.HopDong)
                    .FirstOrDefaultAsync(h => h.MaHoaDon == hoaDonId && h.HopDong.MaNguoiDung == maNguoiDung);

                if (hoaDon == null)
                {
                    TempData["ErrorMessage"] = "Hóa đơn không tồn tại.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
                }

                var nguoiDung = await _context.NguoiDungs.FindAsync(hoaDon.HopDong.MaNguoiDung);
                if (nguoiDung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
                }

                

                var vnpayModel = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    Amount = response.Amount,
                    CustomerId = nguoiDung.MaNguoiDung,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now,
                    MaHopDong = null 
                };

                _context.VnPayInfor.Add(vnpayModel);
                await _context.SaveChangesAsync();

                hoaDon.VnpayId = vnpayModel.VnpayId;
                hoaDon.TrangThai = "Đã thanh toán";
                _context.HoaDon.Update(hoaDon);
                await _context.SaveChangesAsync();

                var email = TempData["PendingEmail"] as string;
                if (!string.IsNullOrEmpty(email))
                {
                    var subject = "Xác nhận thanh toán hóa đơn thành công";
                    var body = $@"<h2>Thanh toán thành công!</h2>
                                <p>Cảm ơn bạn đã thanh toán hóa đơn. Dưới đây là thông tin giao dịch:</p>
                                <ul>
                                    <li><strong>Mã đơn hàng:</strong> {response.OrderId}</li>
                                    <li><strong>Mã giao dịch:</strong> {response.TransactionId}</li>
                                    <li><strong>Số tiền:</strong> {response.Amount.ToString("N0")} VNĐ</li>
                                    <li><strong>Phương thức thanh toán:</strong> VNPay</li>
                                    <li><strong>Mô tả đơn hàng:</strong> {response.OrderDescription}</li>
                                    <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                                    <li><strong>Mã hóa đơn:</strong> {hoaDon.MaHoaDon}</li>
                                </ul>
                                <p>Chúc bạn có trải nghiệm tuyệt vời!</p>";

                    try
                    {
                        await _emailService.SendEmailAsync(email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Gửi email thất bại: {ex.Message}";
                        return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
                    }
                }

                TempData["SuccessMessage"] = "Thanh toán hóa đơn thành công!";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
            }
            else
            {
                TempData["ErrorMessage"] = "Thanh toán thất bại. Vui lòng thử lại.";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "days-receipt" });
            }
        }

        [HttpGet]
        [Route("Payment/PayContractCallbackMomo")]
        public async Task<IActionResult> PayContractCallbackMomo(string returnToUrl = null)
        {
            try
            {
                var response =  _momoService.PaymentExecuteAsync(Request.Query);
                Console.WriteLine($"MoMo Callback: Success={response.Success}, ErrorCode={response.ErrorCode}, Message={response.Message}, OrderId={response.OrderId}, TransId={response.TransId}");

                if (response.Success && response.ErrorCode == 0)
                {
                    returnToUrl = TempData["ReturnToUrl"] as string ?? Url.Action("PayContract", "PayContract");
                    var maHopDongStr = TempData["PendingHopDongN"]?.ToString();
                    if (string.IsNullOrEmpty(maHopDongStr) || !int.TryParse(maHopDongStr, out int hopDongId))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy thông tin hợp đồng.";
                        return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
                    }

                    var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
                    {
                        TempData["ErrorMessage"] = "Không thể xác định người dùng.";
                        return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
                    }

                    var hopDong = await _context.HopDongs
                        .Include(h => h.NguoiDung)
                        .FirstOrDefaultAsync(h => h.MaHopDong == hopDongId && h.MaNguoiDung == maNguoiDung);

                    if (hopDong == null)
                    {
                        TempData["ErrorMessage"] = "Hợp đồng không tồn tại.";
                        return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
                    }

                    var nguoiDung = await _context.NguoiDungs.FindAsync(hopDong.MaNguoiDung);
                    if (nguoiDung == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                        return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
                    }

                    var momoModel = new MomoModel
                    {
                        OrderId = response.OrderId,
                        TransactionId = response.TransId,
                        Amount = double.TryParse(response.Amount, out var amount) ? amount : 0,
                        OrderInfo = response.OrderInfo,
                        CustomerId = nguoiDung.MaNguoiDung,
                        PaymentMethod = "MoMo",
                        CreatedDate = DateTime.Now,
                        MaHopDong = hopDong.MaHopDong
                    };

                    _context.MomoInfor.Add(momoModel);
                    await _context.SaveChangesAsync();

                    hopDong.PhuongThucThanhToan = "MoMo " + momoModel.TransactionId;
                    _context.HopDongs.Update(hopDong);
                    await _context.SaveChangesAsync();

                    var email = TempData["PendingEmail"] as string ?? nguoiDung.Email;
                    if (!string.IsNullOrEmpty(email))
                    {
                        var subject = "Xác nhận thanh toán hợp đồng thành công";
                        var body = $@"<h2>Thanh toán thành công!</h2>
                <p>Cảm ơn bạn đã thanh toán hợp đồng. Dưới đây là thông tin giao dịch:</p>
                <ul>
                    <li><strong>Mã đơn hàng:</strong> {response.OrderId}</li>
                    <li><strong>Mã giao dịch:</strong> {response.TransId}</li>
                    <li><strong>Số tiền:</strong> {(double.TryParse(response.Amount, out var emailAmount) ? emailAmount : 0).ToString("N0")} VNĐ</li>
                    <li><strong>Phương thức thanh toán:</strong> MoMo</li>
                    <li><strong>Mô tả đơn hàng:</strong> {response.OrderInfo}</li>
                    <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                    <li><strong>Mã hợp đồng:</strong> {hopDong.MaHopDong}</li>
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

                    TempData["SuccessMessage"] = "Thanh toán hợp đồng thành công!";
                    Console.WriteLine("Redirecting to TrangCaNhan with tab=room-book");
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
                }
                else
                {
                    TempData["ErrorMessage"] = $"Thanh toán thất bại. Mã lỗi: {response.ErrorCode}. Thông báo: {response.Message}";
                    Console.WriteLine("Redirecting to TrangCaNhan with tab=room-book (Failed)");
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi trong quá trình xử lý thanh toán: {ex.Message}";
                Console.WriteLine($"Error in PayContractCallbackMomo: {ex.Message}");
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
            }
        }

        [HttpGet]
        [Route("Payment/PayContractCallbackVnpay")]
        public async Task<IActionResult> PayContractCallbackVnpay(string returnToUrl = null)
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success && response.VnPayResponseCode == "00") // Thanh toán thành công
            {
                returnToUrl = TempData["ReturnToUrl"] as string ?? Url.Action("PayContract", "PayContract");
                var maHopDongStr = TempData["PendingHopDongN"]?.ToString();
                if (maHopDongStr == null || !int.TryParse(maHopDongStr, out int hopDongId))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin hợp đồng.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
                }

                var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
                {
                    TempData["ErrorMessage"] = "Không thể xác định người dùng.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
                }

                var hopDong = await _context.HopDongs
                    .Include(h => h.NguoiDung)
                    .FirstOrDefaultAsync(h => h.MaHopDong == hopDongId && h.MaNguoiDung == maNguoiDung);

                if (hopDong == null)
                {
                    TempData["ErrorMessage"] = "Hợp đồng không tồn tại.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
                }

                var nguoiDung = await _context.NguoiDungs.FindAsync(hopDong.MaNguoiDung);
                if (nguoiDung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
                }

                var vnpayModel = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = "VNPay",
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    Amount = response.Amount / 100, // Đảm bảo chia cho 100 nếu VNPay trả về số tiền * 100
                    CustomerId = nguoiDung.MaNguoiDung,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now,
                    MaHopDong = hopDong.MaHopDong
                };

                _context.VnPayInfor.Add(vnpayModel);
                await _context.SaveChangesAsync();

                hopDong.PhuongThucThanhToan = "VNPay " + vnpayModel.TransactionId;
                _context.HopDongs.Update(hopDong);
                await _context.SaveChangesAsync();

                var email = TempData["PendingEmail"] as string ?? nguoiDung.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    var subject = "Xác nhận thanh toán hợp đồng thành công";
                    var body = $@"<h2>Thanh toán thành công!</h2>
                <p>Cảm ơn bạn đã thanh toán hợp đồng. Dưới đây là thông tin giao dịch:</p>
                <ul>
                    <li><strong>Mã đơn hàng:</strong> {response.OrderId}</li>
                    <li><strong>Mã giao dịch:</strong> {response.TransactionId}</li>
                    <li><strong>Số tiền:</strong> {(response.Amount / 100).ToString("N0")} VNĐ</li>
                    <li><strong>Phương thức thanh toán:</strong> VNPay</li>
                    <li><strong>Mô tả đơn hàng:</strong> {response.OrderDescription}</li>
                    <li><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                    <li><strong>Mã hợp đồng:</strong> {hopDong.MaHopDong}</li>
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

                TempData["SuccessMessage"] = "Thanh toán hợp đồng thành công!";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
            }
            else
            {
                TempData["ErrorMessage"] = $"Thanh toán thất bại. Mã lỗi: {response.VnPayResponseCode}";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "room-book" });
            }
        }
    }
    
}