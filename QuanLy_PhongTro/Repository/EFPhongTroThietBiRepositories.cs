using QuanLy_PhongTro.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public class EFPhongTroThietBiRepositories : IPhongTroThietBiRepositories
    {
        private readonly DataContext _context;

        public EFPhongTroThietBiRepositories(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PhongTro_ThietBiModel>> GetAllAsync()
        {
            return await _context.PhongTro_ThietBiModels
                .Include(p => p.ThietBi)
                .Include(p => p.PhongTro)
                .ToListAsync();
        }

        public async Task<PhongTro_ThietBiModel> GetByIdAsync(int id)
        {
            return await _context.PhongTro_ThietBiModels
                .Include(p => p.ThietBi)
                .Include(p => p.PhongTro)
                .FirstOrDefaultAsync(p => p.MaPhongThietBi == id);
        }

        public async Task<IEnumerable<PhongTro_ThietBiModel>> GetByMaPhongAsync(int maPhong)
        {
            return await _context.PhongTro_ThietBiModels
                .Where(p => p.MaPhong == maPhong)
                .Include(p => p.ThietBi)
                .ToListAsync();
        }

        public async Task AddAsync(PhongTro_ThietBiModel item)
        {
            await _context.PhongTro_ThietBiModels.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PhongTro_ThietBiModel item)
        {
            _context.PhongTro_ThietBiModels.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.PhongTro_ThietBiModels.FindAsync(id);
            if (item != null)
            {
                _context.PhongTro_ThietBiModels.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
