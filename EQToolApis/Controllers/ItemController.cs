using EQToolApis.DB;
using EQToolApis.Models;
using EQToolApis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [Route("api/item/getall/{server}/")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        public List<AuctionItem> Get([DefaultValue(Servers.Green)] Servers server)
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
            return ret;
        }

        /// <summary>
        /// Use this API when you want just the averages and counts for the item. This is MUCH faster than the getdetails function and should be preferred.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="itemname"></param>
        /// <returns></returns>
        [Route("api/item/get/{server}/{itemname}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        public Item GetItem([DefaultValue(Servers.Green)] Servers server, [DefaultValue("10 Dose Greater Potion of Purity")] string itemname)
        {
            var item = context.EQitems
                        .Where(a => a.Server == server && a.ItemName == itemname)
                        .Select(a => new Item
                        {
                            EQitemId = a.EQitemId,
                            ItemName = a.ItemName,
                            TotalWTSLast30DaysAverage = a.TotalWTSLast30DaysAverage,
                            TotalWTSLast30DaysCount = a.TotalWTSLast30DaysCount,
                            TotalWTSLast60DaysCount = a.TotalWTSLast60DaysCount,
                            TotalWTSLast60DaysAverage = a.TotalWTSLast60DaysAverage,
                            TotalWTSLast90DaysCount = a.TotalWTSLast90DaysCount,
                            TotalWTSLast90DaysAverage = a.TotalWTSLast90DaysAverage,
                            TotalWTSLast6MonthsCount = a.TotalWTSLast6MonthsCount,
                            TotalWTSLast6MonthsAverage = a.TotalWTSLast6MonthsAverage,
                            TotalWTSAuctionCount = a.TotalWTSAuctionCount,
                            TotalWTSAuctionAverage = a.TotalWTSAuctionAverage,
                            TotalWTSLastYearAverage = a.TotalWTSLastYearAverage,
                            TotalWTSLastYearCount = a.TotalWTSLastYearCount,
                            LastWTSSeen = a.LastWTSSeen,
                            TotalWTBLast30DaysAverage = a.TotalWTBLast30DaysAverage,
                            TotalWTBLast30DaysCount = a.TotalWTBLast30DaysCount,
                            TotalWTBLast60DaysCount = a.TotalWTBLast60DaysCount,
                            TotalWTBLast60DaysAverage = a.TotalWTBLast60DaysAverage,
                            TotalWTBLast90DaysCount = a.TotalWTBLast90DaysCount,
                            TotalWTBLast90DaysAverage = a.TotalWTBLast90DaysAverage,
                            TotalWTBLast6MonthsCount = a.TotalWTBLast6MonthsCount,
                            TotalWTBLast6MonthsAverage = a.TotalWTBLast6MonthsAverage,
                            TotalWTBAuctionCount = a.TotalWTBAuctionCount,
                            TotalWTBAuctionAverage = a.TotalWTBAuctionAverage,
                            TotalWTBLastYearAverage = a.TotalWTBLastYearAverage,
                            TotalWTBLastYearCount = a.TotalWTBLastYearCount,
                            LastWTBSeen = a.LastWTBSeen
                        }).FirstOrDefault();
            return item;
        }

        /// <summary>
        /// Use this API when you want Full history on item. This might transfer ALOT of data, so only use it if you need it!
        /// </summary>
        /// <param name="server"></param>
        /// <param name="itemname"></param>
        /// <returns></returns>
        [Route("api/item/getdetails/{server}/{itemname}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        public ItemDetail GetItemDetail([DefaultValue(Servers.Green)] Servers server, [DefaultValue("10 Dose Greater Potion of Purity")] string itemname)
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
        public ItemDetail GetItemDetail([DefaultValue(1981)] int itemid)
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
