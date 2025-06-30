using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;

namespace QuanLy_PhongTro.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            if (!_context.Database.CanConnect())
            {
                _context.Database.Migrate();
            }
            
        }
    }
}