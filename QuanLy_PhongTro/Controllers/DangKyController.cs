using Microsoft.AspNetCore.Mvc;
using QuanLy_PhongTro.Models;
using BCrypt.Net;
// Bỏ using Microsoft.AspNetCore.Http; nếu không dùng trực tiếp
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.ViewModels; // *** THÊM USING NÀY ***
using Microsoft.EntityFrameworkCore; // Cho AnyAsync

namespace QuanLy_PhongTro.Controllers
{
    public class DangKyController : Controller
    {
        private readonly DataContext _context;

        public DangKyController(DataContext context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            // Trả về View với đường dẫn đúng
            return View("~/Views/Account/DangKy.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel viewModel)
        {
            // Ghi log để kiểm tra viewModel nhận được (tùy chọn)
            Console.WriteLine($"Received ViewModel: Email={viewModel.Email}");

            // *** KIỂM TRA MODELSTATE ***
            // ModelState.IsValid sẽ tự động kiểm tra các [Required], [EmailAddress], [Compare]... trên ViewModel
            if (!ModelState.IsValid)
            {
                // Lấy lỗi validation từ ModelState để trả về cho client
                var errors = ModelState.ToDictionary(
                   kvp => kvp.Key,
                   kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );
                string firstErrorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ.";
                // Trả về lỗi có cấu trúc để JavaScript hiển thị đúng chỗ
                return Json(new { success = false, message = firstErrorMessage, errors = errors });
            }

            // *** Không cần kiểm tra mật khẩu khớp thủ công nữa, [Compare] đã làm việc đó ***

            // Kiểm tra email đã tồn tại
            if (await _context.NguoiDungs.AnyAsync(u => u.Email == viewModel.Email.Trim()))
            {
                // Trả lỗi cụ thể cho trường Email
                return Json(new { success = false, message = "Email đã được sử dụng.", errors = new { Email = new[] { "Email đã được sử dụng." } } });
            }

            // Kiểm tra số điện thoại đã tồn tại
            if (await _context.NguoiDungs.AnyAsync(u => u.SoDienThoai == viewModel.SoDienThoai.Trim()))
            {
                // Trả lỗi cụ thể cho trường SoDienThoai
                return Json(new { success = false, message = "Số điện thoại đã được sử dụng.", errors = new { SoDienThoai = new[] { "Số điện thoại đã được sử dụng." } } });
            }

            // *** TẠO ĐỐI TƯỢNG NguoiDungModel TỪ ViewModel ***
            // Chỉ lấy các trường cần thiết để lưu vào DB, KHÔNG lấy ConfirmPassword
            var newUser = new NguoiDungModel
            {
                HoTen = viewModel.HoTen.Trim(),
                Email = viewModel.Email.Trim(),
                SoDienThoai = viewModel.SoDienThoai.Trim(),
                // Mã hóa mật khẩu từ ViewModel
                MatKhau = BCrypt.Net.BCrypt.HashPassword(viewModel.MatKhau), // Trim đã làm ở JS hoặc ViewModel rồi, hash luôn
                MaQuyen = viewModel.MaQuyen,
                NgayTao = DateTime.Now
            };

            // Thêm người dùng vào cơ sở dữ liệu
            _context.NguoiDungs.Add(newUser);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đăng ký thành công! Vui lòng đăng nhập." });
        }
    }
}