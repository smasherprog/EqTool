using EQToolApis.DB;
using EQToolApis.Models;
using EQToolApis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, VaryByHeader = "server")]
    public class ServerIndexModel : PageModel
    {
        private readonly UIDataBuild uIDataBuild;

        public ServerIndexModel(UIDataBuild uIDataBuild)
        {
            this.uIDataBuild = uIDataBuild;
        }
#if DEBUG
        public List<AuctionItem> AuctionItems
        {
            get
            {
                if (UIDataBuild.ItemCache[(int)Server] == null)
                {
                    uIDataBuild.BuildData(Server);
                }
                return UIDataBuild.ItemCache[(int)Server];
            }
        }
#else
        public List<AuctionItem> AuctionItems => UIDataBuild.ItemCache[(int)Server];
#endif 

        public Servers Server { get; set; }

        public IActionResult OnGet([FromRoute] Servers server)
        {
            Server = server;
            return Page();
        }
    }
}
