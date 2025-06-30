using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;

namespace QuanLy_PhongTro.Repository
{
    public class EFPhanQuyenRepositories : IPhanQuyenRepositories
    {
        private readonly DataContext _context;

        public EFPhanQuyenRepositories(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PhanQuyenModel>> GetAllAsync()
        {
            return await _context.PhanQuyens.ToListAsync();
        }
    }
}
