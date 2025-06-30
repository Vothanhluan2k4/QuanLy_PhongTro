using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Owl.reCAPTCHA;
using QuanLy_PhongTro.Models.Momo;
using QuanLy_PhongTro.Repositories;
using QuanLy_PhongTro.Repository;
using QuanLy_PhongTro.Services;
using QuanLy_PhongTro.Services.Momo;
using QuanLy_PhongTro.Services.Vnpay;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();



// Connect Db
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
});

// Thêm d?ch v? xác th?c và c?u hình Cookie + Google Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // S? d?ng Google cho challenge
})
.AddCookie(options =>
{
    options.LoginPath = "/DangNhap/Index";
    options.AccessDeniedPath = "/DangNhap/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value ?? throw new InvalidOperationException("GoogleKeys:ClientId is missing in configuration.");
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value ?? throw new InvalidOperationException("GoogleKeys:ClientSecret is missing in configuration.");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddreCAPTCHAV2(x =>
{
    x.SiteKey = "6LcfSz4rAAAAAFVqX0lm3Y49Eim3vXoKWuIGeMCM";
    x.SiteSecret = "6LcfSz4rAAAAANKRc0Rl7KTYQ6dq7ROoLelXC1Hu";
});

//Momo API Payment
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

// C?u hình EmailService
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IPhongTroRepositories, EFPhongTroRepositories>();
builder.Services.AddScoped<ILoaiPhongRepositories, EFLoaiPhongRepositories>();
builder.Services.AddScoped<IThietBiRepositories, EFThietBiRepositories>();
builder.Services.AddScoped<IPhongTroThietBiRepositories, EFPhongTroThietBiRepositories>();
builder.Services.AddScoped<IAnhPhongRepositories, EFAnhPhongRepositories>();
builder.Services.AddScoped<INguoiDungRepositories, EFNguoiDungRepositories>();
builder.Services.AddScoped<IPhanQuyenRepositories, EFPhanQuyenRepositories>();
builder.Services.AddScoped<IHopDongRepositories, EFHopDongRepositories>();

//Conncet VNpay API
builder.Services.AddScoped<IVnPayService, VnPayService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            var dbContext = context.RequestServices.GetRequiredService<DataContext>();
            var user = await dbContext.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user?.IsDeleted == true)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Session.Clear();
                context.Response.Redirect("/DangNhap/AccessDenied");
                return;
            }
        }
    }
    await next();
});

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=PhongTro}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seeding Data
var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedingData(context);

app.Run();