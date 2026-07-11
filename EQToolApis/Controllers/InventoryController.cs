using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Services;
using EQToolShared.APIModels.InventoryControllerModels;
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
        [HttpGet("profile")]
        public async Task<ContentResult> Profile(string character)
        {
            var assetBase = $"{Request.Scheme}://{Request.Host}";
            ContentResult Html(CharacterProfile? profile, string message = "") =>
                Content(CharacterProfileHtml.RenderDocument(profile, message, assetBase), "text/html");

            var discordId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(discordId))
            {
                return Html(null, "Not logged in. Link Discord from the General tab.");
            }

            var discordUser = await _context.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
            if (discordUser == null)
            {
                return Html(null, "Not logged in. Link Discord from the General tab.");
            }

            var inventory = await _context.CharacterInventories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.DiscordUserId == discordUser.DiscordUserId && c.CharacterName == character);

            if (inventory == null)
            {
                return Html(null, $"No inventory uploaded for {character}. Run /outputfile inventory in game while Pigparse is running.");
            }

            return Html(_profileService.BuildProfile(inventory));
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
                .FirstOrDefaultAsync(c => c.DiscordUserId == discordUser.DiscordUserId && c.CharacterName == request.CharacterName);

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
