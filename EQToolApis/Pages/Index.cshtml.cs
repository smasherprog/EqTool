using EQToolApis.DB;
using EQToolApis.DB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EQToolApis.Pages
{
    public class IndexModel : PageModel
    {
        private readonly EQToolContext context;

        public IndexModel(EQToolContext context)
        {
            this.context = context;
        }

        public List<EQTunnelMessage> EQTunnelMessages => context.EQTunnelMessages
            .Include(a => a.EQAuctionPlayer)
            .Include(a => a.EQTunnelAuctionItems)
            .ThenInclude(a => a.EQitem)
            .OrderByDescending(a => a.EQTunnelMessageId)
            .Take(500)
            .ToList();

        private List<AuctionItem> _AuctionItems = new();

        public List<AuctionItem> AuctionItems
        {
            get
            {
                if (!_AuctionItems.Any())
                {
                    _AuctionItems = context.EQTunnelAuctionItems
                       .Where(a => a.EQTunnelMessage.AuctionType == AuctionType.WTS)
                       .GroupBy(a => new AuctionItem
                       {
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
                       .ToList();
                }

                return _AuctionItems;
            }
        }
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
