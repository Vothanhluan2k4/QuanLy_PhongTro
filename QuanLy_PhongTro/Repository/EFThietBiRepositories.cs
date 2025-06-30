using QuanLy_PhongTro.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public class EFThietBiRepositories : IThietBiRepositories
    {
        private readonly DataContext _context;

        public EFThietBiRepositories(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ThietBiModel>> GetAllAsync()
        {
            return await _context.ThietBis.ToListAsync();
        }

        public async Task<ThietBiModel> GetByIdAsync(int id)
        {
            return await _context.ThietBis.FirstOrDefaultAsync(tb => tb.MaThietBi == id);
        }

        public async Task AddAsync(ThietBiModel thietbi)
        {
            _context.ThietBis.Add(thietbi);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ThietBiModel thietbi)
        {
            _context.ThietBis.Update(thietbi);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var thietbi = await _context.ThietBis.FirstOrDefaultAsync(tb => tb.MaThietBi == id);
            if (thietbi != null)
            {
                _context.ThietBis.Remove(thietbi);
                await _context.SaveChangesAsync();
            }
        }
    }
}