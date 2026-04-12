using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.Web.Configuration; 
using ASC.Web.Data;          
using ASC.Web.Services;      
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCongfig(builder.Configuration)
    .AddMyDependencyGroup(builder.Configuration);

var app = builder.Build();

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

app.MapRazorPages();

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

//Create Navigation Cache
using (var scope = app.Services.CreateScope())
{
    var navigationCacheOperations = scope.ServiceProvider.GetRequiredService<INavigationCacheOperations>();
    await navigationCacheOperations.CreateNavigationCacheAsync();
}

app.Run();