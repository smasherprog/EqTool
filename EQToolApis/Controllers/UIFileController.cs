using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolShared.APIModels.UIFileControllerModels;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EQToolApis.Controllers
{
    // Backup/sync store for a Discord user's EverQuest UI config files. Mirrors
    // InventoryController: the caller is identified from the ApiToken-derived
    // NameIdentifier claim (the Discord id), never from the request body, so a
    // user can only ever read/write/delete their own files. 401 = not logged in
    // with Discord.
    [ApiController]
    [Route("api/uifile")]
    [Authorize(AuthenticationSchemes = "ApiToken")]
    public class UIFileController : ControllerBase
    {
        private readonly EQToolContext _context;

        public UIFileController(EQToolContext context)
        {
            _context = context;
        }

        private async Task<DiscordUser?> GetCallerAsync()
        {
            var discordId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(discordId))
            {
                return null;
            }
            return await _context.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] UIFileUploadRequest request)
        {
            var user = await GetCallerAsync();
            if (user == null)
            {
                return Unauthorized();
            }
            if (request == null || string.IsNullOrWhiteSpace(request.FileName) || request.Contents == null)
            {
                return BadRequest();
            }

            var existing = await _context.UIFiles
                .FirstOrDefaultAsync(f => f.DiscordUserId == user.DiscordUserId && f.FileName == request.FileName);

            if (existing == null)
            {
                existing = new UIFile
                {
                    DiscordUserId = user.DiscordUserId,
                    FileName = request.FileName
                };
                _context.UIFiles.Add(existing);
            }

            existing.PlayerName = request.PlayerName;
            existing.Server = request.Server;
            existing.Contents = request.Contents;
            existing.LastModifiedUtc = request.LastModifiedUtc;
            existing.UploadedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { existing.FileName, existing.LastModifiedUtc });
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var user = await GetCallerAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var files = await _context.UIFiles
                .Where(f => f.DiscordUserId == user.DiscordUserId)
                .Select(f => new UIFileMetadata
                {
                    FileName = f.FileName,
                    PlayerName = f.PlayerName,
                    Server = f.Server,
                    LastModifiedUtc = f.LastModifiedUtc
                })
                .ToListAsync();

            return new JsonResult(files);
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download(string fileName)
        {
            var user = await GetCallerAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var row = await _context.UIFiles
                .FirstOrDefaultAsync(f => f.DiscordUserId == user.DiscordUserId && f.FileName == fileName);
            if (row == null)
            {
                return NotFound(new { error = "no_uifile" });
            }

            return new JsonResult(new UIFileDownloadResponse
            {
                FileName = row.FileName,
                LastModifiedUtc = row.LastModifiedUtc,
                Contents = row.Contents
            });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string fileName)
        {
            var user = await GetCallerAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var row = await _context.UIFiles
                .FirstOrDefaultAsync(f => f.DiscordUserId == user.DiscordUserId && f.FileName == fileName);
            if (row == null)
            {
                return NotFound(new { error = "no_uifile" });
            }

            _context.UIFiles.Remove(row);
            await _context.SaveChangesAsync();
            return Ok(new { deleted = fileName });
        }
    }
}
