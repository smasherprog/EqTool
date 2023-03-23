using EQToolApis.DB;
using EQToolApis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{

    public class IndexModel : PageModel
    {
        private readonly EQToolContext context;

        public IndexModel(EQToolContext context)
        {
            this.context = context;
        }

        public AllData AllData => UIDataBuild.AllData;

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
