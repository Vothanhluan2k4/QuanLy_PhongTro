using QuanLy_PhongTro.Models;
using System.Threading.Tasks;

namespace QuanLy_PhongTro.Repository
{
    public interface IPhanQuyenRepositories
    {
        Task<IEnumerable<PhanQuyenModel>> GetAllAsync();
    }
}
