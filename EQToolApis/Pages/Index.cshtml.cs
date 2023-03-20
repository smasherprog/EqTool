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
                       .GroupBy(a => new { a.EQitem.ItemName, a.EQitem.TotalLast30DaysAverage, a.EQitem.TotalLast30DaysCount, a.EQitem.LastSeen })
                       .Select(a => new AuctionItem
                       {
                           ItemName = a.Key.ItemName,
                           AveragePrice = a.Key.TotalLast30DaysAverage,
                           Count = a.Key.TotalLast30DaysCount,
                           LastSeen = a.Key.LastSeen
                       }).ToList()
                       .OrderBy(a => a.ItemName)
                       .ToList();
                }

                return _AuctionItems;
            }
        }

        public List<AuctionItem> AuctionItemsCol1 => AuctionItems.Take(AuctionItems.Count / 2).ToList();

        public List<AuctionItem> AuctionItemsCol2 => AuctionItems.Skip(AuctionItems.Count / 2).ToList();


        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
