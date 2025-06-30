using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.ViewModel;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/NguoiDung")]
    public class NguoiDungController : Controller
    {
        private readonly INguoiDungRepositories _nguoiDungRepository;

        public NguoiDungController(INguoiDungRepositories nguoiDungRepository)
        {
            _nguoiDungRepository = nguoiDungRepository;
        }

        // Action để render view với danh sách người dùng
        public async Task<IActionResult> Index()
        {
            var users = await _nguoiDungRepository.GetAllAsync();
            return View(users);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _nguoiDungRepository.GetAllAsync();

                if (users == null || !users.Any())
                {
                    return Json(new
                    {
                        success = true,
                        message = "Không có người dùng nào",
                        data = new List<object>()
                    });
                }

                var result = users.Select(u => new
                {
                    u.MaNguoiDung,
                    HoTen = u.HoTen ?? "Không có",
                    Email = u.Email ?? "Không có",
                    SoDienThoai = u.SoDienThoai ?? "Không có",
                    TenQuyen = u.PhanQuyen?.TenQuyen ?? "Không xác định",
                    NgayTao = u.NgayTao.ToString("dd/MM/yyyy"),
                    u.IsDeleted
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine("Lỗi khi lấy danh sách người dùng");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi tải danh sách",
                    error = ex.Message
                });
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _nguoiDungRepository.GetByIdAsyncWithPhanQuyen(id);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "Người dùng không tồn tại" });
                }
                var result = new
                {
                    MaNguoiDung = user.MaNguoiDung,
                    HoTen = user.HoTen,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    MaQuyen = user.MaQuyen,
                    TenQuyen = user.PhanQuyen != null ? user.PhanQuyen.TenQuyen : "Không xác định",
                    NgayTao = user.NgayTao,
                    IsDeleted = user.IsDeleted
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi lấy người dùng: {ex.Message}" });
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] NguoiDungDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Dữ liệu không hợp lệ", detail = errors });
            }

            // Tạo một đối tượng NguoiDungModel từ DTO
            var nguoiDung = new NguoiDungModel
            {
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                MatKhau = model.MatKhau,
                MaQuyen = model.MaQuyen,
                IsDeleted = model.IsDeleted,
                NgayTao = DateTime.Now
            };


            await _nguoiDungRepository.AddAsync(nguoiDung);
            return Json(new { success = true, message = "Thêm người dùng thành công!" });
        }
        [HttpPost("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NguoiDungModel model)
        {
            var user = await _nguoiDungRepository.GetByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại" });
            }

            user.HoTen = model.HoTen;
            user.Email = model.Email;
            user.SoDienThoai = model.SoDienThoai;
            if (!string.IsNullOrEmpty(model.MatKhau))
            {
                user.MatKhau = model.MatKhau;
            }
            user.MaQuyen = model.MaQuyen;
            user.IsDeleted = model.IsDeleted;

            await _nguoiDungRepository.UpdateAsync(user);
            return Json(new { success = true, message = "Cập nhật người dùng thành công!" });
        }

        [HttpPost("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed([FromBody] int id)
        {
            var user = await _nguoiDungRepository.GetByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại" });
            }

            user.IsDeleted = true; // Soft delete
            await _nguoiDungRepository.UpdateAsync(user);
            return Json(new { success = true, message = "Xóa người dùng thành công!" });
        }
    }
}