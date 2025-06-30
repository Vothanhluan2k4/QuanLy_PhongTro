using QuanLy_PhongTro.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public interface ILoaiPhongRepositories
    {
        Task<IEnumerable<LoaiPhongModel>> GetAllAsync();
        Task<LoaiPhongModel> GetByIdAsync(int id);
        Task AddAsync(LoaiPhongModel loaiPhong);
        Task UpdateAsync(LoaiPhongModel loaiphong);
        Task DeleteAsync(int id);
    }
}
