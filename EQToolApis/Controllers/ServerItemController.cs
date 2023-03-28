using EQToolApis.DB;
using EQToolApis.Models;
using EQToolApis.Services;
using Microsoft.AspNetCore.Mvc;

namespace EQToolApis.Controllers
{
    public class ServerItemController : ControllerBase
    {
        private readonly UIDataBuild uIDataBuild;
        private readonly EQToolContext context;

        public ServerItemController(UIDataBuild uIDataBuild, EQToolContext context)
        {
            this.uIDataBuild = uIDataBuild; 
            this.context = context;
        }

        [Route("api/serveritem/{server}")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, VaryByHeader = "server")]
        public List<AuctionItem> Get(Servers server)
        {
#if DEBUG
            if (UIDataBuild.ItemCache[(int)server] == null)
            {
                uIDataBuild.BuildData(server);
            }
            return UIDataBuild.ItemCache[(int)server];

#else
            return UIDataBuild.ItemCache[(int)server];
#endif
        }

        [Route("api/serveritemdetail/{server}/{itemid}")]
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, VaryByHeader = "itemid")]
        public ItemDetail GetItemDetail(Servers server, int itemid)
        {
            ItemDetail Item = new();
            Item.ItemName = context.EQitems
                    .Where(a => a.EQitemId == itemid && a.Server == server)
                    .Select(a => a.ItemName)
                    .FirstOrDefault();

            Item.Items = context.EQTunnelAuctionItems
                .Where(a => a.EQitemId == itemid && a.Server == server)
                .Select(a => new ItemAuctionDetail
                {
                    AuctionType = a.EQTunnelMessage.AuctionType,
                    PlayerName = a.EQTunnelMessage.EQAuctionPlayer.Name,
                    AuctionPrice = a.AuctionPrice,
                    TunnelTimestamp = a.EQTunnelMessage.TunnelTimestamp
                }).ToList()
                .OrderByDescending(a => a.TunnelTimestamp)
                .ToList();
            return Item;
        }
    }
}
