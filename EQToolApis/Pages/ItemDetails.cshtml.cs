using EQToolApis.DB;
using EQToolApis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    public class ItemDetailsModel : PageModel
    {
        private readonly EQToolContext context;

        public ItemDetailsModel(EQToolContext context)
        {
            this.context = context;
        }

        public ItemDetail Item = new();

        public IActionResult OnGet([FromRoute] int itemid, [FromRoute] Servers server)
        {
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
            return Page();
        }
    }
}
