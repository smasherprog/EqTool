using EQToolApis.DB;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EQToolApis.Services
{
    public class ApiTokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ApiTokenAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IServiceScopeFactory scopeFactory)
            : base(options, logger, encoder)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return AuthenticateResult.NoResult();
            }

            var header = authHeader.ToString();
            if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.NoResult();
            }

            var token = header["Bearer ".Length..].Trim();
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Empty token.");
            }

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EQToolContext>();
            var user = await db.DiscordUsers.FirstOrDefaultAsync(u => u.ApiToken == token);
            if (user == null)
            {
                return AuthenticateResult.Fail("Invalid token.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.DiscordId),
                new Claim(ClaimTypes.Name, user.Username)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
        }
    }
}
