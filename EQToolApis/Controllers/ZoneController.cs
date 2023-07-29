using EQToolApis.DB;
using EQToolApis.Services;
using EQToolShared.APIModels.ZoneControllerModels;
using Microsoft.AspNetCore.Mvc;

namespace EQToolApis.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/zone")]
    public class ZoneController : ControllerBase
    {
        private readonly EQToolContext dbcontext;
        private readonly NpcTrackingService notableNpcService;

        public ZoneController(EQToolContext dbcontext, NpcTrackingService notableNpcService)
        {
            this.dbcontext = dbcontext;
            this.notableNpcService = notableNpcService;
        }

        [Route("death"), HttpPost]
        public void Death([FromBody] DeathDataRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip) || dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return;
            }
            notableNpcService.Add(new NPCActivityRequest
            {
                IsDeath = true,
                Server = model.Server,
                NPCData = model.Death
            }, ip);
        }

        [Route("npcactivity"), HttpPost]
        public void Seen([FromBody] NPCActivityRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip) || dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return;
            }
            notableNpcService.Add(model, ip);
        }
    }
}