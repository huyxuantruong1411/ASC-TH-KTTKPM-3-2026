using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.Web.Configuration; 
using ASC.Web.Data;          
using ASC.Web.Services;      
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ASC.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCongfig(builder.Configuration)
    .AddMyDependencyGroup(builder.Configuration);

builder.Services.AddSignalR();

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

// --- THÊM ĐOẠN NÀY ĐỂ FIX LỖI NGÀY THÁNG ---
var supportedCultures = new[] { "vi-VN", "en-GB" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-GB") // Dùng en-GB hoặc vi-VN để mặc định lấy chuẩn dd/MM/yyyy
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);
// ------------------------------------------

app.UseRouting(); // Dòng cũ của bạn đã có sẵn

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

app.MapHub<ServiceMessagesHub>("/serviceMessagesHub");
app.MapHub<ChatHub>("/chatHub");

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

using (var scope = app.Services.CreateScope())
{
    var masterCacheOperations = scope.ServiceProvider.GetRequiredService<IMasterDataCacheOperations>();
    await masterCacheOperations.CreateMasterDataCacheAsync();
}

app.Run();