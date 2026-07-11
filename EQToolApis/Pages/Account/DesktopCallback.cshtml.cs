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

            var user = await _db.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordId == discordId);

            // Reuse the existing token so logging in from another computer does not
            // invalidate tokens already saved on other desktops; only mint one the
            // first time (or after the token has been cleared server-side).
            var token = user?.ApiToken;
            if (string.IsNullOrEmpty(token))
            {
                token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
                if (user != null)
                {
                    user.ApiToken = token;
                    await _db.SaveChangesAsync();
                }
            }

            var callbackUrl = $"http://127.0.0.1:{port}/?username={Uri.EscapeDataString(username)}&discord_id={Uri.EscapeDataString(discordId)}&api_token={Uri.EscapeDataString(token)}";
            return Redirect(callbackUrl);
        }
    }
}
