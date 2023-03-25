using EQToolApis.DB;
using EQToolApis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{

    public class IndexModel : PageModel
    {
        private readonly EQToolContext context;
        private readonly UIDataBuild uIDataBuild;

        public IndexModel(EQToolContext context, UIDataBuild uIDataBuild)
        {
            this.context = context;
            this.uIDataBuild = uIDataBuild;
        }

        public AllData AllData
        {
            get
            {
#if DEBUG
                uIDataBuild.BuildSummaryData();
                return UIDataBuild.AllData;
#else      
return UIDataBuild.AllData;
#endif
            }
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
