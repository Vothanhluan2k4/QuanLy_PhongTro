using QuanLy_PhongTro.Models;

public interface INguoiDungRepositories
{
    Task<IEnumerable<NguoiDungModel>> GetAllAsync();
    Task<NguoiDungModel> GetByIdAsync(int id);
    Task<NguoiDungModel> GetByIdAsyncWithPhanQuyen(int id);
    Task AddAsync(NguoiDungModel nguoiDung);
    Task UpdateAsync(NguoiDungModel nguoiDung);
    Task DeleteAsync(int id);
}