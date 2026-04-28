using ASC.Business;
using ASC.Business.Interfaces;
using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;
using ASC.Web.Configuration;
using ASC.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASC.Web.Services
{
    public static class DependencyInjection
    {
        //Config services
        public static IServiceCollection AddCongfig(this IServiceCollection services, IConfiguration config)
        {
            // Add AddDbContext with connectionString to mirage database
            var connectionString = config.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            //Add Options and get data from appsettings.json with "AppSettings"
            services.AddOptions(); // IOption
            services.Configure<ApplicationSettings>(config.GetSection("AppSettings"));

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = config.GetSection("CacheSettings:CacheConnectionString").Value;
                options.InstanceName = config.GetSection("CacheSettings:CacheInstance").Value;
            });

            return services;
        }

        //Add service
        public static IServiceCollection AddMyDependencyGroup(this IServiceCollection services, IConfiguration config)
        {
            //Add ApplicationDbContext
            services.AddScoped<DbContext, ApplicationDbContext>();

            //Add IdentityUser IdentityRole
            services.AddIdentity<IdentityUser, IdentityRole>((options) =>
            {
                options.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            // CHÚ Ý: Cấu hình Google Auth phải nằm ở đây (SAU AddIdentity)
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = config["Google:Identity:ClientId"] ?? string.Empty;
                    options.ClientSecret = config["Google:Identity:ClientSecret"] ?? string.Empty;
                });

            //Add services

            // Đăng ký cho interface của Identity (Dùng cho Forgot Password, Register,...)
            services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, AuthMessageSender>();

            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddSingleton<IIdentitySeed, IdentitySeed>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // Đảm bảo session hoạt động ngay cả khi chưa accept cookie consent
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDistributedMemoryCache();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();

            services.AddScoped<IMasterDataCacheOperations,  MasterDataCacheOperations>();
            services.AddScoped<IServiceRequestOperations, ServiceRequestOperations>();
            
            services.AddScoped<IServiceRequestMessageOperations, ServiceRequestMessageOperations>();

            services.AddScoped<IOnlineUsersOperations, OnlineUsersOperations>();

            //Add RazorPages , MVC
            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddControllersWithViews();

            services.AddTransient<ASC.Web.Services.IEmailSender, AuthMessageSender>();

            services.AddScoped<IMasterDataOperations, MasterDataOperations>();
            services.AddAutoMapper(typeof(ApplicationDbContext));

            return services;
        }
    }
}