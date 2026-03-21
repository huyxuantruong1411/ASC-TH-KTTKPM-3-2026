using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.Web.Configuration; 
using ASC.Web.Data;          
using ASC.Web.Services;      
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CẤU HÌNH DATABASE & IDENTITY
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

/*
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
*/

// ==========================================
// 2. CẤU HÌNH APP SETTINGS (TỪ APPSettings.json)
// ==========================================
builder.Services.AddOptions();
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("AppSettings"));

// ==========================================
// 3. ĐĂNG KÝ SERVICES THÔNG QUA EXTENSION METHODS 
// ==========================================
// Lưu ý: Đảm bảo bạn đã định nghĩa các extension method này trong dự án
builder.Services
    .AddCongfig(builder.Configuration)
    .AddMyDependencyGroup();

// ==========================================
// 4. ĐĂNG KÝ CÁC SERVICES CỐT LÕI (DI)
// ==========================================
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();
builder.Services.AddTransient<ISmsSender, AuthMessageSender>();
builder.Services.AddSingleton<IIdentitySeed, IdentitySeed>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// ==========================================
// 5. CẤU HÌNH SESSION, CACHE & CONTROLLERS
// ==========================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Đảm bảo session hoạt động ngay cả khi chưa accept cookie consent
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ==========================================
// 6. CẤU HÌNH HTTP REQUEST PIPELINE (MIDDLEWARE)
// ==========================================
if (app.Environment.IsDevelopment())
{
    // Dùng cho môi trường dev
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // Giá trị HSTS mặc định là 30 ngày. 
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thứ tự Middleware rất quan trọng: Session -> Authentication -> Authorization
app.UseSession();
app.UseAuthentication(); // Bắt buộc phải có trước Authorization
app.UseAuthorization();

// Routing cho các Area 
app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}");

// Routing mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map các trang Razor Pages (dùng cho Identity)
app.MapRazorPages();

// ==========================================
// 7. SEED DỮ LIỆU MẪU LÊN CSDL TỪ APPSETTINGS
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var storageSeed = scope.ServiceProvider.GetRequiredService<IIdentitySeed>();

    // Dùng GetRequiredService thay vì GetService để đảm bảo ứng dụng báo lỗi rõ ràng nếu thiếu dependency
    await storageSeed.Seed(
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetRequiredService<IOptions<ApplicationSettings>>()
    );
}

app.Run();