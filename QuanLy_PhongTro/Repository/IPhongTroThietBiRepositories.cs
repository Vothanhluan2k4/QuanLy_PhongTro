using QuanLy_PhongTro.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public interface IPhongTroThietBiRepositories
    {
        Task<IEnumerable<PhongTro_ThietBiModel>> GetAllAsync();
        Task<PhongTro_ThietBiModel> GetByIdAsync(int id);
        Task<IEnumerable<PhongTro_ThietBiModel>> GetByMaPhongAsync(int maPhong);
        Task AddAsync(PhongTro_ThietBiModel item);
        Task UpdateAsync(PhongTro_ThietBiModel item);
        Task DeleteAsync(int id);
    }
}