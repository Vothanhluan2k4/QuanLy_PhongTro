using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.Repository;

public class NhomPhongsViewComponent : ViewComponent
{
    private readonly DataContext _context;

    public NhomPhongsViewComponent(DataContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(int page = 1)
    {
        const int PageSize = 6;

        // Giả sử MaNguoiDung được lấy từ hệ thống đăng nhập
        int maNguoiDung = 1; // Thay bằng logic lấy MaNguoiDung từ HttpContext.User (nếu có hệ thống đăng nhập)

        // Lấy danh sách phòng với các quan hệ
        var query = _context.PhongTros
            .Include(p => p.AnhPhongTros)
            .Include(p => p.PhongTroThietBis)
                .ThenInclude(ptb => ptb.ThietBi)
            .Include(p => p.LoaiPhong)
            .OrderBy(p => p.MaPhong);

        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = 0;
            ViewBag.TotalItems = 0;
            return View(Enumerable.Empty<IGrouping<LoaiPhongModel, PhongTroModel>>());
        }

        var phongTros = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        // Kiểm tra trạng thái lưu phòng cho từng phòng
        foreach (var phongTro in phongTros)
        {
            var isSaved = await _context.LuuPhongs
                .AnyAsync(lp => lp.MaPhong == phongTro.MaPhong && lp.MaNguoiDung == maNguoiDung);
            // Lưu trạng thái vào ViewData với key là MaPhong
            ViewData[$"IsSaved_{phongTro.MaPhong}"] = isSaved;
        }

        var nhomPhong = phongTros.GroupBy(p => p.LoaiPhong ?? new LoaiPhongModel { TenLoaiPhong = "Không xác định" });

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
        ViewBag.TotalItems = totalItems;

        return View(nhomPhong);
    }
}