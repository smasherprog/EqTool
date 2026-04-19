using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages.Account
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string? returnUrl = null, int? desktop_port = null)
        {
            var redirectUri = returnUrl ?? "/";
            if (desktop_port.HasValue && desktop_port.Value >= 1024 && desktop_port.Value <= 65535)
            {
                redirectUri = $"/Account/DesktopCallback?port={desktop_port.Value}";
            }
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUri },
                "Discord"
            );
        }
    }
}
