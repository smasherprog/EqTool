using EQToolApis.DB;
using EQToolApis.Hubs;
using EQToolApis.Services;
using EQToolShared.APIModels.ZoneControllerModels;
using EQToolShared.HubModels;
using EQToolShared.Map;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace EQToolApis.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/zone")]
    public class ZoneController : ControllerBase
    {
        private readonly EQToolContext dbcontext;
        private readonly NpcTrackingService notableNpcService;
        private readonly IHubContext<MapHub> hubContext;
        private DateTime LastKaelFactionSend = DateTime.UtcNow.AddMonths(-2);

        public ZoneController(EQToolContext dbcontext, NpcTrackingService notableNpcService, IHubContext<MapHub> hubContext)
        {
            this.hubContext = hubContext;
            this.dbcontext = dbcontext;
            this.notableNpcService = notableNpcService;
        }

        [Route("npcactivity"), HttpPost]
        public async Task SeenAsync([FromBody] NPCActivityRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip) || dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return;
            }
            if (ZoneParser.KaelFactionMobs.Contains(model.NPCData.Name) && model.IsDeath && (DateTime.UtcNow - LastKaelFactionSend).TotalSeconds > 10)
            {
                this.LastKaelFactionSend = DateTime.UtcNow;
                await this.hubContext.Clients.Group(model.Server.ToString()).SendAsync("AddCustomTrigger", new SignalrCustomTimer
                {
                    Server = model.Server,
                    DurationInSeconds = 28 * 60,
                    Name = "Next Kael Faction Pull"
                });
            }
            notableNpcService.Add(model, ip);
        }

        [Route("quake"), HttpPost]
        public void Quake()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip) || dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return;
            }
            var d = DateTimeOffset.UtcNow.AddHours(-2);
            if (!dbcontext.QuakeTimes.Any(a => a.DateTime > d))
            {
                _ = dbcontext.QuakeTimes.Add(new DB.Models.QuakeTime { DateTime = DateTimeOffset.UtcNow });
            }
        }
    }
}