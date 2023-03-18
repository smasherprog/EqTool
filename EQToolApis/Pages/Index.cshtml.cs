using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EQToolApis.Pages
{
    public class IndexModel : PageModel
    {
        private readonly EQToolContext context;
        public readonly string Login;

        public IndexModel(EQToolContext context, IOptions<DiscordServiceOptions> options)
        {
            this.context = context;
            Login = options.Value.login;
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
                       .GroupBy(a => a.EQitemId)
                       .Select(a => new AuctionItem
                       {
                           ItemName = a.First().EQitem.ItemName,
                           AveragePrice = (int)a.Average(b => b.AuctionPrice ?? 0),
                           Count = a.Count(),
                           LastSeen = a.Select(b => b.EQTunnelMessage.TunnelTimestamp).OrderByDescending(b => b).FirstOrDefault()
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
