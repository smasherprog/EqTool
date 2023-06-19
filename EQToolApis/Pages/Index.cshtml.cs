using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    public class IndexModel : PageModel
    {
        public readonly DBData AllData;
        private readonly EQToolContext eQToolContext;
        public IndexModel(DBData allData, EQToolContext eQToolContext)
        {
            AllData = allData;
            this.eQToolContext = eQToolContext;
        }

        public ServerMessage ServerMessage { get; set; } = new ServerMessage();
        public IActionResult OnGet()
        {
            ServerMessage = eQToolContext.ServerMessages.FirstOrDefault();
            return Page();
        }
    }
}
