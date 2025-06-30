using QuanLy_PhongTro.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public interface IAnhPhongRepositories
    {
        Task<IEnumerable<AnhPhongTroModel>> GetByMaPhongAsync(int maPhong);
        Task<AnhPhongTroModel> GetByIdAsync(int id);
        Task AddAsync(AnhPhongTroModel anh);
        Task UpdateAsync(AnhPhongTroModel anh);
        Task DeleteAsync(int id);
        Task SetAsDefaultAsync(int id); // Đặt làm ảnh đại diện
    }
}
