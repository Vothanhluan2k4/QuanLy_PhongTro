using QuanLy_PhongTro.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLy_PhongTro.Repository;

namespace QuanLy_PhongTro.Repositories
{
    public class EFHopDongRepositories : IHopDongRepositories
    {
        private readonly DataContext _context;

        public EFHopDongRepositories(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HopDongModel>> GetAllAsync()
        {
            return await _context.HopDongs
                .Include(hd => hd.PhongTro)
                .Include(hd => hd.NguoiDung)
                .ToListAsync();
        }

        public async Task<HopDongModel> GetByIdAsync(int id)
        {
            return await _context.HopDongs
                .Include(hd => hd.PhongTro)
                .Include(hd => hd.NguoiDung)
                .FirstOrDefaultAsync(hd => hd.MaHopDong == id);
        }

        public async Task AddAsync(HopDongModel hopDong)
        {
            if (!hopDong.IsValid())
            {
                throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            }

            _context.HopDongs.Add(hopDong);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(HopDongModel hopDong)
        {
            var existingHopDong = await _context.HopDongs.FindAsync(hopDong.MaHopDong);
            if (existingHopDong == null)
            {
                throw new KeyNotFoundException("Không tìm thấy hợp đồng với ID này.");
            }

            if (!hopDong.IsValid())
            {
                throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            }

            existingHopDong.MaPhong = hopDong.MaPhong;
            existingHopDong.MaNguoiDung = hopDong.MaNguoiDung;
            existingHopDong.NgayBatDau = hopDong.NgayBatDau;
            existingHopDong.NgayKetThuc = hopDong.NgayKetThuc;
            existingHopDong.TienCoc = hopDong.TienCoc;
            existingHopDong.TrangThai = hopDong.TrangThai;
            existingHopDong.NgayKy = hopDong.NgayKy;
            existingHopDong.GhiChu = hopDong.GhiChu;

            _context.HopDongs.Update(existingHopDong);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);
            if (hopDong == null)
            {
                throw new KeyNotFoundException("Không tìm thấy hợp đồng với ID này.");
            }

            _context.HopDongs.Remove(hopDong);
            await _context.SaveChangesAsync();
        }
    }
}