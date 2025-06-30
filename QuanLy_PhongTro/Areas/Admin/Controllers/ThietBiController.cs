using Microsoft.AspNetCore.Mvc;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Controllers
{
    [Area("Admin")]
    [Route("Admin/ThietBi")]
    public class ThietBiController : Controller
    {
        private readonly IThietBiRepositories _thietBiRepository;

        public ThietBiController(IThietBiRepositories thietBiRepository)
        {
            _thietBiRepository = thietBiRepository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var equipments = await _thietBiRepository.GetAllAsync();
                return Ok(equipments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách thiết bị", detail = ex.Message });
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var equipment = await _thietBiRepository.GetByIdAsync(id);
                if (equipment == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thiết bị" });
                }
                return Ok(equipment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin thiết bị", detail = ex.Message });
            }
        }

        [HttpPost("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] ThietBiModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", detail = errors });
                }

                var equipment = new ThietBiModel
                {
                    TenThietBi = model.TenThietBi,
                    MoTa = model.MoTa,
                    TinhTrang =  model.TinhTrang,
                    DonViTinh = model.DonViTinh // Đảm bảo trường này được gửi từ form
                };

                await _thietBiRepository.AddAsync(equipment);
                return Ok(new { success = true, message = "Thêm thiết bị thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi thêm thiết bị", detail = ex.Message });
            }
        }

        [HttpPost("Update/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromForm] ThietBiModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", detail = errors });
                }

                var equipment = await _thietBiRepository.GetByIdAsync(id);
                if (equipment == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thiết bị" });
                }

                equipment.TenThietBi = model.TenThietBi;
                equipment.MoTa = model.MoTa;
                equipment.TinhTrang = model.TinhTrang;
                equipment.DonViTinh = model.DonViTinh;
                await _thietBiRepository.UpdateAsync(equipment);

                return Ok(new { success = true, message = "Cập nhật thiết bị thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật thiết bị", detail = ex.Message });
            }
        }

        [HttpPost("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromForm] int id)
        {
            try
            {
                var equipment = await _thietBiRepository.GetByIdAsync(id);
                if (equipment == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thiết bị" });
                }

                await _thietBiRepository.DeleteAsync(id);
                return Ok(new { success = true, message = "Xóa thiết bị thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa thiết bị", detail = ex.Message });
            }
        }
    }
}