using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.Controllers
{
    public class PlayerRequest
    {
        [Required]
        public List<string>? Players { get; set; }
        [EnumDataType(typeof(Servers))]
        public Servers Server { get; set; }
    }

    public class PlayerUpdateRequest
    {
        [Required]
        public List<Player>? Players { get; set; }
        [EnumDataType(typeof(Servers))]
        public Servers Server { get; set; }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/player")]
    public class PlayerController : ControllerBase
    {
        private readonly EQToolContext dbcontext;
        public PlayerController(EQToolContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        [Route("getall")]
        public List<Player> Get()
        {
            return dbcontext.Players.ToList();
        }

        [Route("getbyname")]
        public List<Player> GetGetByName([FromQuery] PlayerRequest playerRequest)
        {
            return dbcontext.Players.Where(a => playerRequest.Players.Contains(a.Name) && a.Server == playerRequest.Server).ToList();
        }

        [Route("getbynames"), HttpPost]
        public List<Player> GetByNames([FromBody] PlayerUpdateRequest playerRequest)
        {
            var names = playerRequest.Players.Select(a => a.Name).Distinct().ToList();
            return dbcontext.Players.Where(a => names.Contains(a.Name) && a.Server == playerRequest.Server).ToList();
        }

        [Route("upsertplayers"), HttpPost]
        public void Update([FromBody] PlayerUpdateRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return;
            }

            var players = model.Players.Select(a => a.Name).Distinct().ToList();
            var dbplayers = dbcontext.Players.Where(a => players.Contains(a.Name) && a.Server == model.Server).ToList();
            foreach (var item in model.Players)
            {
                var apilog = new APILog
                {
                    APIAction = APIAction.PlayerAdded,
                    IpAddress = ip,
                    LogMessage = string.Empty
                };
                item.Server = model.Server;
                var p = dbplayers.FirstOrDefault(a => a.Name.ToLower() == item.Name.ToLower());
                if (p == null)
                {
                    _ = dbcontext.Players.Add(item);
                    apilog.APIAction = APIAction.PlayerAdded;
                    apilog.LogMessage = item.Name;
                    _ = dbcontext.Add(apilog);
                    p = item;
                }
                else
                {
                    apilog.APIAction = APIAction.PlayerUpdate;
                    if (item.Level > p.Level)
                    {
                        apilog.LogMessage += $"{item.Level}-{p.Level},";
                        p.Level = item.Level;
                    }

                    if (p.GuildName != item.GuildName && string.IsNullOrWhiteSpace(item.GuildName))
                    {
                        apilog.LogMessage += $"{item.GuildName}-{p.GuildName},";
                        p.GuildName = item.GuildName;
                    }

                    if (p.PlayerClass != item.PlayerClass && item.PlayerClass.HasValue)
                    {
                        apilog.LogMessage += $"{item.PlayerClass}-{p.PlayerClass},";
                        p.PlayerClass = item.PlayerClass;
                    }

                    if (!string.IsNullOrWhiteSpace(apilog.LogMessage))
                    {
                        _ = dbcontext.Add(apilog);
                    }
                }
            }

            _ = dbcontext.SaveChanges();
        }
    }
}