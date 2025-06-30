using Microsoft.AspNetCore.Mvc;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Controllers
{
    [Area("Admin")]
    [Route("Admin/LoaiPhong")]
    public class LoaiPhongController : Controller
    {
        private readonly ILoaiPhongRepositories _loaiPhongRepository;

        public LoaiPhongController(ILoaiPhongRepositories loaiPhongRepository)
        {
            _loaiPhongRepository = loaiPhongRepository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var roomTypes = await _loaiPhongRepository.GetAllAsync();
            return Ok(roomTypes);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var roomType = await _loaiPhongRepository.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy loại phòng" });
            }
            return Ok(roomType);
        }

        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] LoaiPhongModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", detail = errors });
            }

            var roomType = new LoaiPhongModel
            {
                TenLoaiPhong = model.TenLoaiPhong,
                MoTa = model.MoTa
            };

            await _loaiPhongRepository.AddAsync(roomType);
            return Ok(new { success = true, message = "Thêm loại phòng thành công" });
        }

        [HttpPost("Update/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromForm] LoaiPhongModel model)
        {
            if (id != model.MaLoaiPhong && model.MaLoaiPhong != 0) // Cho phép MaLoaiPhong là 0 khi thêm mới
            {
                return BadRequest(new { success = false, message = "ID không khớp" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", detail = errors });
            }

            var roomType = await _loaiPhongRepository.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy loại phòng" });
            }

            roomType.TenLoaiPhong = model.TenLoaiPhong;
            roomType.MoTa = model.MoTa;
            await _loaiPhongRepository.UpdateAsync(roomType);

            return Ok(new { success = true, message = "Cập nhật loại phòng thành công" });
        }

        [HttpPost("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromForm] int id)
        {
            var roomType = await _loaiPhongRepository.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy loại phòng" });
            }

            await _loaiPhongRepository.DeleteAsync(id);
            return Ok(new { success = true, message = "Xóa loại phòng thành công" });
        }
    }
}