using Microsoft.EntityFrameworkCore;
using QuanLy_PhongTro.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public class EFPhongTroRepositories : IPhongTroRepositories
    {
        private readonly DataContext _context;

        public EFPhongTroRepositories(DataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<PhongTroModel>> GetAllAsync()
        {
            return await _context.PhongTros
                .Include(pt => pt.LoaiPhong) // Tải thông tin Loại Phòng
                .Include(pt => pt.PhongTroThietBis) // Tải danh sách thiết bị
                    .ThenInclude(pttb => pttb.ThietBi) // Tải thông tin thiết bị
                .Include(pt => pt.AnhPhongTros) // Tải danh sách ảnh
                .ToListAsync();
        }

        public async Task<PhongTroModel> GetByIdAsync(int id)
        {
            try
            {
                var phong = await _context.PhongTros
                    .Include(pt => pt.LoaiPhong)
                    .Include(pt => pt.PhongTroThietBis)
                        .ThenInclude(pttb => pttb.ThietBi)
                    .Include(pt => pt.AnhPhongTros)
                    .FirstOrDefaultAsync(pt => pt.MaPhong == id);

                if (phong == null)
                {
                    Console.WriteLine($"Không tìm thấy phòng với MaPhong: {id}");
                    return null;
                }

                // Kiểm tra các thuộc tính liên quan để tránh lỗi null
                if (phong.LoaiPhong == null)
                {
                    Console.WriteLine($"Phòng {id} không có LoaiPhong hợp lệ (MaLoaiPhong: {phong.MaLoaiPhong})");
                    phong.LoaiPhong = new LoaiPhongModel(); // Gán giá trị mặc định nếu cần
                }

                if (phong.PhongTroThietBis == null)
                {
                    Console.WriteLine($"Phòng {id} không có danh sách thiết bị");
                    phong.PhongTroThietBis = new List<PhongTro_ThietBiModel>();
                }

                if (phong.AnhPhongTros == null)
                {
                    Console.WriteLine($"Phòng {id} không có danh sách ảnh");
                    phong.AnhPhongTros = new List<AnhPhongTroModel>();
                }

                // Kiểm tra từng thiết bị trong danh sách
                if (phong.PhongTroThietBis.Any(pttb => pttb.ThietBi == null))
                {
                    Console.WriteLine($"Phòng {id} có thiết bị không hợp lệ trong danh sách PhongTroThietBis");
                    phong.PhongTroThietBis = phong.PhongTroThietBis.Where(pttb => pttb.ThietBi != null).ToList();
                }

                return phong;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong repository khi lấy phòng ID {id}: {ex.Message}\nStack Trace: {ex.StackTrace}");
                throw; // Ném lại ngoại lệ để controller bắt
            }
        }

        public async Task AddAsync(PhongTroModel phongTro)
        {
            await _context.PhongTros.AddAsync(phongTro);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PhongTroModel phongTro)
        {
            _context.PhongTros.Update(phongTro);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var phongTro = await _context.PhongTros.FindAsync(id);
            if (phongTro != null)
            {
                _context.PhongTros.Remove(phongTro);
                await _context.SaveChangesAsync();
            }
        }
    }
}