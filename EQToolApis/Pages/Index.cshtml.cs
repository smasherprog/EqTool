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
                            ItemCache = context.EQTunnelAuctionItems
                              .Where(a => a.EQTunnelMessage.AuctionType != AuctionType.BOTH)
                              .GroupBy(a => new AuctionItem
                              {
                                  AuctionType = a.EQTunnelMessage.AuctionType,
                                  ItemName = a.EQitem.ItemName,
                                  TotalLast30DaysAverage = a.EQitem.TotalLast30DaysAverage,
                                  TotalLast30DaysCount = a.EQitem.TotalLast30DaysCount,
                                  TotalLast60DaysCount = a.EQitem.TotalLast60DaysCount,
                                  TotalLast60DaysAverage = a.EQitem.TotalLast60DaysAverage,
                                  TotalLast90DaysCount = a.EQitem.TotalLast90DaysCount,
                                  TotalLast90DaysAverage = a.EQitem.TotalLast90DaysAverage,
                                  TotalLast6MonthsCount = a.EQitem.TotalLast6MonthsCount,
                                  TotalLast6MonthsAverage = a.EQitem.TotalLast6MonthsAverage,
                                  TotalAuctionCount = a.EQitem.TotalAuctionCount,
                                  TotalAuctionAverage = a.EQitem.TotalAuctionAverage,
                                  LastSeen = a.EQitem.LastSeen
                              })
                              .Select(a => a.Key).ToList()
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

        public DateTimeOffset LastSeen => context.EQTunnelMessages.OrderByDescending(a => a.TunnelTimestamp).Select(a => a.TunnelTimestamp).FirstOrDefault();

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
