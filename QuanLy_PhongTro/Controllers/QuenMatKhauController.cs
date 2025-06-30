using Microsoft.AspNetCore.Mvc;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.Services;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using QuanLy_PhongTro.ViewModel;

namespace QuanLy_PhongTro.Controllers
{
    public class QuenMatKhauController : Controller
    {
        private readonly DataContext _context;
        private readonly IEmailService _emailService;

        public QuenMatKhauController(DataContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Hiển thị form Quên mật khẩu
        public IActionResult Index()
        {
            return View("~/Views/Account/QuenMatKhau.cshtml");
        }

        // Xử lý yêu cầu gửi OTP
        [HttpPost]
        public async Task<IActionResult> GuiOTP(string email)
        {
            
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "Vui lòng nhập email" });
                }

                var nguoiDung = _context.NguoiDungs.FirstOrDefault(u => u.Email == email);
                if (nguoiDung == null)
                {
                    return Json(new { success = false, message = "Email không tồn tại" });
                }

                // Tạo OTP ngẫu nhiên (6 chữ số)
                var otp = new Random().Next(100000, 999999).ToString();

                // Lưu OTP và thời gian hết hạn vào Session
                HttpContext.Session.SetString("OTP", otp);
                HttpContext.Session.SetString("OTP_Email", email);
                HttpContext.Session.SetString("OTP_Expires", DateTime.Now.AddMinutes(5).ToString()); // OTP hết hạn sau 5 phút

                var subject = "Mã OTP để đặt lại mật khẩu";
                var body = $@"
                        <h3>Xin chào bạn,</h3>
                        <p>Mã OTP của bạn là: <strong>{otp}</strong></p>
                        <p>Mã này có hiệu lực trong 5 phút.</p>
                    ";
                await _emailService.SendEmailAsync(email, subject, body);

                return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Hiển thị form nhập OTP
        public IActionResult XacNhanOTP()
        {
            return View("~/Views/Account/XacNhanOTP.cshtml");
        }

        // Xử lý xác nhận OTP
        [HttpPost]
        public IActionResult XacNhanOTP([FromBody] string otp)
        {
            try
            {
                // Debug: Log session values
                var debugInfo = new
                {
                    SessionOTP = HttpContext.Session.GetString("OTP"),
                    SessionExpires = HttpContext.Session.GetString("OTP_Expires"),
                    InputOTP = otp
                };
                Console.WriteLine($"Debug OTP: {JsonSerializer.Serialize(debugInfo)}");

                // Kiểm tra session OTP
                var storedOtp = HttpContext.Session.GetString("OTP");
                if (string.IsNullOrEmpty(storedOtp))
                {
                    return Json(new { success = false, message = "Mã OTP không tồn tại hoặc đã hết hạn" });
                }

                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(otp))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã OTP" });
                }

                // Kiểm tra thời gian hết hạn
                if (!DateTime.TryParse(HttpContext.Session.GetString("OTP_Expires"), out var otpExpires))
                {
                    return Json(new { success = false, message = "Lỗi hệ thống khi kiểm tra thời gian OTP" });
                }

                if (DateTime.Now > otpExpires)
                {
                    return Json(new { success = false, message = "Mã OTP đã hết hạn" });
                }

                // So sánh OTP (loại bỏ khoảng trắng và so sánh chính xác)
                if (!string.Equals(otp.Trim(), storedOtp.Trim(), StringComparison.Ordinal))
                {
                    return Json(new { success = false, message = "Mã OTP không đúng" });
                }

                // Xác nhận thành công
                HttpContext.Session.Remove("OTP");
                HttpContext.Session.Remove("OTP_Expires");

                return Json(new
                {
                    success = true,
                    message = "Xác nhận OTP thành công",
                    redirectUrl = Url.Action("DatLaiMatKhau", "QuenMatKhau")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // Hiển thị form đặt lại mật khẩu
        public IActionResult DatLaiMatKhau()
        {
            return View("~/Views/Account/DatLaiMatKhau.cshtml");
        }

        // Xử lý đặt lại mật khẩu
        [HttpPost]
        public IActionResult DatLaiMatKhau([FromBody] ResetPasswordModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model?.newPassword))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mật khẩu mới" });
                }

                var email = HttpContext.Session.GetString("OTP_Email");
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "Phiên làm việc đã hết hạn. Vui lòng thử lại." });
                }

                var nguoiDung = _context.NguoiDungs.FirstOrDefault(u => u.Email == email);
                if (nguoiDung == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng" });
                }

                // Cập nhật mật khẩu mới
                nguoiDung.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.newPassword);
                _context.SaveChanges();

                // Xóa email khỏi session
                HttpContext.Session.Remove("OTP_Email");

                return Json(new
                {
                    success = true,
                    message = "Đặt lại mật khẩu thành công",
                    redirectUrl = Url.Action("Index", "DangNhap")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}