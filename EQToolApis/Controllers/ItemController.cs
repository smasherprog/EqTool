using EQToolApis.DB;
using EQToolApis.Models;
using EQToolApis.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;

namespace EQToolApis.Controllers
{
    public class ItemController : ControllerBase
    {
        private readonly UIDataBuild uIDataBuild;
        private readonly EQToolContext context;
        private readonly PlayerCache playerCache;
        public ItemController(UIDataBuild uIDataBuild, EQToolContext context, PlayerCache playerCache)
        {
            this.uIDataBuild = uIDataBuild;
            this.context = context;
            this.playerCache = playerCache;
        }
        /// <summary>
        /// Will get all items for server and the averages. This data is rebuild every 10 minutes.
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        [Route("api/item/getall/{server}/{top}/")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        public List<AuctionItem> Get([DefaultValue("Green")] Servers server, [DefaultValue(100)] int? top)
        {
            List<AuctionItem> ret;
#if DEBUG
            if (UIDataBuild.ItemCache[(int)server] == null)
            {
                uIDataBuild.BuildData(server);
            }
            ret = UIDataBuild.ItemCache[(int)server];

#else
            ret = UIDataBuild.ItemCache[(int)server];
#endif
            if (top.HasValue)
            {
                return ret.OrderBy(a => a.n).Take(top.Value).ToList();
            }

            return ret;
        }

        [Route("api/item/getdetails/{server}/{itemname}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        public ItemDetail GetItemDetail([DefaultValue("Green")] Servers server, [DefaultValue("A Black Throne")] string itemname)
        { 
            var items = context.EQTunnelAuctionItems
                .Where(a => a.EQitem.ItemName == itemname && a.EQitem.Server == server)
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
                    .Where(a => a.ItemName == itemname && a.Server == server)
                    .Select(a => a.ItemName)
                    .FirstOrDefault(),
                Items = new List<ItemAuctionDetail>(),
                Players = new Dictionary<int, string>()
            };
            playerCache.PlayersLock.EnterReadLock();
            try
            {
                foreach (var i in items)
                {
                    Item.Items.Add(new ItemAuctionDetail
                    {
                        u = i.AuctionType,
                        i = i.EQAuctionPlayerId,
                        p = i.AuctionPrice,
                        t = i.TunnelTimestamp
                    });
                    Item.Players.TryAdd(i.EQAuctionPlayerId, playerCache.Players[i.EQAuctionPlayerId].Name);
                }
            }
            finally
            {
                playerCache.PlayersLock.ExitReadLock();
            }
            Item.Items = Item.Items.OrderByDescending(a => a.t).ToList();
            return Item;
        }

        /// <summary>
        /// Will get an item and all of its details. This can include alot of data!
        /// </summary>
        /// <param name="itemid"></param>
        /// <returns></returns>
        [Route("api/item/getdetails/{itemid}")]
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        public ItemDetail GetItemDetail([DefaultValue(368)] int itemid)
        { 
            var items = context.EQTunnelAuctionItems
                .Where(a => a.EQitemId == itemid)
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
                    .Where(a => a.EQitemId == itemid)
                    .Select(a => a.ItemName)
                    .FirstOrDefault(),
                Items = new List<ItemAuctionDetail>(),
                Players = new Dictionary<int, string>()
            };
            playerCache.PlayersLock.EnterReadLock();
            try
            {
                foreach (var i in items)
                {
                    Item.Items.Add(new ItemAuctionDetail
                    {
                        u = i.AuctionType,
                        i = i.EQAuctionPlayerId,
                        p = i.AuctionPrice,
                        t = i.TunnelTimestamp
                    });
                    Item.Players.TryAdd(i.EQAuctionPlayerId, playerCache.Players[i.EQAuctionPlayerId].Name);
                }
            }
            finally
            {
                playerCache.PlayersLock.ExitReadLock();
            }
            Item.Items = Item.Items.OrderByDescending(a => a.t).ToList();
            return Item;
        }
    }
}
