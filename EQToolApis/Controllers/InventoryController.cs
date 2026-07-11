using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Services;
using EQToolShared.APIModels.InventoryControllerModels;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EQToolApis.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize(AuthenticationSchemes = "ApiToken")]
    public class InventoryController : ControllerBase
    {
        private readonly EQToolContext _context;
        private readonly CharacterProfileService _profileService;

        public InventoryController(EQToolContext context, CharacterProfileService profileService)
        {
            _context = context;
            _profileService = profileService;
        }

        // Standalone HTML profile page for the desktop app's embedded browser panel.
        // Status codes matter to the desktop client: 401 = log in with Discord,
        // 404 = no inventory uploaded for this character yet.
        [HttpGet("profile")]
        public async Task<ContentResult> Profile(string character, Servers server = Servers.Green)
        {
            var assetBase = $"{Request.Scheme}://{Request.Host}";
            ContentResult Html(CharacterProfile? profile, string message = "", int statusCode = StatusCodes.Status200OK)
            {
                var result = Content(CharacterProfileHtml.RenderDocument(profile, message, assetBase), "text/html");
                result.StatusCode = statusCode;
                return result;
            }

            var discordId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(discordId))
            {
                return Html(null, "Not logged in. Log in with Discord to view character profiles.", StatusCodes.Status401Unauthorized);
            }

            var discordUser = await _context.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
            if (discordUser == null)
            {
                return Html(null, "Not logged in. Log in with Discord to view character profiles.", StatusCodes.Status401Unauthorized);
            }

            var inventory = await _context.CharacterInventories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.DiscordUserId == discordUser.DiscordUserId && c.CharacterName == character && c.Server == server);

            if (inventory == null)
            {
                return Html(null, $"No inventory uploaded for {character} on {server}. Run /outputfile inventory in game while Pigparse is running.", StatusCodes.Status404NotFound);
            }

            return Html(_profileService.BuildProfile(inventory));
        }

        // Profile as JSON for the desktop app, which renders the view natively
        // (its settings window is a WPF layered window, where an embedded browser
        // control cannot render). 401 = log in with Discord, 404 = no data yet.
        [HttpGet("profile-data")]
        public async Task<IActionResult> ProfileData(string character, Servers server = Servers.Green)
        {
            var discordId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(discordId))
            {
                return Unauthorized();
            }

            var discordUser = await _context.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
            if (discordUser == null)
            {
                return Unauthorized();
            }

            var name = (character ?? string.Empty).Trim();
            var inventory = await _context.CharacterInventories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.DiscordUserId == discordUser.DiscordUserId && c.CharacterName == name && c.Server == server);

            if (inventory == null)
            {
                // Body marker lets the desktop tell "no data for this character"
                // apart from a routing 404 (endpoint missing on an older deploy).
                return NotFound(new { error = "no_inventory" });
            }

            return new JsonResult(_profileService.BuildProfile(inventory));
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] InventoryUploadRequest request)
        {
            var discordId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(discordId))
            {
                return Unauthorized();
            }

            var discordUser = await _context.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
            if (discordUser == null)
            {
                return Unauthorized();
            }

            var existing = await _context.CharacterInventories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.DiscordUserId == discordUser.DiscordUserId && c.CharacterName == request.CharacterName && c.Server == request.Server);

            if (existing != null)
            {
                _context.CharacterInventoryItems.RemoveRange(existing.Items);
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                existing = new CharacterInventory
                {
                    DiscordUserId = discordUser.DiscordUserId,
                    CharacterName = request.CharacterName,
                    Server = request.Server,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CharacterInventories.Add(existing);
                await _context.SaveChangesAsync();
            }

            var items = request.Items.Select(i => new CharacterInventoryItem
            {
                CharacterInventoryId = existing.CharacterInventoryId,
                Location = i.Location,
                Name = i.Name,
                ItemId = i.ItemId,
                Count = i.Count,
                Slots = i.Slots
            }).ToList();

            _context.CharacterInventoryItems.AddRange(items);
            await _context.SaveChangesAsync();

            return Ok(new { characterName = existing.CharacterName, itemCount = items.Count });
        }
    }
}
