using Microsoft.AspNetCore.WebUtilities;
using System.Text;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASC.Web.Services;
using ASC.Utilities;

namespace ASC.Web.Areas.Identity.Pages.Account
{
    public class InitiateResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ASC.Web.Services.IEmailSender _emailSender;

        public InitiateResetPasswordModel(UserManager<IdentityUser> userManager, ASC.Web.Services.IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // 1. Lấy email của người dùng đang đăng nhập
            var userEmail = HttpContext.User.GetCurrentUserDetails()?.Email;

            if (string.IsNullOrEmpty(userEmail))
            {
                // Nếu không có thông tin đăng nhập, có thể chuyển hướng về trang chủ hoặc trang lỗi
                return RedirectToPage("/Index");
            }

            // 2. Tìm user trong Database
            var user = await _userManager.FindByEmailAsync(userEmail);

            // Chỉ thực hiện tạo code và gửi mail nếu user thực sự tồn tại
            if (user != null)
            {
                // 3. Tạo mã code và Link reset password
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                // 4. Gọi thư viện gửi Mail
                await _emailSender.SendEmailAsync(
                    userEmail,
                    "Reset Password",
                    $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");
            }

            // 5. Trả về giao diện Page hiện tại (Sẽ hiển thị câu "Please check your email...")
            // Dù user == null thì vẫn trả về Page này để bảo mật (tránh lỗi dò quét tài khoản)
            return Page();
        }
    }
}