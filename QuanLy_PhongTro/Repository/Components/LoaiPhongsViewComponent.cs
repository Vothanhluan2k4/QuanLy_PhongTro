using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QuanLy_PhongTro.Repository.Components
{
    public class LoaiPhongsViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public LoaiPhongsViewComponent(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.LoaiPhongs.ToListAsync());
    }
}
