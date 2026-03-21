using System;
using System.Threading.Tasks;
using ASC.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ASC.Web.Data
{
    public class IdentitySeed : IIdentitySeed
    {
        public async Task Seed(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<ApplicationSettings> options)
        {
            // Lấy danh sách roles từ appsettings.json (dạng chuỗi phân cách bởi dấu phẩy)
            var roles = options.Value.Roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Tạo các role nếu chưa tồn tại
            foreach (var role in roles)
            {
                try
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var storageRole = new IdentityRole
                        {
                            Name = role.Trim()
                        };
                        var roleResult = await roleManager.CreateAsync(storageRole);

                        if (!roleResult.Succeeded)
                        {
                            Console.WriteLine("Lỗi tạo role: " + role);
                            foreach (var error in roleResult.Errors)
                            {
                                Console.WriteLine(error.Description);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi xử lý role {role}: {ex.Message}");
                }
            }

            // Tạo tài khoản Admin nếu chưa tồn tại
            var admin = await userManager.FindByEmailAsync(options.Value.AdminEmail);
            if (admin == null)
            {
                var user = new IdentityUser
                {
                    UserName = options.Value.AdminName,
                    Email = options.Value.AdminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, options.Value.AdminPassword);

                if (result.Succeeded)
                {
                    // Thêm claim (nếu cần)
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim(
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                        options.Value.AdminEmail));

                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));

                    // Gán role Admin
                    await userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    Console.WriteLine("Lỗi tạo tài khoản Admin:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(error.Description);
                    }
                }
            }

            // Tạo tài khoản Engineer (Kỹ sư dịch vụ) nếu chưa tồn tại
            var engineer = await userManager.FindByEmailAsync(options.Value.EngineerEmail);
            if (engineer == null)
            {
                var user = new IdentityUser
                {
                    UserName = options.Value.EngineerName,
                    Email = options.Value.EngineerEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                var result = await userManager.CreateAsync(user, options.Value.EngineerPassword);

                if (result.Succeeded)
                {
                    // Thêm claim
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim(
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                        options.Value.EngineerEmail));

                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));

                    // Gán role Engineer
                    await userManager.AddToRoleAsync(user, "Engineer");
                }
                else
                {
                    Console.WriteLine("Lỗi tạo tài khoản Engineer:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(error.Description);
                    }
                }
            }
        }
    }
}