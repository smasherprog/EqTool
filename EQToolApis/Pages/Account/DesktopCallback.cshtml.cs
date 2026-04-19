using EQToolApis.DB;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EQToolApis.Pages.Account
{
    public class DesktopCallbackModel : PageModel
    {
        private readonly EQToolContext _db;

        public DesktopCallbackModel(EQToolContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync(int port)
        {
            if (port < 1024 || port > 65535)
            {
                return BadRequest("Invalid port.");
            }

            var result = await HttpContext.AuthenticateAsync("DiscordCookie");
            if (!result.Succeeded || result.Principal == null)
            {
                return Redirect("/");
            }

            var username = result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var discordId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            var user = await _db.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);
            if (user != null)
            {
                user.ApiToken = token;
                await _db.SaveChangesAsync();
            }

            var callbackUrl = $"http://127.0.0.1:{port}/?username={Uri.EscapeDataString(username)}&discord_id={Uri.EscapeDataString(discordId)}&api_token={Uri.EscapeDataString(token)}";
            return Redirect(callbackUrl);
        }
    }
}
