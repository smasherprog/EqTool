using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages.Account
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string? returnUrl = null)
        {
            return Challenge(
                new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
                "Discord"
            );
        }
    }
}
