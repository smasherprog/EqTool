using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolShared;
using EQToolShared.APIModels.BoatControllerModels;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EQToolApis.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/boat")]
    public class BoatController : ControllerBase
    {
        private readonly EQToolContext dbcontext;

        public BoatController(EQToolContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        [Route("seen"), HttpPost]
        public void SeenAsync([FromBody] BoatActivityRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip) || dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return;
            }
            var zones = Zones.Boats.Select(a => a.ZoneStart).ToList();
            if (!zones.Contains(model.Zone))
            {
                return;
            }
            var mintime = DateTimeOffset.Now.AddMinutes(-1);
            if (!this.dbcontext.EQBoatActivites.Any(a => a.Zone == model.Zone && a.Boat == model.Boat && a.Server == model.Server && a.LastSeen > mintime))
            {
                this.dbcontext.Add(new EQBoatActivity
                {
                    Zone = model.Zone,
                    LastSeen = DateTimeOffset.Now,
                    Server = model.Server,
                    Boat = model.Boat
                });
                this.dbcontext.SaveChanges();
            }
        }

        [Route("serverActivity/{server}"), HttpGet]
        public List<BoatActivityResponce> serverActivity(Servers server)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip) || dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return new List<BoatActivityResponce>();
            }

            var dbzones = this.dbcontext.EQBoatActivites.Where(a => a.Server == server)
                .GroupBy(a => a.Boat)
                .Select(a => new
                {
                    Boat = a.Key,
                    Data = a.OrderByDescending(b => b.EQBoatActivityId).Select(b => new { b.LastSeen, b.Zone }).FirstOrDefault()
                }).ToList()
                .Select(a => new BoatActivityResponce
                {
                    Boat = a.Boat,
                    Zone = a.Data.Zone,
                    LastSeen = a.Data.LastSeen
                }).ToList();
            return dbzones;
        }
    }
}