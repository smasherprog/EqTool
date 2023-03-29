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
        private readonly PlayerCache playerCache;
        public ServerItemController(UIDataBuild uIDataBuild, EQToolContext context, PlayerCache playerCache)
        {
            this.uIDataBuild = uIDataBuild;
            this.context = context;
            this.playerCache = playerCache;
        }

        [Route("api/serveritem/{server}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByHeader = "server")]
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
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByHeader = "itemid")]
        public ItemDetail GetItemDetail(Servers server, int itemid)
        {
            var items = context.EQTunnelAuctionItems
                .Where(a => a.EQitemId == itemid && a.Server == server)
                .Select(a => new
                {
                    a.EQTunnelMessage.AuctionType,
                    a.AuctionPrice,
                    a.EQTunnelMessage.EQAuctionPlayerId,
                    a.EQTunnelMessage.TunnelTimestamp
                }).ToList();

            ItemDetail Item = new()
            {
                ItemName = context.EQitems
                    .Where(a => a.EQitemId == itemid && a.Server == server)
                    .Select(a => a.ItemName)
                    .FirstOrDefault(),
                Items = new List<ItemAuctionDetail>()
            };
            playerCache.PlayersLock.EnterReadLock();
            try
            {
                foreach (var item in items)
                {
                    Item.Items.Add(new ItemAuctionDetail
                    {
                        AuctionType = item.AuctionType,
                        PlayerName = playerCache.Players[item.EQAuctionPlayerId].Name,
                        AuctionPrice = item.AuctionPrice,
                        TunnelTimestamp = item.TunnelTimestamp
                    });
                }
            }
            finally
            {
                playerCache.PlayersLock.ExitReadLock();
            }
            Item.Items = Item.Items.OrderByDescending(a => a.TunnelTimestamp).ToList();
            return Item;
        }
    }
}
