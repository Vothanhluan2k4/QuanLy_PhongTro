using Microsoft.AspNetCore.Mvc;
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using QuanLy_PhongTro.ViewModel;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Owl.reCAPTCHA.v2;
using NuGet.Common;
using Owl.reCAPTCHA;

namespace QuanLy_PhongTro.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly DataContext _context;
        private readonly IreCAPTCHASiteVerifyV2 _siteVerify;


        public DangNhapController(DataContext context, IreCAPTCHASiteVerifyV2 siteVerifyV2)
        {
            _context = context;
            _siteVerify = siteVerifyV2;
        }

        public ActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Account/DangNhap.cshtml");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken] // Tạm bỏ xác thực để xử lý từ fetch
        public async Task<IActionResult> XuLyDangNhap([FromBody] DangNhapRequest model)
        {
            if(string.IsNullOrEmpty(model.Captcha))
            {
                return Json(new { success = false, message = "Invalid captcha" });
            }

            var response = await _siteVerify.Verify(new reCAPTCHASiteVerifyRequest
            {
                Response = model.Captcha,
                RemoteIp = HttpContext.Connection.RemoteIpAddress.ToString()
            });

            if(! response.Success) 
            {
                return Json(new { success = false, message = "Invalid captcha" });
            }
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ email và mật khẩu " });
            }

            // Tìm người dùng theo email
            var nguoiDung = _context.NguoiDungs.FirstOrDefault(u => u.Email == model.Email);
            if (nguoiDung.IsDeleted == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                return Json(new { success = false, message = "Tài khoản đã bị ngừng hoạt động" , redirectToAccessDenied = true });
            }
            // Kiểm tra người dùng tồn tại và mật khẩu đúng
            if (nguoiDung == null || !BCrypt.Net.BCrypt.Verify(model.Password, nguoiDung.MatKhau))
            {
                return Json(new { success = false, message = "Email hoặc mật khẩu không chính xác " });
            }

            // Lấy thông tin phân quyền
            var phanQuyen = _context.PhanQuyens.Find(nguoiDung.MaQuyen);
            if (phanQuyen == null)
            {
                return Json(new { success = false, message = "Tài khoản không có quyền truy cập " });
            }

            // Tạo claims cho người dùng
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, nguoiDung.HoTen),
                new Claim(ClaimTypes.Email, nguoiDung.Email),
                new Claim(ClaimTypes.Role, phanQuyen.TenQuyen),
                new Claim("IsDeleted", nguoiDung.IsDeleted.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Đăng nhập người dùng bằng cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Lưu thông tin người dùng vào session
            HttpContext.Session.SetInt32("MaNguoiDung", nguoiDung.MaNguoiDung);
            HttpContext.Session.SetString("HoTen", nguoiDung.HoTen);
            HttpContext.Session.SetInt32("MaQuyen", nguoiDung.MaQuyen);
            HttpContext.Session.SetString("AvatarUrl", nguoiDung.AvatarUrl ?? "/asset/avatarUser/default_avatarUser.png");

            // Xác định URL chuyển hướng
            string redirectUrl = model.ReturnUrl;

            if (string.IsNullOrEmpty(redirectUrl) || redirectUrl.Contains("@Url.Action"))
            {
                // Nếu không có returnUrl hoặc returnUrl chứa cú pháp Razor, chuyển hướng về trang Home
                redirectUrl = Url.Action("Index", "Home");
            }
            else if (!Url.IsLocalUrl(redirectUrl))
            {
                // Nếu returnUrl không phải là URL cục bộ (có thể là tấn công), chuyển hướng về trang Home
                redirectUrl = Url.Action("Index", "Home");
            }

            return Json(new
            {
                success = true,
                message = "Đăng nhập thành công ",
                redirectUrl = redirectUrl
            });
        }

        public async Task<IActionResult> DangXuat()
        {
            // Xóa cookie xác thực
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa session
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        public async Task LoginByGoogle(string returnUrl = null)
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse", new { returnUrl })
                });
        }

        public async Task<IActionResult> GoogleResponse(string returnUrl = null)
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null)
            {
                TempData["error"] = "Đăng nhập bằng Google thất bại";
                return RedirectToAction("Index", "DangNhap");
            }

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            if (claims == null)
            {
                TempData["error"] = "Không tìm thấy thông tin người dùng từ Google";
                return RedirectToAction("Index", "DangNhap");
            }

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["error"] = "Không tìm thấy email từ Google";
                return RedirectToAction("Index", "DangNhap");
            }

            // Sử dụng FirstOrDefaultAsync() để kiểm tra người dùng tồn tại
            var existingUser = await _context.NguoiDungs.FirstOrDefaultAsync(n => n.Email == email);

            if (existingUser.IsDeleted == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                TempData["error"] = "Bạn không có quyền để truy cập!";
                return RedirectToAction("AccessDenied", "DangNhap");
            }

            if (existingUser == null)
            {
                // Nếu người dùng không tồn tại, tạo người dùng mới
                var newUser = new NguoiDungModel
                {
                    Email = email,
                    HoTen = name ?? "Google User",
                    SoDienThoai = "", // Có thể để trống hoặc yêu cầu người dùng cập nhật sau
                    MatKhau = "", // Không cần mật khẩu vì đăng nhập bằng Google
                    MaQuyen = 2, // Ví dụ: Gán quyền mặc định là "Người dùng" (2)
                    NgayTao = DateTime.Now,
                    IsDeleted = false
                };

                try
                {
                    _context.NguoiDungs.Add(newUser);
                    var saveResult = await _context.SaveChangesAsync();

                    if (saveResult > 0) // Kiểm tra xem có bản ghi nào được lưu thành công không
                    {
                        TempData["success"] = "Đăng ký thành công với Google";
                        existingUser = newUser;
                    }
                    else
                    {
                        TempData["error"] = "Đăng ký thất bại. Vui lòng thử lại.";
                        return RedirectToAction("Index", "DangNhap");
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Đăng ký thất bại do lỗi: {ex.Message}";
                    return RedirectToAction("Index", "DangNhap");
                }
            }
            else
            {
                TempData["success"] = "Đăng nhập thành công với Google";
            }

            // Lấy thông tin phân quyền
            var phanQuyen = await _context.PhanQuyens.FindAsync(existingUser.MaQuyen);
            if (phanQuyen == null)
            {
                TempData["error"] = "Tài khoản không có quyền truy cập";
                return RedirectToAction("Index", "DangNhap");
            }

            // Tạo claims cho người dùng
            var newClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, existingUser.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, existingUser.HoTen),
                new Claim(ClaimTypes.Email, existingUser.Email),
                new Claim(ClaimTypes.Role, phanQuyen.TenQuyen),
                new Claim("IsDeleted", existingUser.IsDeleted.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Đăng nhập người dùng bằng cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Lưu thông tin người dùng vào session
            HttpContext.Session.SetString("Email", email);
            HttpContext.Session.SetString("TenNguoiDung", name ?? "Google User");
            HttpContext.Session.SetInt32("MaQuyen", existingUser.MaQuyen);
            HttpContext.Session.SetString("AvatarUrl", existingUser.AvatarUrl ?? "/asset/avatarUser/default_avatarUser.png");

            // Chuyển hướng dựa trên returnUrl nếu có
            string message = existingUser == null ? "Đăng ký thành công với Google" : "Đăng nhập thành công với Google";
            string redirectUrl = "/phong-tro";
            redirectUrl = redirectUrl.Contains("?") ? $"{redirectUrl}&success={Uri.EscapeDataString(message)}" : $"{redirectUrl}?success={Uri.EscapeDataString(message)}";

            return Redirect(redirectUrl);
        }

        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            TempData["error"] = "Bạn không có quyền truy cập vào khu vực này.";
            return View("~/Views/Account/AccessDenied.cshtml");
        }

       
    }
}