using EQToolApis.DB;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace EQToolApis.Pages
{
    public class AllItemRow
    {
        public string Name { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public string Character { get; set; } = string.Empty;
        public Servers Server { get; set; }
        public string Location { get; set; } = string.Empty;
        public int Count { get; set; }
        // Pre-lowered "name id character server location" blob the client filters against.
        public string Search { get; set; } = string.Empty;
    }

    public class AllItemsModel : PageModel
    {
        private readonly EQToolContext _db;

        public string? DiscordUsername { get; private set; }
        public List<AllItemRow> Items { get; private set; } = [];

        public AllItemsModel(EQToolContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await HttpContext.AuthenticateAsync("DiscordCookie");
            if (!result.Succeeded || result.Principal == null)
            {
                return Redirect("/Account/Login?returnUrl=/AllItems");
            }

            DiscordUsername = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var discordId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(discordId))
            {
                return Redirect("/Account/Login?returnUrl=/AllItems");
            }

            var user = await _db.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
            if (user == null)
            {
                return Page();
            }

            var rows = await _db.CharacterInventoryItems
                .Where(i => i.Inventory.DiscordUserId == user.DiscordUserId)
                .Select(i => new
                {
                    i.Name,
                    i.ItemId,
                    Character = i.Inventory.CharacterName,
                    i.Inventory.Server,
                    i.Location,
                    i.Count,
                })
                .ToListAsync();

            Items = rows
                .Select(r =>
                {
                    var location = FriendlyLocation(r.Location);
                    return new AllItemRow
                    {
                        Name = r.Name,
                        ItemId = r.ItemId,
                        Character = r.Character,
                        Server = r.Server,
                        Location = location,
                        Count = r.Count,
                        Search = $"{r.Name} {(r.ItemId > 0 ? r.ItemId.ToString() : string.Empty)} {r.Character} {r.Server} {location}".ToLowerInvariant(),
                    };
                })
                .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Character, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Server)
                .ToList();

            return Page();
        }

        private static readonly Regex BankBagSlotRegex = new(@"^Bank(\d+)Slot(\d+)$", RegexOptions.Compiled);
        private static readonly Regex BankBagRegex = new(@"^Bank(\d+)$", RegexOptions.Compiled);
        private static readonly Regex GeneralBagSlotRegex = new(@"^General(\d+)Slot(\d+)$", RegexOptions.Compiled);
        private static readonly Regex GeneralBagRegex = new(@"^General(\d+)$", RegexOptions.Compiled);
        private static readonly Regex SharedBankRegex = new(@"^SharedBank(\d+)$", RegexOptions.Compiled);

        // Turns the raw InventoryLocation enum name into something readable, e.g.
        // General2Slot5 -> "Bag 2, Slot 5", Bank3 -> "Bank Slot 3", Head -> "Worn - Head".
        private static string FriendlyLocation(InventoryLocation loc)
        {
            if (loc == InventoryLocation.Unknown)
            {
                return "Unknown";
            }

            var name = loc.ToString();

            var m = BankBagSlotRegex.Match(name);
            if (m.Success)
            {
                return $"Bank Bag {m.Groups[1].Value}, Slot {m.Groups[2].Value}";
            }

            m = BankBagRegex.Match(name);
            if (m.Success)
            {
                return $"Bank Slot {m.Groups[1].Value}";
            }

            m = GeneralBagSlotRegex.Match(name);
            if (m.Success)
            {
                return $"Bag {m.Groups[1].Value}, Slot {m.Groups[2].Value}";
            }

            m = GeneralBagRegex.Match(name);
            if (m.Success)
            {
                return $"Inventory Slot {m.Groups[1].Value}";
            }

            m = SharedBankRegex.Match(name);
            if (m.Success)
            {
                return $"Shared Bank {m.Groups[1].Value}";
            }

            // Everything left is an equipped/worn slot (Head, Primary, Fingers, ...).
            return $"Worn - {name}";
        }
    }
}
