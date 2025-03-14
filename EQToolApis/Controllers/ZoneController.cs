﻿using EQToolApis.DB;
using EQToolApis.Hubs;
using EQToolApis.Models;
using EQToolApis.Services;
using EQToolShared;
using EQToolShared.APIModels.ZoneControllerModels;
using EQToolShared.Enums;
using EQToolShared.HubModels;
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
        private readonly IHubContext<MapHub> mapHubContext;
        private readonly IHubContext<PPHub> ppHubContext;
        private DateTime LastKaelFactionSend = DateTime.UtcNow.AddMonths(-2);
        private static readonly QuakeCache[] QuakeCache = new QuakeCache[(int)Servers.MaxServers]
        {
            new() { Server = Servers.Green, DateTime = DateTimeOffset.MinValue },
            new() { Server = Servers.Blue, DateTime = DateTimeOffset.MinValue },
            new() { Server = Servers.Red, DateTime = DateTimeOffset.MinValue },
            new() { Server = Servers.Quarm, DateTime = DateTimeOffset.MinValue }
        };

        public ZoneController(EQToolContext dbcontext, NpcTrackingService notableNpcService, IHubContext<MapHub> mapHubContext, IHubContext<PPHub> ppHubContext)
        {
            this.mapHubContext = mapHubContext;
            this.ppHubContext = ppHubContext;
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
            if (Zones.KaelFactionMobs.Contains(model.NPCData.Name) && model.IsDeath && (DateTime.UtcNow - LastKaelFactionSend).TotalSeconds > 10)
            {
                LastKaelFactionSend = DateTime.UtcNow;
                await mapHubContext.Clients.Group(model.Server.ToString()).SendAsync("AddCustomTrigger", new SignalrCustomTimer
                {
                    Server = model.Server,
                    DurationInSeconds = 28 * 60,
                    Name = "Next Kael Faction Pull"
                });
                await ppHubContext.Clients.Group(model.Server.ToString()).SendAsync("AddCustomTrigger", new SignalrCustomTimer
                {
                    Server = model.Server,
                    DurationInSeconds = 28 * 60,
                    Name = "Next Kael Faction Pull"
                });
            }
            notableNpcService.Add(model, ip);
        }

        [Route("quakev2/{server}"), HttpGet]
        public void Quake(Servers server)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip) || dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return;
            }
            var d = DateTimeOffset.UtcNow.AddHours(-2);
            lock (QuakeCache)
            {
                if (QuakeCache[(int)server].DateTime < d)
                {
                    QuakeCache[(int)server].DateTime = DateTimeOffset.Now;
                    if (!dbcontext.QuakeTimes.Any(a => a.DateTime > d && a.Server == server))
                    {
                        _ = dbcontext.QuakeTimes.Add(new DB.Models.QuakeTime { DateTime = DateTimeOffset.UtcNow, Server = server });
                        _ = dbcontext.SaveChanges();
                    }
                }
            }
        }
    }
}