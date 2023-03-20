using EQToolApis.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    public class IndexModel : PageModel
    {
        private readonly EQToolContext context;
        private static List<AuctionItem> ItemCache = new();
        private static DateTime LastItemCache = DateTime.MinValue;
        public IndexModel(EQToolContext context)
        {
            this.context = context;
        }

        public List<AuctionItem> AuctionItems
        {
            get
            {
                var d = DateTime.Now;
                if (!ItemCache.Any() || (d - LastItemCache).TotalHours > 1)
                {
                    lock (ItemCache)
                    {
                        if (!ItemCache.Any() || (d - LastItemCache).TotalHours > 1)
                        {
                            LastItemCache = DateTime.Now;
                            var wts = context.EQTunnelAuctionItems
                              .Where(a => a.EQTunnelMessage.AuctionType == AuctionType.WTS)
                              .GroupBy(a => new AuctionItem
                              {
                                  AuctionType = a.EQTunnelMessage.AuctionType,
                                  ItemName = a.EQitem.ItemName,
                                  TotalLast30DaysAverage = a.EQitem.TotalWTSLast30DaysAverage,
                                  TotalLast30DaysCount = a.EQitem.TotalWTSLast30DaysCount,
                                  TotalLast60DaysCount = a.EQitem.TotalWTSLast60DaysCount,
                                  TotalLast60DaysAverage = a.EQitem.TotalWTSLast60DaysAverage,
                                  TotalLast90DaysCount = a.EQitem.TotalWTSLast90DaysCount,
                                  TotalLast90DaysAverage = a.EQitem.TotalWTSLast90DaysAverage,
                                  TotalLast6MonthsCount = a.EQitem.TotalWTSLast6MonthsCount,
                                  TotalLast6MonthsAverage = a.EQitem.TotalWTSLast6MonthsAverage,
                                  TotalAuctionCount = a.EQitem.TotalWTSAuctionCount,
                                  TotalAuctionAverage = a.EQitem.TotalWTSAuctionAverage,
                                  LastSeen = a.EQitem.LastWTSSeen
                              })
                              .Select(a => a.Key).ToList();

                            var wtb = context.EQTunnelAuctionItems
                              .Where(a => a.EQTunnelMessage.AuctionType == AuctionType.WTB)
                              .GroupBy(a => new AuctionItem
                              {
                                  AuctionType = a.EQTunnelMessage.AuctionType,
                                  ItemName = a.EQitem.ItemName,
                                  TotalLast30DaysAverage = a.EQitem.TotalWTBLast30DaysAverage,
                                  TotalLast30DaysCount = a.EQitem.TotalWTBLast30DaysCount,
                                  TotalLast60DaysCount = a.EQitem.TotalWTBLast60DaysCount,
                                  TotalLast60DaysAverage = a.EQitem.TotalWTBLast60DaysAverage,
                                  TotalLast90DaysCount = a.EQitem.TotalWTBLast90DaysCount,
                                  TotalLast90DaysAverage = a.EQitem.TotalWTBLast90DaysAverage,
                                  TotalLast6MonthsCount = a.EQitem.TotalWTBLast6MonthsCount,
                                  TotalLast6MonthsAverage = a.EQitem.TotalWTBLast6MonthsAverage,
                                  TotalAuctionCount = a.EQitem.TotalWTBAuctionCount,
                                  TotalAuctionAverage = a.EQitem.TotalWTBAuctionAverage,
                                  LastSeen = a.EQitem.LastWTBSeen
                              })
                              .Select(a => a.Key).ToList();

                            ItemCache = wts.Concat(wtb)
                            .OrderBy(a => a.ItemName)
                            .ThenBy(a => a.AuctionType)
                            .ToList();
                        }
                    }
                }

                return ItemCache;
            }
        }

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
