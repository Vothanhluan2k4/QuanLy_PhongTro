using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QuanLy_PhongTro.Repository.Components
{
    public class AnhPhongTrosViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;
        public AnhPhongTrosViewComponent(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.AnhPhongTros.ToListAsync());
    }
}
