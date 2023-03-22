using EQToolApis.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    public class ServerData
    {
        public int TotalEQTunnelMessages { get; set; }

        public int TotalEQTunnelAuctionItems { get; set; }

        public DateTimeOffset RecentImportTimeStamp { get; set; }

        public DateTimeOffset OldestImportTimeStamp { get; set; }
    }

    public class AllData
    {
        public int TotalEQAuctionPlayers { get; set; }

        public int TotalUniqueItems { get; set; }

        public ServerData GreenServerData { get; set; } = new ServerData();

        public ServerData BlueServerData { get; set; } = new ServerData();
    }

    public class IndexModel : PageModel
    {
        private readonly EQToolContext context;

        public IndexModel(EQToolContext context)
        {
            this.context = context;
        }

        public AllData AllData = new();


        public IActionResult OnGet()
        {
            AllData = new AllData
            {
                TotalEQAuctionPlayers = context.EQAuctionPlayers.Count(),
                TotalUniqueItems = context.EQitems.Count(),
                GreenServerData = new ServerData
                {
                    OldestImportTimeStamp = context.EQTunnelMessages.Where(a => a.Server == Servers.Green).OrderBy(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
                    RecentImportTimeStamp = context.EQTunnelMessages.Where(a => a.Server == Servers.Green).OrderByDescending(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
                    TotalEQTunnelAuctionItems = context.EQTunnelAuctionItems.Where(a => a.EQTunnelMessage.Server == Servers.Green).Count(),
                    TotalEQTunnelMessages = context.EQTunnelMessages.Where(a => a.Server == Servers.Green).Count()
                },

                BlueServerData = new ServerData
                {
                    OldestImportTimeStamp = context.EQTunnelMessages.Where(a => a.Server == Servers.Blue).OrderBy(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
                    RecentImportTimeStamp = context.EQTunnelMessages.Where(a => a.Server == Servers.Blue).OrderByDescending(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault(),
                    TotalEQTunnelAuctionItems = context.EQTunnelAuctionItems.Where(a => a.EQTunnelMessage.Server == Servers.Blue).Count(),
                    TotalEQTunnelMessages = context.EQTunnelMessages.Where(a => a.Server == Servers.Blue).Count()
                }
            };
            return Page();
        }
    }
}
