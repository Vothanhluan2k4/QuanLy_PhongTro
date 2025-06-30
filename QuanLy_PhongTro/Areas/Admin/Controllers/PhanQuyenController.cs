using Microsoft.AspNetCore.Mvc;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/PhanQuyen")]
    public class PhanQuyenController : Controller
    {
        private readonly IPhanQuyenRepositories _phanQuyenRepository;

        public PhanQuyenController(IPhanQuyenRepositories phanQuyenRepository)
        {
            _phanQuyenRepository = phanQuyenRepository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _phanQuyenRepository.GetAllAsync();
                return Json(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi tải danh sách quyền: {ex.Message}" });
            }
        }
    }
}