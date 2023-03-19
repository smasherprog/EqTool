using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    [Authorize]
    public class AuthIndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
