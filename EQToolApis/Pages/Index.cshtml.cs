using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    public class IndexModel : PageModel
    {
        public readonly AllData AllData;
        private readonly EQToolContext eQToolContext;
        public IndexModel(AllData allData, EQToolContext eQToolContext)
        {
            AllData = allData;
            this.eQToolContext = eQToolContext;
        }

        public ServerMessage ServerMessage { get; set; }
        public IActionResult OnGet()
        {
            ServerMessage = eQToolContext.ServerMessages.FirstOrDefault();
            return Page();
        }
    }
}
