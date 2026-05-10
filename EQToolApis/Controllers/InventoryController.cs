using EQToolApis.DB;
using EQToolApis.DB.Models;
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

        public InventoryController(EQToolContext context)
        {
            _context = context;
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
