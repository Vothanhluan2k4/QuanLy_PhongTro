using QuanLy_PhongTro.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public class EFLoaiPhongRepositories : ILoaiPhongRepositories
    {
        private readonly DataContext _context;

        public EFLoaiPhongRepositories(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LoaiPhongModel>> GetAllAsync()
        {
            return await _context.LoaiPhongs.ToListAsync();
        }

        public async Task<LoaiPhongModel> GetByIdAsync(int id)
        {
            return await _context.LoaiPhongs.FirstOrDefaultAsync(lp => lp.MaLoaiPhong == id);
        }

        public async Task AddAsync(LoaiPhongModel loaiPhong)
        {
            _context.LoaiPhongs.Add(loaiPhong);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LoaiPhongModel loaiPhong)
        {
            _context.LoaiPhongs.Update(loaiPhong);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var loaiPhong = await _context.LoaiPhongs.FirstOrDefaultAsync(lp => lp.MaLoaiPhong == id);
            if (loaiPhong != null)
            {
                _context.LoaiPhongs.Remove(loaiPhong);
                await _context.SaveChangesAsync();
            }
        }
    }
}
