using EQToolShared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{

    public class ServerIndexModel : PageModel
    {
        public Servers Server { get; set; }

        public IActionResult OnGet([FromRoute] Servers server)
        {
            Server = server;
            return Page();
        }
    }
}
