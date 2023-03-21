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
#if DEBUG
        public List<AuctionItem> AuctionItems
        {
            get
            {
                if (!UIDataBuild.ItemCache.Any())
                {
                    this.uIDataBuild.BuildData();
                }
                return UIDataBuild.ItemCache; 
            }
        }
#else
        public List<AuctionItem> AuctionItems => UIDataBuild.ItemCache;
#endif 
        public int TotalEQAuctionPlayers => context.EQAuctionPlayers.Count();
        public int TotalEQitems => context.EQitems.Count();
        public int TotalEQTunnelAuctionItems => context.EQTunnelAuctionItems.Count();
        public int TotalEQTunnelMessages => context.EQTunnelMessages.Count();

        public DateTimeOffset LastSeen => context.EQTunnelMessages.OrderBy(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault();

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
