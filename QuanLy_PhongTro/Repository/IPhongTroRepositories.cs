using QuanLy_PhongTro.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public interface IPhongTroRepositories
    {
        Task<List<PhongTroModel>> GetAllAsync();
        Task<PhongTroModel> GetByIdAsync(int id);
        Task AddAsync(PhongTroModel phongTro);
        Task UpdateAsync(PhongTroModel phongTro);
        Task DeleteAsync(int id);
    }
}