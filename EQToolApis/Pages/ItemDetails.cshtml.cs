using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    public class ItemDetailsModel : PageModel
    {
        public int ItemId { get; set; }

        public IActionResult OnGet([FromRoute] int itemid)
        {
            ItemId = itemid;
            return Page();
        }
    }
}
