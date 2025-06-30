using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QuanLy_PhongTro.Repository.Components
{
    public class ThietBisViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public ThietBisViewComponent(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.ThietBis.ToListAsync());
    }
}
