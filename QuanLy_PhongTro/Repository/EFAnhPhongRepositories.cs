using QuanLy_PhongTro.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public class EFAnhPhongRepositories : IAnhPhongRepositories
    {
        private readonly DataContext _context;

        public EFAnhPhongRepositories(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnhPhongTroModel>> GetByMaPhongAsync(int maPhong)
        {
            return await _context.AnhPhongTros
                .Where(a => a.MaPhong == maPhong)
                .ToListAsync();
        }

        public async Task<AnhPhongTroModel> GetByIdAsync(int id)
        {
            return await _context.AnhPhongTros.FindAsync(id);
        }

        public async Task AddAsync(AnhPhongTroModel anh)
        {
            await _context.AnhPhongTros.AddAsync(anh);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AnhPhongTroModel anh)
        {
            _context.AnhPhongTros.Update(anh);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var anh = await _context.AnhPhongTros.FindAsync(id);
            if (anh != null)
            {
                _context.AnhPhongTros.Remove(anh);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetAsDefaultAsync(int id)
        {
            var anh = await _context.AnhPhongTros.FindAsync(id);
            if (anh != null)
            {
                // Reset ảnh đại diện hiện tại
                var all = _context.AnhPhongTros.Where(a => a.MaPhong == anh.MaPhong);
                foreach (var item in all)
                {
                    item.LaAnhDaiDien = false;
                }

                // Đặt ảnh này làm đại diện
                anh.LaAnhDaiDien = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
