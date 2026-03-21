using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ASC.Utilities
{
    public static class ClaimsPrincipalExtensions
    {
        // Thêm dấu ? vào CurrentUser? và ClaimsPrincipal? để cho phép null
        public static CurrentUser? GetCurrentUserDetails(this ClaimsPrincipal? principal)
        {
            // Kiểm tra principal null trước để tránh lỗi
            if (principal == null || !principal.Claims.Any())
                return null;

            // Lấy chuỗi cấu hình IsActive an toàn
            var isActiveClaim = principal.Claims.FirstOrDefault(c => c.Type == "IsActive")?.Value;

            // Dùng bool.TryParse thay vì Boolean.Parse để tránh crash nếu value bị null hoặc không hợp lệ
            bool.TryParse(isActiveClaim, out bool isActive);

            return new CurrentUser
            {
                // Dùng ?.Value và ?? string.Empty để gán chuỗi rỗng nếu không tìm thấy Claim
                Name = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty,
                Email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty,
                Roles = principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray(),
                IsActive = isActive
            };
        }
    }
}