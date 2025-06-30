using QuanLy_PhongTro.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public interface IThietBiRepositories
    {
        Task<IEnumerable<ThietBiModel>> GetAllAsync();
        Task<ThietBiModel> GetByIdAsync(int id);
        Task AddAsync(ThietBiModel thietbi);
        Task UpdateAsync(ThietBiModel thietbi);
        Task DeleteAsync(int id);
    }
}
