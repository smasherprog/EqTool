using EQToolApis.DB;
using EQToolApis.Services;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EQToolApis.Pages
{
    public class CharacterSummary
    {
        public string CharacterName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CharactersModel : PageModel
    {
        private readonly EQToolContext _db;
        private readonly ItemDataService _itemData;

        public string? DiscordUsername { get; private set; }
        public List<CharacterSummary> Characters { get; private set; } = [];

        public CharactersModel(EQToolContext db, ItemDataService itemData)
        {
            _db = db;
            _itemData = itemData;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await HttpContext.AuthenticateAsync("DiscordCookie");
            if (!result.Succeeded || result.Principal == null)
                return Redirect("/Account/Login?returnUrl=/Characters");

            DiscordUsername = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var discordId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(discordId))
                return Redirect("/Account/Login?returnUrl=/Characters");

            var user = await _db.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
            if (user == null)
                return Page();

            Characters = await _db.CharacterInventories
                .Where(c => c.DiscordUserId == user.DiscordUserId)
                .Select(c => new CharacterSummary
                {
                    CharacterName = c.CharacterName,
                    ItemCount = c.Items.Count,
                    UpdatedAt = c.UpdatedAt
                })
                .OrderBy(c => c.CharacterName)
                .ToListAsync();

            return Page();
        }

        private static readonly HashSet<InventoryLocation> EquippedLocations =
        [
            InventoryLocation.Charm, InventoryLocation.Ear, InventoryLocation.Head,
            InventoryLocation.Face, InventoryLocation.Neck, InventoryLocation.Shoulders,
            InventoryLocation.Arms, InventoryLocation.Back, InventoryLocation.Wrist,
            InventoryLocation.Range, InventoryLocation.Hands, InventoryLocation.Primary,
            InventoryLocation.Secondary, InventoryLocation.Fingers, InventoryLocation.Chest,
            InventoryLocation.Legs, InventoryLocation.Feet, InventoryLocation.Waist,
            InventoryLocation.Ammo, InventoryLocation.Held
        ];

        public async Task<JsonResult> OnGetEquipmentAsync(string character)
        {
            var result = await HttpContext.AuthenticateAsync("DiscordCookie");
            if (!result.Succeeded || result.Principal == null)
                return new JsonResult(new { error = "unauthorized" }) { StatusCode = 401 };

            var discordId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(discordId))
                return new JsonResult(new { error = "unauthorized" }) { StatusCode = 401 };

            var user = await _db.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
            if (user == null)
                return new JsonResult(new { error = "not found" }) { StatusCode = 404 };

            var inventory = await _db.CharacterInventories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.DiscordUserId == user.DiscordUserId && c.CharacterName == character);

            if (inventory == null)
                return new JsonResult(new { error = "not found" }) { StatusCode = 404 };

            var grouped = inventory.Items
                .Where(i => EquippedLocations.Contains(i.Location))
                .GroupBy(i => i.Location)
                .ToDictionary(g => g.Key, g => g.ToList());

            object? Slot(InventoryLocation loc, int idx = 0)
            {
                if (!grouped.TryGetValue(loc, out var items) || idx >= items.Count) return null;
                var item = items[idx];
                return new
                {
                    name = item.Name,
                    image = _itemData.GetImageUrl(item.ItemId, item.Name),
                    tooltip = _itemData.GetTooltip(item.Name)
                };
            }

            var slots = new Dictionary<string, object?>
            {
                ["Charm"]     = Slot(InventoryLocation.Charm),
                ["Ear1"]      = Slot(InventoryLocation.Ear, 0),
                ["Head"]      = Slot(InventoryLocation.Head),
                ["Face"]      = Slot(InventoryLocation.Face),
                ["Neck"]      = Slot(InventoryLocation.Neck),
                ["Shoulders"] = Slot(InventoryLocation.Shoulders),
                ["Arms"]      = Slot(InventoryLocation.Arms),
                ["Back"]      = Slot(InventoryLocation.Back),
                ["Ear2"]      = Slot(InventoryLocation.Ear, 1),
                ["Wrist1"]    = Slot(InventoryLocation.Wrist, 0),
                ["Wrist2"]    = Slot(InventoryLocation.Wrist, 1),
                ["Range"]     = Slot(InventoryLocation.Range),
                ["Hands"]     = Slot(InventoryLocation.Hands),
                ["Primary"]   = Slot(InventoryLocation.Primary),
                ["Secondary"] = Slot(InventoryLocation.Secondary),
                ["Finger1"]   = Slot(InventoryLocation.Fingers, 0),
                ["Finger2"]   = Slot(InventoryLocation.Fingers, 1),
                ["Chest"]     = Slot(InventoryLocation.Chest),
                ["Legs"]      = Slot(InventoryLocation.Legs),
                ["Feet"]      = Slot(InventoryLocation.Feet),
                ["Waist"]     = Slot(InventoryLocation.Waist),
                ["Ammo"]      = Slot(InventoryLocation.Ammo),
                ["Held"]      = Slot(InventoryLocation.Held),
            };

            return new JsonResult(new { characterName = character, slots });
        }
    }
}
