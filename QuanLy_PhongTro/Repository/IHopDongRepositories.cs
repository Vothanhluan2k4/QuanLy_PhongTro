using QuanLy_PhongTro.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repositories
{
    public interface IHopDongRepositories
    {
        Task<IEnumerable<HopDongModel>> GetAllAsync();
        Task<HopDongModel> GetByIdAsync(int id);
        Task AddAsync(HopDongModel hopDong);
        Task UpdateAsync(HopDongModel hopDong);
        Task DeleteAsync(int id);
    }
}