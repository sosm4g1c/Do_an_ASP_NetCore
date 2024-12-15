using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

var connectionString = builder.Configuration.GetConnectionString("MySql");
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddDistributedMemoryCache();  // Đăng ký dịch vụ lưu cache trong bộ nhớ (Session sẽ sử dụng nó)
builder.Services.AddSession(cfg =>
{           // Đăng ký dịch vụ Session
    cfg.Cookie.Name = "bookStore";            // Đặt tên Session - tên này sử dụng ở Browser (Cookie)
    cfg.IdleTimeout = new TimeSpan(30, 00, 00, 0);  // Thời gian tồn tại của Session
    cfg.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Cookie settings
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/accessdenied";
        options.SlidingExpiration = true;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration.GetSection("Authentication:Google:ClientId").Value;
        options.ClientSecret = builder.Configuration.GetSection("Authentication:Google:ClientSecret").Value;
    });

builder.Services.AddTransient<IDbContext, AppDbContext>();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserConfig, UserConfig>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IMailService, MailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapDefaultControllerRoute();

app.UseSession();

app.UseRotativa();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
