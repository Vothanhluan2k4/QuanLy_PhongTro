using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using QuanLy_PhongTro.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace QuanLy_PhongTro.Repository
{
    public class EFNguoiDungRepositories : INguoiDungRepositories
    {
        private readonly DataContext _context;

        public EFNguoiDungRepositories(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NguoiDungModel>> GetAllAsync()
        {
            return await _context.NguoiDungs.Include(u => u.PhanQuyen).ToListAsync();
               
        }

        public async Task<NguoiDungModel> GetByIdAsync(int id)
        {
            return await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.MaNguoiDung == id);
        }

        public async Task<NguoiDungModel> GetByIdAsyncWithPhanQuyen(int id)
        {
            return await _context.NguoiDungs
                .Include(u => u.PhanQuyen)
                .FirstOrDefaultAsync(u => u.MaNguoiDung == id);
        }

        public async Task AddAsync(NguoiDungModel nguoiDung)
        {
            if (!string.IsNullOrEmpty(nguoiDung.MatKhau))
            {
                nguoiDung.MatKhau = BCrypt.Net.BCrypt.HashPassword(nguoiDung.MatKhau);
            }
            else
            {
                throw new ArgumentException("Mật khẩu không được để trống.");
            }

            await _context.NguoiDungs.AddAsync(nguoiDung);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(NguoiDungModel nguoiDung)
        {
            _context.NguoiDungs.Update(nguoiDung);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var nguoiDung = await GetByIdAsync(id);
            if (nguoiDung != null)
            {
                nguoiDung.IsDeleted = true; // Soft delete
                await UpdateAsync(nguoiDung);
            }
        }
    }
}
