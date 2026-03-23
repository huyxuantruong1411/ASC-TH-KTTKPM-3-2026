using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASC.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordEmailConfirmationModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}