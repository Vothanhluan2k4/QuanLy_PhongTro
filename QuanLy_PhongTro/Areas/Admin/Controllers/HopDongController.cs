using Microsoft.AspNetCore.Mvc;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Threading.Tasks;
using System.Linq;
using QuanLy_PhongTro.Repository;

namespace QuanLy_PhongTro.Controllers
{
    [Area("Admin")]
    [Route("Admin/HopDong")]
    public class HopDongController : Controller
    {
        private readonly DataContext _context;
        private readonly ILogger<HopDongController> _logger;

        public HopDongController(DataContext context, ILogger<HopDongController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [HttpGet("GetAll")]
public async Task<IActionResult> GetAll(string search = "", int pageNumber = 1, int pageSize = 10)
{
    try
    {
        _logger.LogInformation("GetAll HopDong called with search term: {Search}, pageNumber: {PageNumber}, pageSize: {PageSize}", search, pageNumber, pageSize);

        var query = _context.HopDongs
            .Include(h => h.PhongTro)
            .Include(h => h.NguoiDung)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(h =>
                h.MaHopDong.ToString().Contains(search) ||
                (h.NguoiDung != null && h.NguoiDung.HoTen != null && h.NguoiDung.HoTen.Contains(search)) ||
                (h.PhongTro != null && h.PhongTro.TenPhong != null && h.PhongTro.TenPhong.Contains(search))
            );
        }

        var totalItems = await query.CountAsync();
        var hopDongs = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (hopDongs == null || !hopDongs.Any())
        {
            return Json(new
            {
                success = true,
                message = "Không có hợp đồng nào",
                data = new List<object>(),
                totalItems = 0,
                pageNumber,
                pageSize,
                totalPages = 0
            });
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var result = hopDongs.Select(h => new
        {
            MaHopDong = h.MaHopDong,
            MaPhong = h.MaPhong,
            TenPhong = h.PhongTro?.TenPhong ?? "Không xác định",
            MaNguoiDung = h.MaNguoiDung,
            HoTenNguoiDung = h.NguoiDung?.HoTen ?? "Không xác định",
            NgayBatDau = h.NgayBatDau.ToString("dd/MM/yyyy"),
            NgayKetThuc = h.NgayKetThuc.ToString("dd/MM/yyyy"),
            TienCoc = h.TienCoc,
            TrangThai = h.TrangThai ?? "Không xác định",
            PhuongThucThanhToan = h.PhuongThucThanhToan.ToString(),
            NgayKy = h.NgayKy.ToString("dd/MM/yyyy HH:mm:ss"),
            GhiChu = h.GhiChu ?? "Không có ghi chú"
        });

        return Json(new
        {
            success = true,
            data = result,
            totalItems,
            pageNumber,
            pageSize,
            totalPages
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in GetAll HopDong with search term: {Search}, pageNumber: {PageNumber}, pageSize: {PageSize}", search, pageNumber, pageSize);
        return StatusCode(500, new
        {
            success = false,
            message = "Đã xảy ra lỗi khi tải danh sách hợp đồng",
            error = ex.Message
        });
    }
}
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("GetById HopDong called with id: {Id}", id);

                var hopDong = await _context.HopDongs
                    .Include(h => h.PhongTro)
                    .Include(h => h.NguoiDung)
                    .FirstOrDefaultAsync(h => h.MaHopDong == id);

                if (hopDong == null)
                {
                    _logger.LogWarning("HopDong not found with id: {Id}", id);
                    return Json(new { success = false, message = "Hợp đồng không tồn tại" });
                }

                var result = new
                {
                    MaHopDong = hopDong.MaHopDong,
                    MaPhong = hopDong.MaPhong,
                    TenPhong = hopDong.PhongTro?.TenPhong ?? "Không xác định",
                    MaNguoiDung = hopDong.MaNguoiDung,
                    HoTenNguoiDung = hopDong.NguoiDung?.HoTen ?? "Không xác định",
                    NgayBatDau = hopDong.NgayBatDau,
                    NgayKetThuc = hopDong.NgayKetThuc,
                    TienCoc = hopDong.TienCoc,
                    TrangThai = hopDong.TrangThai ?? "Không xác định",
                    NgayKy = hopDong.NgayKy,
                    GhiChu = hopDong.GhiChu ?? "Không có ghi chú",
                    PhuongThucThanhToan = hopDong.PhuongThucThanhToan ?? "Chưa thanh toán"
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById HopDong with id: {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi lấy hợp đồng: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] HopDongViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Add HopDong called with viewModel: {@ViewModel}", viewModel);

                // Kiểm tra các trường bắt buộc chung
                if (viewModel.MaPhong <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn phòng." });
                }
                if (viewModel.NgayBatDau == default)
                {
                    return Json(new { success = false, message = "Ngày bắt đầu không được để trống." });
                }
                if (viewModel.NgayKetThuc == default)
                {
                    return Json(new { success = false, message = "Ngày kết thúc không được để trống." });
                }
                if (viewModel.TienCoc <= 0)
                {
                    return Json(new { success = false, message = "Tiền cọc phải lớn hơn 0." });
                }
                if (string.IsNullOrEmpty(viewModel.TrangThai))
                {
                    return Json(new { success = false, message = "Trạng thái không được để trống." });
                }
                if (viewModel.NgayKy == default)
                {
                    return Json(new { success = false, message = "Ngày ký không được để trống." });
                }
                if (string.IsNullOrEmpty(viewModel.UserInputType))
                {
                    return Json(new { success = false, message = "Vui lòng chọn cách nhập người dùng." });
                }

                // Kiểm tra ngày tháng
                if (viewModel.NgayBatDau < DateTime.Now.Date)
                {
                    return Json(new { success = false, message = "Ngày bắt đầu không được trong quá khứ." });
                }
                if (viewModel.NgayKetThuc <= viewModel.NgayBatDau)
                {
                    return Json(new { success = false, message = "Ngày kết thúc phải sau ngày bắt đầu." });
                }
                // Kiểm tra phương thức thanh toán
                if (string.IsNullOrEmpty(viewModel.PhuongThucThanhToan))
                {
                    return Json(new { success = false, message = "Phương thức thanh toán không được để trống." });
                }
                if (string.IsNullOrEmpty(viewModel.PhuongThucThanhToan))
                {
                    return Json(new { success = false, message = "Phương thức thanh toán không được để trống." });
                }
                if(string.IsNullOrEmpty(viewModel.GhiChu))
                {
                    return Json(new { success = false, message = "Ghi chú không được để trống" });
                }    

                // Kiểm tra người dùng dựa trên UserInputType
                if (viewModel.UserInputType == "existing")
                {
                    if (viewModel.MaNguoiDung <= 0)
                    {
                        return Json(new { success = false, message = "Vui lòng chọn người dùng hiện có." });
                    }
                    var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == viewModel.MaNguoiDung);
                    if (nguoiDung == null)
                    {
                        return Json(new { success = false, message = "Người dùng không tồn tại." });
                    }
                }
                else if (viewModel.UserInputType == "new")
                {
                    if (string.IsNullOrWhiteSpace(viewModel.NewUserHoTen) ||
                        string.IsNullOrWhiteSpace(viewModel.NewUserEmail) ||
                        string.IsNullOrWhiteSpace(viewModel.NewUserSoDienThoai))
                    {
                        return Json(new { success = false, message = "Thông tin người dùng mới không đầy đủ." });
                    }

                    if (await _context.NguoiDungs.AnyAsync(u => u.Email == viewModel.NewUserEmail))
                    {
                        return Json(new { success = false, message = "Email đã tồn tại." });
                    }

                    var newUser = new NguoiDungModel
                    {
                        HoTen = viewModel.NewUserHoTen,
                        Email = viewModel.NewUserEmail,
                        SoDienThoai = viewModel.NewUserSoDienThoai,
                        MatKhau = BCrypt.Net.BCrypt.HashPassword("12345678"),
                        MaQuyen = 2,
                        NgayTao = DateTime.Now,
                        IsDeleted = false
                    };
                    _context.NguoiDungs.Add(newUser);
                    await _context.SaveChangesAsync();
                    viewModel.MaNguoiDung = newUser.MaNguoiDung;
                }
                else
                {
                    return Json(new { success = false, message = "Cách nhập người dùng không hợp lệ." });
                }

                // Kiểm tra phòng
                var phong = await _context.PhongTros.FirstOrDefaultAsync(p => p.MaPhong == viewModel.MaPhong);
                if (phong == null)
                {
                    return Json(new { success = false, message = "Phòng không tồn tại." });
                }

                if (phong.TrangThai != "Còn trống")
                {
                    return Json(new { success = false, message = "Phòng đã được thuê hoặc không ở trạng thái còn trống." });
                }
                


                var hopDong = new HopDongModel
                {
                    MaPhong = viewModel.MaPhong,
                    MaNguoiDung = viewModel.MaNguoiDung,
                    NgayBatDau = viewModel.NgayBatDau,
                    NgayKetThuc = viewModel.NgayKetThuc,
                    TienCoc = viewModel.TienCoc,
                    TrangThai = viewModel.TrangThai,
                    NgayKy = viewModel.NgayKy,
                    GhiChu = viewModel.GhiChu ,
                    PhuongThucThanhToan = viewModel.PhuongThucThanhToan
                };

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        
                        phong.TrangThai = "Đã thuê";
                        _context.PhongTros.Update(phong);
                        _context.HopDongs.Add(hopDong);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return Json(new { success = true, message = "Thêm hợp đồng thành công!" });
                    }
                    catch (DbUpdateException ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Database error during Add HopDong with viewModel: {@ViewModel}", viewModel);
                        return Json(new { success = false, message = "Lỗi khi lưu vào cơ sở dữ liệu.", detail = ex.InnerException?.Message ?? ex.Message });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error in transaction for Add HopDong with viewModel: {@ViewModel}", viewModel);
                        return Json(new { success = false, message = "Lỗi hệ thống.", detail = ex.Message });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Add HopDong with viewModel: {@ViewModel}", viewModel);
                return Json(new { success = false, message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromForm] HopDongModel model)
        {
            try
            {
                _logger.LogInformation("Update HopDong called with model: {@Model}", model);

                // Kiểm tra các trường bắt buộc
                if (model.MaHopDong <= 0)
                {
                    return Json(new { success = false, message = "Mã hợp đồng không hợp lệ." });
                }
                if (model.MaPhong <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn phòng." });
                }
                if (model.NgayBatDau == default)
                {
                    return Json(new { success = false, message = "Ngày bắt đầu không được để trống." });
                }
                if (model.NgayKetThuc == default)
                {
                    return Json(new { success = false, message = "Ngày kết thúc không được để trống." });
                }
                if (model.TienCoc <= 0)
                {
                    return Json(new { success = false, message = "Tiền cọc phải lớn hơn 0." });
                }
                if (string.IsNullOrEmpty(model.TrangThai))
                {
                    return Json(new { success = false, message = "Trạng thái không được để trống." });
                }
                if (model.NgayKy == default)
                {
                    return Json(new { success = false, message = "Ngày ký không được để trống." });
                }

                // Kiểm tra ngày tháng
                if (model.NgayBatDau < DateTime.Now.Date)
                {
                    return Json(new { success = false, message = "Ngày bắt đầu không được trong quá khứ." });
                }
                if (model.NgayKetThuc <= model.NgayBatDau)
                {
                    return Json(new { success = false, message = "Ngày kết thúc phải sau ngày bắt đầu." });
                }
                // Kiểm tra phương thức thanh toán
                if (string.IsNullOrEmpty(model.PhuongThucThanhToan))
                {
                    return Json(new { success = false, message = "Phương thức thanh toán không được để trống." });
                }

                var hopDong = await _context.HopDongs
                    .Include(h => h.PhongTro) // Bao gồm thông tin phòng trọ để cập nhật
                    .FirstOrDefaultAsync(h => h.MaHopDong == model.MaHopDong);

                if (hopDong == null)
                {
                    return Json(new { success = false, message = "Hợp đồng không tồn tại." });
                }

                // Kiểm tra người dùng
                string userInputType = Request.Form["UserInputType"];
                if (string.IsNullOrEmpty(userInputType))
                {
                    return Json(new { success = false, message = "Vui lòng chọn cách nhập người dùng." });
                }

                if (userInputType == "existing")
                {
                    if (model.MaNguoiDung <= 0)
                    {
                        return Json(new { success = false, message = "Vui lòng chọn người dùng hiện có." });
                    }
                }
                else if (userInputType == "new")
                {
                    string hoTen = Request.Form["NewUserHoTen"];
                    string email = Request.Form["NewUserEmail"];
                    string soDienThoai = Request.Form["NewUserSoDienThoai"];

                    if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(soDienThoai))
                    {
                        return Json(new { success = false, message = "Thông tin người dùng mới không đầy đủ." });
                    }

                    // Kiểm tra email trùng lặp
                    if (await _context.NguoiDungs.AnyAsync(u => u.Email == email))
                    {
                        return Json(new { success = false, message = "Email đã tồn tại." });
                    }

                    var newUser = new NguoiDungModel
                    {
                        HoTen = hoTen,
                        Email = email,
                        SoDienThoai = soDienThoai,
                        MatKhau = BCrypt.Net.BCrypt.HashPassword("12345678"),
                        MaQuyen = 2,
                        NgayTao = DateTime.Now,
                        IsDeleted = false
                    };
                    _context.NguoiDungs.Add(newUser);
                    await _context.SaveChangesAsync();
                    model.MaNguoiDung = newUser.MaNguoiDung;
                }
                else
                {
                    return Json(new { success = false, message = "Cách nhập người dùng không hợp lệ." });
                }

                // Cập nhật thông tin hợp đồng
                hopDong.MaPhong = model.MaPhong;
                hopDong.MaNguoiDung = model.MaNguoiDung;
                hopDong.NgayBatDau = model.NgayBatDau;
                hopDong.NgayKetThuc = model.NgayKetThuc;
                hopDong.TienCoc = model.TienCoc;
                hopDong.TrangThai = model.TrangThai;
                hopDong.NgayKy = model.NgayKy;
                hopDong.GhiChu = model.GhiChu;
                hopDong.PhuongThucThanhToan = model.PhuongThucThanhToan;

                // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.SaveChangesAsync();

                        // Nếu trạng thái hợp đồng là "Hết hiệu lực", cập nhật trạng thái phòng trọ thành "Còn trống"
                        if (hopDong.TrangThai == "Hết hiệu lực" && hopDong.PhongTro != null)
                        {
                            hopDong.PhongTro.TrangThai = "Còn trống";
                            _context.PhongTros.Update(hopDong.PhongTro);
                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();
                        return Json(new { success = true, message = "Cập nhật hợp đồng thành công!" });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error during transaction in Update HopDong with id: {Id}", model.MaHopDong);
                        return Json(new { success = false, message = $"Lỗi khi cập nhật hợp đồng: {ex.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update HopDong");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Delete HopDong called with id: {Id}", id);

                var hopDong = await _context.HopDongs
                    .Include(h => h.PhongTro)
                    .FirstOrDefaultAsync(h => h.MaHopDong == id);

                if (hopDong == null)
                {
                    _logger.LogWarning("HopDong not found with id: {Id}", id);
                    return Json(new { success = false, message = "Hợp đồng không tồn tại." });
                }

                var phongtro = hopDong.PhongTro;
                if (phongtro == null)
                {
                    _logger.LogWarning("PhongTro not found with MaPhong: {MaPhong}", hopDong.MaPhong);
                    return Json(new { success = false, message = "Phòng trọ liên quan đến hợp đồng không tồn tại." });
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Cập nhật trạng thái phòng trọ thành "Còn trống"
                        phongtro.TrangThai = "Còn trống";
                        _context.PhongTros.Update(phongtro);

                        hopDong.TrangThai = "Hết hiệu lực";
                       await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Json(new { success = true, message = "Xóa hợp đồng thành công!" });
                    }
                    catch (DbUpdateException ex)
                    {
                        await transaction.RollbackAsync();          
                        _logger.LogError(ex, "Database error during Delete HopDong with id: {Id}", id);
                        return Json(new { success = false, message = $"Lỗi cơ sở dữ liệu: {ex.InnerException?.Message ?? ex.Message}" });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error during transaction in Delete HopDong with id: {Id}", id);
                        return Json(new { success = false, message = $"Lỗi khi xóa hợp đồng: {ex.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete HopDong with id: {Id}", id);
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
   
    }
}