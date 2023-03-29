using EQToolApis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    public class IndexModel : PageModel
    {
        public readonly AllData AllData;
        public IndexModel(AllData allData)
        {
            AllData = allData;
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
