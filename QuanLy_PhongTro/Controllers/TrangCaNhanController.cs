using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;
using System.Security.Claims;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using QuanLy_PhongTro.ViewModel;
using QuanLy_PhongTro.Models.Order;
using QuanLy_PhongTro.Models.Vnpay;
using QuanLy_PhongTro.Services.Momo;
using QuanLy_PhongTro.Services.Vnpay;
using Newtonsoft.Json;

namespace QuanLy_PhongTro.Controllers
{
    public class TrangCaNhanController : Controller
    {
        private readonly DataContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;

        public TrangCaNhanController(DataContext context, IVnPayService vnPayService, IMomoService momoService)
        {
            _context = context;
            _vnPayService = vnPayService;
            _momoService = momoService;
        }

        // GET: TrangCaNhanController
        public async Task<IActionResult> Index(string tab = "", string returnToUrl = null)
        {
            var maNguoiDungClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
            {
                return Unauthorized(new { success = false, message = "Không thể xác định người dùng" });
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);

            if (nguoiDung == null)
            {
                return NotFound();
            }

            var rooms = await _context.HopDongs
                .Where(hd => hd.MaNguoiDung == nguoiDung.MaNguoiDung && hd.TrangThai == "Đang hiệu lực")
                .Include(hd => hd.PhongTro) // Bao gồm thông tin phòng
                .Select(hd => hd.PhongTro) // Chỉ lấy phòng
                .Distinct() 
                .ToListAsync();

            ViewBag.Rooms = rooms;
            ViewBag.MaNguoiDung = maNguoiDung;
            ViewBag.SoDu = Math.Floor(nguoiDung.SoDu).ToString("N0");
            Console.WriteLine($"Số dư định dạng (ViewBag.SoDu): {ViewBag.SoDu}");
            ViewBag.AvatarUrl = nguoiDung.AvatarUrl ?? "/asset/avatarUser/default_avatarUser.png";
            ViewBag.HoTen = nguoiDung.HoTen;
            ViewBag.Email = nguoiDung.Email;
            ViewBag.SoDienThoai = nguoiDung.SoDienThoai;


            // Truyền tab vào view để kích hoạt tab mong muốn
            // Xử lý tab với danh sách hợp lệ
            var validTabs = new[] { "profile", "days-receipt", "room-book", "balance-info", "deposit-history" };
            ViewBag.ActiveTab = validTabs.Contains(tab) ? tab : "profile";
            if (!string.IsNullOrEmpty(returnToUrl))
            {
                TempData["ReturnToUrl"] = returnToUrl;
            }
            return View("~/Views/Account/TrangCaNhan.cshtml");
        }

        // POST: Cập nhật thông tin cá nhân
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateModel model)
        {
            try
            {
                var maNguoiDungClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
                {
                    return Json(new { success = false, message = "Không thể xác định người dùng" });
                }

                var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
                if (nguoiDung == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại" });
                }

                bool hasChanges = false;

                // Chuẩn hóa dữ liệu
                model.DisplayName = model.DisplayName?.Trim();
                model.Email = model.Email?.Trim();
                model.Phone = model.Phone?.Trim();

                // Xử lý tên hiển thị
                if (!string.IsNullOrWhiteSpace(model.DisplayName) &&
                    !string.Equals(nguoiDung.HoTen?.Trim(), model.DisplayName, StringComparison.Ordinal))
                {
                    nguoiDung.HoTen = model.DisplayName;
                    hasChanges = true;
                }

                // Xử lý email
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    if (!new EmailAddressAttribute().IsValid(model.Email))
                    {
                        TempData["ErrorMessage"] = "Email không đúng định dạng";
                        return Json(new { success = false, message = "Email không đúng định dạng" });
                    }

                    if (_context.NguoiDungs.Any(u => u.Email == model.Email && u.MaNguoiDung != maNguoiDung))
                    {
                        TempData["ErrorMessage"] = "Email đã được sử dụng bởi tài khoản khác";
                        return Json(new { success = false, message = "Email đã được sử dụng bởi tài khoản khác" });
                    }

                    if (!string.Equals(nguoiDung.Email?.Trim(), model.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        nguoiDung.Email = model.Email;
                        hasChanges = true;
                    }
                }

                // Xử lý số điện thoại
                if (!string.IsNullOrWhiteSpace(model.Phone))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(model.Phone, @"^\d{10}$"))
                    {
                        TempData["ErrorMessage"] = "Số điện thoại không đúng định dạng (phải có 10 chữ số)";
                        return Json(new { success = false, message = "Số điện thoại không đúng định dạng (phải có 10 chữ số)" });
                    }

                    if (!string.Equals(nguoiDung.SoDienThoai?.Trim(), model.Phone, StringComparison.Ordinal))
                    {
                        nguoiDung.SoDienThoai = model.Phone;
                        hasChanges = true;
                    }
                }

