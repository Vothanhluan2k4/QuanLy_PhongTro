using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repositories;
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/PhongTro")]
    [Authorize(Roles = "Admin")]
    public class PhongTroController : Controller
    {
        private readonly IPhongTroRepositories _phongTroRepo;
        private readonly ILoaiPhongRepositories _loaiPhongRepo;
        private readonly IThietBiRepositories _thietBiRepo;
        private readonly IPhongTroThietBiRepositories _phongtrothietbi;
        private readonly IAnhPhongRepositories _anhphong;
        private readonly IPhanQuyenRepositories _phanquyenRepo;
        private readonly INguoiDungRepositories _nguoiDungRepo;
        private readonly IHopDongRepositories _hopDongRepo;
        private readonly DataContext _context; 

        public PhongTroController(
            IPhongTroRepositories phongTroRepo,
            ILoaiPhongRepositories loaiPhongRepo,
            IThietBiRepositories thietBiRepo,
            INguoiDungRepositories nguoiDungRepo,
            IPhanQuyenRepositories phanQuyenRepo,
            IHopDongRepositories hopDongRepo,
            DataContext context) // Inject DbContext
        {
            _phongTroRepo = phongTroRepo ?? throw new ArgumentNullException(nameof(phongTroRepo));
            _loaiPhongRepo = loaiPhongRepo ?? throw new ArgumentNullException(nameof(loaiPhongRepo));
            _thietBiRepo = thietBiRepo ?? throw new ArgumentNullException(nameof(thietBiRepo));
            _nguoiDungRepo = nguoiDungRepo ?? throw new ArgumentException(nameof(nguoiDungRepo));
            _phanquyenRepo = phanQuyenRepo ?? throw new ArgumentException(nameof(phanQuyenRepo));
            _hopDongRepo = hopDongRepo ?? throw new ArgumentException(nameof(hopDongRepo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Hiển thị danh sách phòng trọ và dữ liệu cho form thêm phòng
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var phongTros = await _phongTroRepo.GetAllAsync();
                var nguoiDungs = await _nguoiDungRepo.GetAllAsync();
                var loaiPhongs = await _loaiPhongRepo.GetAllAsync();
                var thietBis = await _thietBiRepo.GetAllAsync();
                var phanquyen = await _phanquyenRepo.GetAllAsync();
                var hopdong = await _hopDongRepo.GetAllAsync();

                var viewModel = new PhongTroNguoiDungViewModel
                {
                    PhongTros = phongTros,
                    NguoiDungs = nguoiDungs,
                    PhanQuyens = phanquyen,
                    HopDongs = hopdong
                };

                
                ViewBag.LoaiPhongs = loaiPhongs ?? new List<LoaiPhongModel>();
                ViewBag.ThietBis = thietBis ?? new List<ThietBiModel>();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải dữ liệu: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi tải dữ liệu", error = ex.Message });
            }
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var phongTros = await _context.PhongTros.ToListAsync();
                if (phongTros == null || !phongTros.Any())
                {
                    return Json(new { success = true, message = "Không có phòng nào", data = new List<object>() });
                }

                var result = phongTros.Select(p => new
                {
                    MaPhong = p.MaPhong,
                    TenPhong = p.TenPhong,
                    GiaThue = p.GiaThue,
                    TrangThai = p.TrangThai
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine( "Lỗi trong GetAll PhongTro");
                return StatusCode(500, new { success = false, message = "Lỗi khi tải danh sách phòng", error = ex.Message });
            }
        }

        [HttpGet("GetPagedRooms")]
        public async Task<IActionResult> GetPagedRooms(int page = 1, int pageSize = 5)
        {
            try
            {
                var totalRooms = await _context.PhongTros.CountAsync();
                var rooms = await _context.PhongTros
                    .Include(p => p.LoaiPhong)
                    .Include(p => p.PhongTroThietBis)
                    .ThenInclude(pt => pt.ThietBi)
                    .Include(p => p.AnhPhongTros)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = rooms.Select(p => new
                {
                    MaPhong = p.MaPhong,
                    TenPhong = p.TenPhong,
                    TenLoaiPhong = p.LoaiPhong?.TenLoaiPhong ?? "Không xác định",
                    DiaChi = p.DiaChi,
                    ThietBiList = p.PhongTroThietBis.Select(pt => $"{pt.ThietBi.TenThietBi} (Số lượng: {pt.SoLuong})").ToList(),
                    DienTich = p.DienTich,
                    TienDien = p.TienDien,
                    TienNuoc = p.TienNuoc,
                    GiaThue = p.GiaThue,
                    TrangThai = p.TrangThai,
                    AnhDaiDien = p.AnhPhongTros.FirstOrDefault(a => a.LaAnhDaiDien)?.DuongDan,
                    MoTa = p.MoTa
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = result,
                    totalPages = (int)Math.Ceiling((double)totalRooms / pageSize),
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi tải danh sách phòng", error = ex.Message });
            }
        }
        public async Task<IActionResult> GetAllNullRoom(string currentPhong = null)
        {
            try
            {
                // Chuyển đổi currentPhong từ string sang int
                int? currentPhongId = null;
                if (!string.IsNullOrEmpty(currentPhong))
                {
                    if (int.TryParse(currentPhong, out int parsedId))
                    {
                        currentPhongId = parsedId;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Mã phòng không hợp lệ." });
                    }
                }

                // Truy vấn các phòng còn trống và phòng hiện tại (nếu có)
                var phongTros = await _context.PhongTros
                    .Where(p => p.TrangThai == "Còn trống" || (currentPhongId.HasValue && p.MaPhong == currentPhongId.Value))
                    .ToListAsync();

                if (phongTros == null || !phongTros.Any())
                {
                    return Json(new { success = true, message = "Không có phòng nào", data = new List<object>() });
                }

                var result = phongTros.Select(p => new
                {
                    MaPhong = p.MaPhong,
                    TenPhong = p.TenPhong,
                    GiaThue = p.GiaThue,
                    TrangThai = p.TrangThai
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi trong GetAll PhongTro");
                return StatusCode(500, new { success = false, message = "Lỗi khi tải danh sách phòng", error = ex.Message });
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                Console.WriteLine($"Bắt đầu lấy thông tin phòng với ID: {id}");
                var phong = await _phongTroRepo.GetByIdAsync(id);
                if (phong == null)
                {
                    Console.WriteLine($"Không tìm thấy phòng với ID: {id}");
                    return NotFound(new { success = false, message = "Phòng không tồn tại" });
                }

                var phongDto = new PhongTroDto
                {
                    MaPhong = phong.MaPhong,
                    TenPhong = phong.TenPhong,
                    MaLoaiPhong = phong.MaLoaiPhong,
                    TenLoaiPhong = phong.LoaiPhong?.TenLoaiPhong ?? "",
                    DiaChi = phong.DiaChi,
                    DienTich = phong.DienTich,
                    GiaThue = phong.GiaThue,
                    TienDien = phong.TienDien,
                    TienNuoc = phong.TienNuoc,
                    TrangThai = phong.TrangThai,
                    MoTa = phong.MoTa,
                    PhongTroThietBis = phong.PhongTroThietBis?.Select(pttb => new PhongTroThietBiDto
                    {
                        MaThietBi = pttb.MaThietBi,
                        TenThietBi = pttb.ThietBi?.TenThietBi ?? "",
                        SoLuong = pttb.SoLuong
                    }).ToList() ?? new List<PhongTroThietBiDto>(),
                    AnhPhongTros = phong.AnhPhongTros?.Select(ap => new AnhPhongTroDto
                    {
                        DuongDan = ap.DuongDan,
                        LaAnhDaiDien = ap.LaAnhDaiDien
                    }).ToList() ?? new List<AnhPhongTroDto>()
                };

                Console.WriteLine($"Lấy thông tin phòng thành công: {System.Text.Json.JsonSerializer.Serialize(phongDto)}");
                return Json(phongDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy thông tin phòng: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Lỗi server khi lấy thông tin phòng", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        [HttpPost("UploadImages")]
        public async Task<IActionResult> UploadImages(List<IFormFile> files)
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một ảnh" });
                }

                var uploadedFiles = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length == 0)
                    {
                        continue;
                    }

                    // Tạo tên mới cho ảnh bằng GUID để tránh trùng lặp
                    var fileExtension = Path.GetExtension(file.FileName);
                    var newFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/asset", newFileName);

                    // Lưu ảnh vào thư mục
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    uploadedFiles.Add(newFileName);
                }

                return Json(new { success = true, fileNames = uploadedFiles });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi upload ảnh", detail = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] PhongTroViewModel viewModel)
        {
            try
            {
                // Ghi log dữ liệu đầu vào
                Console.WriteLine($"Dữ liệu nhận được: {System.Text.Json.JsonSerializer.Serialize(viewModel)}");

                // Kiểm tra dữ liệu
                if (string.IsNullOrEmpty(viewModel.TenPhong))
                {
                    return Json(new { success = false, message = "Tên phòng không được để trống" });
                }

                if (viewModel.MaLoaiPhong <= 0 || !await _loaiPhongRepo.GetAllAsync().ContinueWith(t => t.Result.Any(lp => lp.MaLoaiPhong == viewModel.MaLoaiPhong)))
                {
                    return Json(new { success = false, message = "Loại phòng không tồn tại" });
                }

                if (viewModel.DienTich <= 0 || viewModel.GiaThue <= 0 || viewModel.TienDien < 0 || viewModel.TienNuoc < 0)
                {
                    return Json(new { success = false, message = "Diện tích, giá thuê, tiền điện, tiền nước phải hợp lệ" });
                }

                // Kiểm tra địa chỉ
                if (viewModel.DiaChi == null || string.IsNullOrEmpty(viewModel.DiaChi.Province) || string.IsNullOrEmpty(viewModel.DiaChi.District) || string.IsNullOrEmpty(viewModel.DiaChi.HouseNumber))
                {
                    return Json(new { success = false, message = "Địa chỉ không hợp lệ" });
                }

                // Kiểm tra danh sách thiết bị
                var thietBiList = new List<(int MaThietBi, int SoLuong)>();
                if (viewModel.ThietBiData != null && viewModel.ThietBiData.Any())
                {
                    var allThietBis = await _thietBiRepo.GetAllAsync();
                    foreach (var thietBi in viewModel.ThietBiData)
                    {
                        if (thietBi.SoLuong <= 0)
                        {
                            return Json(new { success = false, message = $"Số lượng thiết bị {thietBi.ThietBiName} phải lớn hơn 0" });
                        }
                        if (!allThietBis.Any(tb => tb.MaThietBi == thietBi.ThietBiId))
                        {
                            return Json(new { success = false, message = $"Thiết bị với ID {thietBi.ThietBiId} không tồn tại" });
                        }
                        thietBiList.Add((thietBi.ThietBiId, thietBi.SoLuong));
                    }
                }

                // Kiểm tra danh sách ảnh
                if (viewModel.AnhData == null || !viewModel.AnhData.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một ảnh" });
                }

                var anhDaiDien = viewModel.AnhData.FirstOrDefault(a => a.IsDaiDien)?.Name;
                var anhDuongDans = viewModel.AnhData.Select(a => a.Name).ToList();
                if (string.IsNullOrEmpty(anhDaiDien) || !anhDuongDans.Contains(anhDaiDien))
                {
                    return Json(new { success = false, message = "Ảnh đại diện không hợp lệ", detail = $"AnhDaiDien: {anhDaiDien}, Danh sách ảnh: {string.Join(", ", anhDuongDans)}" });
                }

                // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Tổng hợp địa chỉ
                        var diaChi = $"{viewModel.DiaChi.HouseNumber}, {viewModel.DiaChi.District}, {viewModel.DiaChi.Province}";

                        // Lưu phòng vào database
                        var phongTro = new PhongTroModel
                        {
                            TenPhong = viewModel.TenPhong,
                            MaLoaiPhong = viewModel.MaLoaiPhong,
                            DiaChi = diaChi,
                            DienTich = viewModel.DienTich,
                            GiaThue = viewModel.GiaThue,
                            TienDien = viewModel.TienDien,
                            TienNuoc = viewModel.TienNuoc,
                            TrangThai = viewModel.TrangThai,
                            MoTa = viewModel.MoTa ?? ""
                        };

                        await _phongTroRepo.AddAsync(phongTro);
                        await _context.SaveChangesAsync();

                        if (phongTro.MaPhong <= 0)
                        {
                            throw new Exception("Không thể tạo MaPhong cho phòng trọ");
                        }

                        Console.WriteLine($"MaPhong vừa tạo: {phongTro.MaPhong}");

                        // Lưu thiết bị
                        foreach (var thietBi in thietBiList)            
                        {
                            var phongTroThietBi = new PhongTro_ThietBiModel
                            {
                                MaPhong = phongTro.MaPhong,
                                MaThietBi = thietBi.MaThietBi,
                                SoLuong = thietBi.SoLuong,
                            };
                            _context.PhongTro_ThietBiModels.Add(phongTroThietBi);
                        }

                        // Lưu ảnh (giả sử ảnh đã được tải lên và `Name` là đường dẫn)
                        foreach (var anh in viewModel.AnhData)
                        {
                            var anhPhong = new AnhPhongTroModel
                            {
                                MaPhong = phongTro.MaPhong,
                                DuongDan = anh.Name,
                                LaAnhDaiDien = anh.IsDaiDien
                            };
                            _context.AnhPhongTros.Add(anhPhong);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Json(new { success = true, message = "Thêm phòng thành công", maPhong = phongTro.MaPhong });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "Lỗi khi thêm phòng", detail = new[] { ex.Message, ex.StackTrace } });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi server", detail = new[] { ex.Message, ex.StackTrace } });
            }
        }

        [HttpPost("Update/{id}")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromBody] PhongTroViewModel viewModel)
        {
            Console.WriteLine($"Nhận được yêu cầu Update cho ID: {id}");
            Console.WriteLine($"ViewModel - TenPhong: {viewModel.TenPhong}");
            Console.WriteLine($"ViewModel - MaLoaiPhong: {viewModel.MaLoaiPhong}");
            Console.WriteLine($"ViewModel - DiaChi: {viewModel.DiaChi?.HouseNumber}, {viewModel.DiaChi?.District}, {viewModel.DiaChi?.Province}");
            Console.WriteLine($"ViewModel - DienTich: {viewModel.DienTich}");
            Console.WriteLine($"ViewModel - GiaThue: {viewModel.GiaThue}");
            Console.WriteLine($"ViewModel - TienDien: {viewModel.TienDien}");
            Console.WriteLine($"ViewModel - TienNuoc: {viewModel.TienNuoc}");
            Console.WriteLine($"ViewModel - TrangThai: {viewModel.TrangThai}");
            Console.WriteLine($"ViewModel - MoTa: {viewModel.MoTa}");
            //Console.WriteLine($"ViewModel - ThietBiData: {string.Join(", ", viewModel.ThietBiData?.Select(tb => $"{{ID: {tb.ThietBiId}, Name: {tb.ThietBiName}, SoLuong: {tb.SoLuong}}}") ?? "[]")}");
            //Console.WriteLine($"ViewModel - AnhData: {string.Join(", ", viewModel.AnhData?.Select(a => $"{{Name: {a.Name}, IsDaiDien: {a.IsDaiDien}, IsExisting: {a.IsExisting}}}") ?? "[]")}");

            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "ID phòng không hợp lệ" });
                }

                // Kiểm tra dữ liệu
                if (string.IsNullOrEmpty(viewModel.TenPhong))
                {
                    return Json(new { success = false, message = "Tên phòng không được để trống" });
                }

                if (viewModel.MaLoaiPhong <= 0 || !await _loaiPhongRepo.GetAllAsync().ContinueWith(t => t.Result.Any(lp => lp.MaLoaiPhong == viewModel.MaLoaiPhong)))
                {
                    return Json(new { success = false, message = "Loại phòng không tồn tại" });
                }

                if (viewModel.DienTich <= 0 || viewModel.GiaThue <= 0 || viewModel.TienDien < 0 || viewModel.TienNuoc < 0)
                {
                    return Json(new { success = false, message = "Diện tích, giá thuê, tiền điện, tiền nước phải hợp lệ" });
                }

                if (viewModel.DiaChi == null || string.IsNullOrEmpty(viewModel.DiaChi.Province) || string.IsNullOrEmpty(viewModel.DiaChi.District) || string.IsNullOrEmpty(viewModel.DiaChi.HouseNumber))
                {
                    return Json(new { success = false, message = "Địa chỉ không hợp lệ" });
                }

                var existingPhong = await _phongTroRepo.GetByIdAsync(id);
                if (existingPhong == null)
                {
                    return Json(new { success = false, message = "Phòng không tồn tại" });
                }

                // Kiểm tra danh sách thiết bị
                var thietBiList = new List<(int MaThietBi, int SoLuong)>();
                if (viewModel.ThietBiData != null && viewModel.ThietBiData.Any())
                {
                    var allThietBis = await _thietBiRepo.GetAllAsync();
                    foreach (var thietBi in viewModel.ThietBiData)
                    {
                        if (thietBi.SoLuong <= 0)
                        {
                            return Json(new { success = false, message = $"Số lượng thiết bị {thietBi.ThietBiName} phải lớn hơn 0" });
                        }
                        if (!allThietBis.Any(tb => tb.MaThietBi == thietBi.ThietBiId))
                        {
                            return Json(new { success = false, message = $"Thiết bị với ID {thietBi.ThietBiId} không tồn tại" });
                        }
                        thietBiList.Add((thietBi.ThietBiId, thietBi.SoLuong));
                    }
                }

                // Kiểm tra danh sách ảnh
                if (viewModel.AnhData == null || !viewModel.AnhData.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một ảnh" });
                }

                var anhDaiDien = viewModel.AnhData.FirstOrDefault(a => a.IsDaiDien)?.Name;
                var anhDuongDans = viewModel.AnhData.Select(a => a.Name).ToList();
                if (string.IsNullOrEmpty(anhDaiDien) || !anhDuongDans.Contains(anhDaiDien))
                {
                    return Json(new { success = false, message = "Ảnh đại diện không hợp lệ" });
                }

                // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Xóa thiết bị cũ
                        var oldThietBis = _context.PhongTro_ThietBiModels.Where(pttb => pttb.MaPhong == id);
                        _context.PhongTro_ThietBiModels.RemoveRange(oldThietBis);

                        // Thêm thiết bị mới
                        foreach (var thietBi in thietBiList)
                        {
                            var phongTroThietBi = new PhongTro_ThietBiModel
                            {
                                MaPhong = id,
                                MaThietBi = thietBi.MaThietBi,
                                SoLuong = thietBi.SoLuong
                            };
                            _context.PhongTro_ThietBiModels.Add(phongTroThietBi);
                        }

                        // Xóa ảnh cũ không còn trong danh sách mới
                        var oldAnhPhongs = _context.AnhPhongTros.Where(ap => ap.MaPhong == id).ToList();
                        foreach (var anh in oldAnhPhongs)
                        {
                            if (!viewModel.AnhData.Any(a => a.Name == anh.DuongDan && a.IsExisting))
                            {
                                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/asset", anh.DuongDan);
                                try
                                {
                                    if (System.IO.File.Exists(filePath))
                                    {
                                        System.IO.File.Delete(filePath);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Lỗi khi xóa file ảnh {filePath}: {ex.Message}");
                                    // Tiếp tục xử lý mà không làm gián đoạn transaction
                                }
                            }
                        }
                        _context.AnhPhongTros.RemoveRange(oldAnhPhongs);

                        // Thêm ảnh mới hoặc giữ ảnh cũ
                        foreach (var anh in viewModel.AnhData)
                        {
                            var anhPhong = new AnhPhongTroModel
                            {
                                MaPhong = id,
                                DuongDan = anh.Name,
                                LaAnhDaiDien = anh.IsDaiDien
                            };
                            _context.AnhPhongTros.Add(anhPhong);
                        }

                        // Cập nhật thông tin phòng
                        var diaChi = $"{viewModel.DiaChi.HouseNumber}, {viewModel.DiaChi.District}, {viewModel.DiaChi.Province}";
                        existingPhong.TenPhong = viewModel.TenPhong;
                        existingPhong.MaLoaiPhong = viewModel.MaLoaiPhong;
                        existingPhong.DiaChi = diaChi;
                        existingPhong.DienTich = viewModel.DienTich;
                        existingPhong.GiaThue = viewModel.GiaThue;
                        existingPhong.TienDien = viewModel.TienDien;
                        existingPhong.TienNuoc = viewModel.TienNuoc;
                        existingPhong.TrangThai = viewModel.TrangThai;
                        existingPhong.MoTa = viewModel.MoTa ?? "";

                        await _phongTroRepo.UpdateAsync(existingPhong);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Json(new { success = true, message = "Cập nhật phòng thành công!" });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Lỗi trong transaction khi cập nhật phòng: {ex.Message}\nStack Trace: {ex.StackTrace}");
                        return Json(new { success = false, message = "Lỗi khi cập nhật phòng", detail = new[] { ex.Message } });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi server khi cập nhật phòng: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi server khi cập nhật phòng", detail = new[] { ex.Message } });
            }
        }

        [HttpPost("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed([FromBody] Dictionary<string, int> data)
        {
            try
            {
                if (!data.ContainsKey("id") || data["id"] <= 0)
                {
                    return Json(new { success = false, message = "ID phòng không hợp lệ" });
                }

                int id = data["id"];
                var phong = await _phongTroRepo.GetByIdAsync(id);
                if (phong == null)
                {
                    return Json(new { success = false, message = "Phòng không tồn tại" });
                }

                // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Xóa các SuCo liên quan
                        var suCos = await _context.SuCos
                            .Where(sc => sc.MaPhong == id)
                            .AsNoTracking()
                            .ToListAsync();
                        if (suCos.Any())
                        {
                            _context.SuCos.RemoveRange(suCos);
                            await _context.SaveChangesAsync();
                        }

                        // Xóa các HopDongs liên quan
                        var hopDongs = await _context.HopDongs
                            .Where(hd => hd.MaPhong == id)
                            .Include(hd => hd.ChiSoDienNuocs)
                            .Include(hd => hd.HoaDons)
                            .AsNoTracking() // Tránh tracking
                            .ToListAsync();
                        if (hopDongs.Any())
                        {
                            _context.HopDongs.RemoveRange(hopDongs);
                            await _context.SaveChangesAsync();
                        }

                        // Xóa thiết bị liên quan
                        var thietBis = await _context.PhongTro_ThietBiModels
                            .Where(pttb => pttb.MaPhong == id)
                            .ToListAsync();
                        _context.PhongTro_ThietBiModels.RemoveRange(thietBis);

                        // Xóa ảnh liên quan
                        var anhPhongs = await _context.AnhPhongTros
                            .Where(ap => ap.MaPhong == id)
                            .ToListAsync();
                        foreach (var anh in anhPhongs)
                        {
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/asset", anh.DuongDan);
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                        _context.AnhPhongTros.RemoveRange(anhPhongs);

                        // Xóa các LuuPhongs liên quan (nếu cần)
                        var luuPhongs = await _context.LuuPhongs
                            .Where(lp => lp.MaPhong == id)
                            .AsNoTracking()
                            .ToListAsync();
                        if (luuPhongs.Any())
                        {
                            _context.LuuPhongs.RemoveRange(luuPhongs);
                        }

                        // Xóa phòng
                        var phongTroToDelete = await _context.PhongTros
                            .FirstOrDefaultAsync(p => p.MaPhong == id);
                        if (phongTroToDelete != null)
                        {
                            _context.PhongTros.Remove(phongTroToDelete);
                        }
                        else
                        {
                            await _phongTroRepo.DeleteAsync(id); // Fallback nếu cần
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Json(new { success = true, message = "Xóa phòng thành công!" });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "Lỗi khi xóa phòng", detail = new[] { ex.Message, ex.StackTrace } });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi server khi xóa phòng", detail = new[] { ex.Message, ex.StackTrace } });
            }
        }
    }
}