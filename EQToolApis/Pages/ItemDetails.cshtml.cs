using EQToolApis.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    public class ItemDetailsModel : PageModel
    {
        public int ItemId { get; set; }

        public Servers Server { get; set; }

        public IActionResult OnGet([FromRoute] int itemid, [FromRoute] Servers server)
        {
            ItemId = itemid;
            Server = server;
            return Page();
        }
    }
}
