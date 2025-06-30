using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QuanLy_PhongTro.Repository.Components
{
    public class PhongTrosViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public PhongTrosViewComponent(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.PhongTros.ToListAsync());
    }
}
