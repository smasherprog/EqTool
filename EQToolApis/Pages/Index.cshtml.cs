using EQToolApis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
    public class IndexModel : PageModel
    {
        public IndexModel()
        {
        }

        public AllData AllData => UIDataBuild.AllData;

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
