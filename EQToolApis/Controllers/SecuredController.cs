using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EQToolApis.Controllers
{
    [ApiController]
    [Route("api/secured")]
    [Authorize(AuthenticationSchemes = "ApiToken")]
    public class SecuredController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
            return Ok(new
            {
                message = "Authenticated!",
                username,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
