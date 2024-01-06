using EQToolApis.DB;
using EQToolApis.Models;
using EQToolApis.Services;
using EQToolShared.APIModels.ItemControllerModels;
using EQToolShared.Discord;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.Controllers
{
    public class ItemController : ControllerBase
    {
        private readonly UIDataBuild uIDataBuild;
        private readonly EQToolContext context;
        private readonly PlayerCacheV2 playerCachev2;
        private readonly DiscordAuctionParse discordAuctionParse;

        public ItemController(UIDataBuild uIDataBuild, EQToolContext context, PlayerCacheV2 playerCachev2, DiscordAuctionParse discordAuctionParse)
        {
            this.uIDataBuild = uIDataBuild;
            this.context = context;
            this.playerCachev2 = playerCachev2;
            this.discordAuctionParse = discordAuctionParse;
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
            if (UIDataBuild.ItemCacheV2[(int)server] == null)
            {
                uIDataBuild.BuildDataV2(server);
            }
            ret = UIDataBuild.ItemCacheV2[(int)server];

#else
            ret = UIDataBuild.ItemCacheV2[(int)server];
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
            var item = context.EQitemsV2
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
        /// A bulk version of api/item/get/{server}/{itemname}
        /// </summary>
        /// <param name="server"></param>
        /// <param name="itemnames">"10 Dose Greater Potion of Purity ", "A Blue Squire" </param>
        /// <returns></returns>
        [Route("api/item/getmultiple/{server}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        public List<Item> GetMultipleItem([DefaultValue(Servers.Green)] Servers server, [FromQuery] List<string> itemnames)
        {
            itemnames ??= new List<string>();
            itemnames = itemnames.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
            var items = context.EQitemsV2
                        .Where(a => a.Server == server && itemnames.Contains(a.ItemName))
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
                        }).ToList();
            return items;
        }

        /// <summary>
        /// A bulk version of api/item/get/{server}/{itemname}
        /// </summary>
        /// <param name="itemsLookups"></param>
        /// <returns></returns>
        [Route("api/item/postmultiple")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        public List<Item> PostMultipleItem([FromBody] ItemsLookups itemsLookups)
        {
            var itemnames = itemsLookups.Itemnames.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();
            var items = context.EQitemsV2
                        .Where(a => a.Server == itemsLookups.Server && itemnames.Contains(a.ItemName))
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
                        }).ToList();
            return items;
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
        public ItemDetail GetItemDetail([DefaultValue(Servers.Green)] Servers server, [DefaultValue("10 Dose Greater Potion of Purity"), Required] string itemname)
        {
            var items = context.EQTunnelAuctionItemsV2
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
                ItemName = context.EQitemsV2
                    .Where(a => a.ItemName == itemname && a.Server == server)
                    .Select(a => a.ItemName)
                    .FirstOrDefault(),
                Items = new List<ItemAuctionDetail>(),
                Players = new Dictionary<int, string>()
            };
            playerCachev2.PlayersLock.EnterReadLock();
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
                    _ = Item.Players.TryAdd(i.EQAuctionPlayerId, playerCachev2.Players[i.EQAuctionPlayerId].Name);
                }
            }
            finally
            {
                playerCachev2.PlayersLock.ExitReadLock();
            }
            Item.Items = Item.Items.OrderByDescending(a => a.t).ToList();
            return Item;
        }

        /// <summary>
        /// Will get an item and all of its details. This can include alot of data!
        /// DO NOT USE THIS. They will replace the regular version once data import is done!
        /// </summary>
        /// <param name="itemid"></param>
        /// <returns></returns>
        [Route("api/item/getdetails/{itemid}")]
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "*" })]
        public ItemDetail GetItemDetail([DefaultValue(22741)] int itemid)
        {
            var items = context.EQTunnelAuctionItemsV2
                .Where(a => a.EQitemId == itemid)
                .OrderByDescending(a => a.EQTunnelAuctionItemId)
                .Select(a => new
                {
                    a.EQTunnelMessage.AuctionType,
                    a.AuctionPrice,
                    a.EQTunnelMessage.EQAuctionPlayerId,
                    a.EQTunnelMessage.TunnelTimestamp
                }).Take(5000).ToList();

            ItemDetail Item = new()
            {
                ItemName = context.EQitemsV2
                    .Where(a => a.EQitemId == itemid)
                    .Select(a => a.ItemName)
                    .FirstOrDefault(),
                Items = new List<ItemAuctionDetail>(),
                Players = new Dictionary<int, string>()
            };
            this.playerCachev2.PlayersLock.EnterReadLock();
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
                    _ = Item.Players.TryAdd(i.EQAuctionPlayerId, playerCachev2.Players[i.EQAuctionPlayerId].Name);
                }
            }
            finally
            {
                this.playerCachev2.PlayersLock.ExitReadLock();
            }
            Item.Items = Item.Items.OrderByDescending(a => a.t).ToList();
            return Item;
        }

        /// <summary>
        /// Will parse in game message and return auction data
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Route("api/item/auctionParse")]
        [HttpPost]
        public Auction AuctionParse([FromBody, DefaultValue("Fuxi auctions, 'WTS Silver Chitin Hand Wraps 1.3 / Nilitim's Grimoire Pg. 300 x3 / Nilitim's Grimoire Pg. 116 / Nilitim's Grimoire Pg. 115 / Nilitim's Grimoire Pg. 35 / Salil's Writ Pg. 174 L 5pp ea last call pst WTB Spell: Pillar of Lightning 50p l Sarnak-Hide Mask 50p l Arctic Wyvern Hide 300p/stack l WTS Ring of stealthy travel 14k WTS Bag of the Tinkerers 5300pp.  5250ea for qty 2+.  5200 for qty 4+.  (price firm) pst WTB Scepter of the Forlorn paying 5k WTB'")] string message)
        {
            return this.discordAuctionParse.Parse(message);
        }
    }
}