                if (!hasChanges)
                {
                    TempData["SuccessMessage"] = "Không có thay đổi nào để cập nhật";
                    return Json(new { success = true, message = "Không có thay đổi nào để cập nhật", displayName = nguoiDung.HoTen, avatarUrl = nguoiDung.AvatarUrl });
                }
                    
                _context.Update(nguoiDung);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(model.DisplayName))
                {
                    HttpContext.Session.SetString("HoTen", model.DisplayName);
                }

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công";
                return Json(new { success = true, message = "Cập nhật thông tin thành công", displayName = nguoiDung.HoTen, avatarUrl = nguoiDung.AvatarUrl });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật thông tin: {ex.Message}";
                return Json(new { success = false, message = $"Lỗi khi cập nhật thông tin: {ex.Message}" });
            }
        }

        // POST: Cập nhật avatar
        [HttpPost]
        public async Task<IActionResult> CapNhatAvatar(IFormFile avatarFile)
        {
            try
            {
                var maNguoiDungClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
                {
                    return Json(new { success = false, message = "Không thể xác định người dùng" });
                }

                var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
                if (nguoiDung == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại" });
                }

                if (avatarFile == null || avatarFile.Length == 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn file ảnh";
                    return Json(new { success = false, message = "Vui lòng chọn file ảnh" });
                }

                // Kiểm tra loại file (chỉ cho phép ảnh)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(avatarFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = "Chỉ hỗ trợ file ảnh (jpg, jpeg, png, gif)";
                    return Json(new { success = false, message = "Chỉ hỗ trợ file ảnh (jpg, jpeg, png, gif)" });
                }

                // Đảm bảo thư mục tồn tại
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","asset", "avatarUser");
                if (!Directory.Exists(directoryPath))   
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Tạo tên file duy nhất
                var fileName = $"avatar_{maNguoiDung}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(directoryPath, fileName);

                // Lưu file vào thư mục
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn avatar trong database
                nguoiDung.AvatarUrl = $"asset/avatarUser/{fileName}";
                _context.Update(nguoiDung);
                await _context.SaveChangesAsync();

                // Cập nhật thông tin trong session
                HttpContext.Session.SetString("AvatarUrl", nguoiDung.AvatarUrl);
                TempData["SuccessMessage"] = "Cập nhật avatar thành công";
                return Json(new { success = true, message = "Cập nhật avatar thành công", avatarUrl = nguoiDung.AvatarUrl, displayName = nguoiDung.HoTen });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật avatar: {ex.Message}";
                return Json(new { success = false, message = $"Lỗi khi cập nhật avatar: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetRooms(int maNguoiDung, string searchTerm = "")
        {
            try
            {
                var hopDongs = await _context.HopDongs
                    .Where(hd => hd.MaNguoiDung == maNguoiDung)
                    .ToListAsync();

                if (hopDongs == null || !hopDongs.Any())
                {
                    Console.WriteLine($"Không tìm thấy hợp đồng cho MaNguoiDung = {maNguoiDung}");
                }
                else
                {
                    Console.WriteLine($"Tìm thấy {hopDongs.Count} hợp đồng cho MaNguoiDung = {maNguoiDung}");
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    hopDongs = hopDongs
                        .Where(hd => hd.MaPhong.ToString().Contains(searchTerm) ||
                                    (hd.TrangThai != null && hd.TrangThai.ToLower().Contains(searchTerm.ToLower())) ||
                                    (hd.GhiChu != null && hd.GhiChu.ToLower().Contains(searchTerm.ToLower())))
                        .ToList();
                }

                var rooms = hopDongs.Select(hd => new
                {
                    MaHopDong = hd.MaHopDong,
                    MaPhong = hd.MaPhong,
                    NgayBatDau = hd.NgayBatDau.ToString("yyyy-MM-dd"),
                    NgayKetThuc = hd.NgayKetThuc.ToString("yyyy-MM-dd"),
                    TienCoc = hd.TienCoc,
                    TrangThai = hd.TrangThai ?? "N/A",
                    NgayKy = hd.NgayKy.ToString("yyyy-MM-dd"),
                    GhiChu = hd.GhiChu ?? "Không có ghi chú",
                    PhuongThucThanhToan = hd.PhuongThucThanhToan ?? "N/A"
                }).ToList();

                return Json(new { success = true, data = rooms });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi lấy danh sách phòng: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions(int maNguoiDung, string searchTerm = "")
        {
            try
            {
                // Lấy tất cả giao dịch từ MomoInfor theo MaNguoiDung
                var momoTransactions = await _context.MomoInfor
                    .Where(m => m.CustomerId == maNguoiDung)
                    .Select(m => new TransactionViewModel
                    {
                        TransactionId = m.TransactionId,
                        OrderDescription = m.OrderInfo ?? "Không có mô tả",
                        PaymentMethod = m.PaymentMethod,
                        Amount = m.Amount,
                        DateCreated = m.CreatedDate
                    })
                    .ToListAsync();

                // Lấy tất cả giao dịch từ VnPayInfor theo MaNguoiDung
                var vnpayTransactions = await _context.VnPayInfor
                    .Where(v => v.CustomerId == maNguoiDung)
                    .Select(v => new TransactionViewModel
                    {
                        TransactionId = v.TransactionId,
                        OrderDescription = v.OrderDescription ?? "Không có mô tả",
                        PaymentMethod = v.PaymentMethod,
                        Amount = v.Amount,
                        DateCreated = v.DateCreated
                    })
                    .ToListAsync();

                var accountPaid = await _context.AccountPaidInfor
                    .Where(v => v.MaNguoiDung == maNguoiDung)
                    .Select(v => new TransactionViewModel
                    {
                        TransactionId = v.TransactionId,
                        OrderDescription = v.OrderDescription ?? "Không có mô tả",
                        PaymentMethod = v.PaymentMethod,
                        Amount = v.Amount,
                        DateCreated = v.DateCreated
                    }).ToListAsync();

                // Kết hợp tất cả giao dịch
                var transactions = momoTransactions.Concat(vnpayTransactions).Concat(accountPaid).ToList();

                // Lọc theo searchTerm nếu có
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    transactions = transactions
                        .Where(t => t.TransactionId.ToLower().Contains(searchTerm.ToLower()))
                        .ToList();
                }

                // Định dạng dữ liệu trước khi trả về
                var formattedTransactions = transactions.Select(t => new
                {
                    TransactionId = t.TransactionId,
                    OrderDescription = t.OrderDescription,
                    PaymentMethod = t.PaymentMethod,
                    Amount = t.Amount.ToString("N0") + " VNĐ",
                    DateCreated = t.DateCreated.ToString("dd/MM/yyyy HH:mm:ss")
                }).ToList();

                return Json(new { success = true, data = formattedTransactions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi lấy lịch sử giao dịch: {ex.Message}" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetHoaDons(string searchTerm = "", int? fromMonth = null, int? fromYear = null, int? toMonth = null, int? toYear = null, string trangThai = "")
        {
            try
            {
                var maNguoiDungClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
                {
                    return Json(new { success = false, message = "Không thể xác định người dùng" });
                }

                var hopDongs = await _context.HopDongs
                    .Where(hd => hd.MaNguoiDung == maNguoiDung)
                    .Select(hd => hd.MaHopDong)
                    .ToListAsync();

                if (!hopDongs.Any())
                {
                    return Json(new { success = true, data = new List<object>() });
                }

                // Truy vấn cơ bản với GroupJoin và SelectMany
                var query = _context.HoaDon
                    .Where(hd => hopDongs.Contains(hd.MaHopDong))
                    .GroupJoin(
                        _context.ChiSoDienNuoc,
                        hd => new { hd.MaHopDong, hd.Thang, hd.Nam },
                        cs => new { cs.MaHopDong, cs.Thang, cs.Nam },
                        (hd, csCollection) => new { HoaDon = hd, ChiSoDienNuocs = csCollection })
                    .SelectMany(x => x.ChiSoDienNuocs.DefaultIfEmpty(), (hd, cs) => new { hd.HoaDon, ChiSoDienNuoc = cs });

                // Lọc trước khi chuyển sang client-side
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    if (int.TryParse(searchTerm, out int searchId))
                    {
                        query = query.Where(x => x.HoaDon.MaHoaDon == searchId);
                    }
                    else
                    {
                        query = query.Where(x => false);
                    }
                }

                if (fromMonth.HasValue && fromYear.HasValue && toMonth.HasValue && toYear.HasValue)
                {
                    query = query.Where(x => (x.HoaDon.Nam > fromYear || (x.HoaDon.Nam == fromYear && x.HoaDon.Thang >= fromMonth)) &&
                                            (x.HoaDon.Nam < toYear || (x.HoaDon.Nam == toYear && x.HoaDon.Thang <= toMonth)));
                }

                if (!string.IsNullOrEmpty(trangThai) && trangThai != "All")
                {
                    string statusFilter = trangThai == "NotYetPaid" ? "Chưa thanh toán" : trangThai;
                    query = query.Where(x => x.HoaDon.TrangThai == statusFilter);
                }

                // Chuyển sang client-side evaluation
                var intermediateResults = await query
                    .Select(x => new
                    {
                        HoaDon = x.HoaDon,
                        ChiSoDienNuoc = x.ChiSoDienNuoc
                    })
                    .ToListAsync();

                // Xử lý GroupBy và OrderBy trên client-side
                var hoaDons = intermediateResults
                    .GroupBy(x => x.HoaDon.MaHoaDon)
                    .Select(g => g.OrderByDescending(x => x.ChiSoDienNuoc?.NgayGhi ?? DateTime.MinValue).FirstOrDefault())
                    .Select(x => new
                    {
                        MaHoaDon = x.HoaDon.MaHoaDon,
                        MaHopDong = x.HoaDon.MaHopDong,
                        Thang = x.HoaDon.Thang,
                        Nam = x.HoaDon.Nam,
                        ChiSoNuocCu = x.ChiSoDienNuoc != null ? x.ChiSoDienNuoc.ChiSoNuocCu : 0,
                        ChiSoNuocMoi = x.ChiSoDienNuoc != null ? x.ChiSoDienNuoc.ChiSoNuocMoi : 0,
                        ChiSoDienCu = x.ChiSoDienNuoc != null ? x.ChiSoDienNuoc.ChiSoDienCu : 0,
                        ChiSoDienMoi = x.ChiSoDienNuoc != null ? x.ChiSoDienNuoc.ChiSoDienMoi : 0,
                        TienDien = x.HoaDon.TienDien,
                        TienNuoc = x.HoaDon.TienNuoc,
                        TienRac = x.HoaDon.TienRac,
                        TongTien = x.HoaDon.TongTien,
                        NgayPhatHanh = x.HoaDon.NgayPhatHanh != null ? x.HoaDon.NgayPhatHanh.ToString("dd/MM/yyyy") : "N/A",
                        TrangThai = x.HoaDon.TrangThai ?? "N/A"
                    })
                    .ToList();

                return Json(new { success = true, data = hoaDons });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi lấy danh sách hóa đơn: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NapTien(string name, double amount, string orderDescription, string orderType, string PaymentMethod, string OrderId, string OrderInfo, string FullName, string Email, string returnToUrl = null)
        {
            var maNguoiDungClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int maNguoiDung))
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "TrangCaNhan");
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(maNguoiDung);
            if (nguoiDung == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index", "TrangCaNhan");
            }

            var napTienInfo = new NapTienModel
            {
                MaNguoiDung = maNguoiDung,
                Amount = amount,
                PaymentMethod = PaymentMethod,
                CreatedDate = DateTime.Now,
                OrderId = OrderId,
                OrderInfo = OrderInfo,
                OrderDescription = orderDescription,
                OrderType = orderType
            };

            TempData["PendingNapTien"] = JsonConvert.SerializeObject(napTienInfo);
            TempData["PendingEmail"] = Email;
            TempData["ReturnToUrl"] = returnToUrl;

            if (PaymentMethod == "VNPay")
            {
                var paymentInfo = new PaymentInformationModel
                {
                    Name = name,
                    Amount = amount,
                    OrderDescription = orderDescription,
                    CreatedDate = DateTime.Now,
                    OrderType = orderType
                };
                var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext, "Deposit");
                Console.WriteLine($"VNPay PaymentUrl: {paymentUrl}");
                return Redirect(paymentUrl);
            }
            else if (PaymentMethod == "MoMo")
            {
                var paymentInfo = new OrderInfoModel
                {
                    FullName = FullName,
                    Amount = amount,
                    OrderInfo = OrderInfo,
                    OrderId = OrderId
                };

                
                var paymentUrlMomo = await _momoService.CreatePaymentMomo(paymentInfo, "Deposit");
                Console.WriteLine($"MoMo Response: {JsonConvert.SerializeObject(paymentUrlMomo)}");
                return Redirect(paymentUrlMomo.PayUrl);
            }
            else
            {
                TempData["ErrorMessage"] = "Hình thức thanh toán không hợp lệ.";
                return RedirectToAction("Index", "TrangCaNhan", new { tab = "deposit-history" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitIssue(IFormCollection form, List<IFormFile> media)
        {
            try
            {
                // Validate user
                var maNguoiDungClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(maNguoiDungClaim) || !int.TryParse(maNguoiDungClaim, out int userId))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để gửi báo cáo." });
                }

                // Validate room
                if (!int.TryParse(form["MaPhong"], out var MaPhong) || MaPhong <= 0)
                {
                    return Json(new { success = false, message = "Phòng không hợp lệ." });
                }

                // Check if the room exists
                var phongTro = await _context.PhongTros.FindAsync(MaPhong);
                if (phongTro == null)
                {
                    return Json(new { success = false, message = "Phòng không tồn tại." });
                }

                // Check if the user exists
                var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
                if (nguoiDung == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại." });
                }

                // Get form values
                var issueType = form["LoaiSuCo"].ToString();
                var description = form["MoTa"].ToString();
                var priorityStr = form["MucDo"].ToString();
                var phone = form["DienThoai"].ToString();

                // Validate required fields
                if (string.IsNullOrEmpty(issueType))
                {
                    return Json(new { success = false, message = "Vui lòng chọn loại sự cố." });
                }

                if (string.IsNullOrEmpty(description))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mô tả chi tiết." });
                }

                // Validate MucDo (since it's still an enum in this model)
                if (!Enum.TryParse<MucDoUuTien>(priorityStr, true, out var priority))
                {
                    priority = MucDoUuTien.BinhThuong;
                }

                // Handle media upload
                var mediaUrls = new List<string>();
                if (media != null && media.Count > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/asset/uploads");
                    try
                    {
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        foreach (var file in media)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(uploadsFolder, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                mediaUrls.Add(fileName);        
                            }
                        }   
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return Json(new { success = false, message = "Không có quyền lưu file vào thư mục uploads." });
                    }
                }

                // Create SuCoModel instance
                var suCo = new SuCoModel
                {
                    MaPhong = MaPhong,
                    MaNguoiDung = userId,
                    LoaiSuCo = issueType,
                    MoTa = description,
                    MediaUrl = mediaUrls.Any() ? string.Join(";", mediaUrls) : null,
                    MucDo = priority,   
                    DienThoai = phone,
                    NgayTao = DateTime.Now,
                    TrangThai = "Chưa xử lý"
                };

                _context.SuCos.Add(suCo);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Báo cáo sự cố đã được gửi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra khi gửi báo cáo: {ex.Message}" });
            }
        }
    }
}
    
