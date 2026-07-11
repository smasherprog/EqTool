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
        public Servers Server { get; set; }
        public int ItemCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CharactersModel : PageModel
    {
        private readonly EQToolContext _db;
        private readonly CharacterProfileService _profileService;

        public string? DiscordUsername { get; private set; }
        public List<CharacterSummary> Characters { get; private set; } = [];

        public CharactersModel(EQToolContext db, CharacterProfileService profileService)
        {
            _db = db;
            _profileService = profileService;
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
                    Server = c.Server,
                    ItemCount = c.Items.Count,
                    UpdatedAt = c.UpdatedAt
                })
                .OrderBy(c => c.CharacterName).ThenBy(c => c.Server)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetProfileHtmlAsync(string character, Servers server = Servers.Green)
        {
            var result = await HttpContext.AuthenticateAsync("DiscordCookie");
            if (!result.Succeeded || result.Principal == null)
                return Content("<div class=\"pp-message\">Not logged in.</div>", "text/html");

            var discordId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(discordId))
                return Content("<div class=\"pp-message\">Not logged in.</div>", "text/html");

            var user = await _db.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
            if (user == null)
                return Content("<div class=\"pp-message\">No characters found.</div>", "text/html");

            var inventory = await _db.CharacterInventories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.DiscordUserId == user.DiscordUserId && c.CharacterName == character && c.Server == server);

            if (inventory == null)
                return Content("<div class=\"pp-message\">No inventory found for this character.</div>", "text/html");

            var profile = _profileService.BuildProfile(inventory);
            return Content(CharacterProfileHtml.RenderFragment(profile), "text/html");
        }
    }
}
