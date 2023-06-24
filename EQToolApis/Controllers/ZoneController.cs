using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.Controllers
{
    public class DeathData
    {
        [MaxLength(48)]
        public string Name { get; set; }
        [MaxLength(48)]
        public string Zone { get; set; }
        public double? LocX { get; set; }
        public double? LocY { get; set; }
    }

    public class DeathDataRequest
    {
        [Required]
        public DeathData Death { get; set; }
        [EnumDataType(typeof(Servers))]
        public Servers Server { get; set; }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/zone")]
    public class ZoneController : ControllerBase
    {
        private readonly EQToolContext dbcontext;
        public ZoneController(EQToolContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        [Route("death"), HttpPost]
        public void Death([FromBody] DeathDataRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return;
            }
            var apilog = new APILog
            {
                IpAddress = ip,
                APIAction = APIAction.DeathActivity
            };

            var zone = dbcontext.EQZones.FirstOrDefault(a => a.Name == model.Death.Zone);
            if (zone == null)
            {
                apilog.LogMessage = model.Death.Zone;
                apilog.APIAction = APIAction.DeathActivityNoZone;
            }
            else
            {
                apilog.LogMessage = $"{model.Death.Name}-{model.Death.Zone}";
                apilog.APIAction = APIAction.DeathActivity;
                _ = dbcontext.Add(new EQDeath
                {
                    EQDeathTime = DateTime.UtcNow,
                    EQZoneId = zone.EQZoneId,
                    LocX = model.Death.LocX,
                    LocY = model.Death.LocY,
                    Name = model.Death.Name,
                    Server = model.Server
                });
            }
            _ = dbcontext.Add(apilog);
            _ = dbcontext.SaveChanges();
        }
    }
}