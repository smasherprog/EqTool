using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Mappers;
using EQToolShared.APIModels.PlayerControllerModels;
using Microsoft.AspNetCore.Mvc;

namespace EQToolApis.Controllers
{
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
        public List<EQToolShared.APIModels.PlayerControllerModels.Player> Get()
        {
            return dbcontext.Players.ToList().Map();
        }

        [Route("getbyname")]
        public List<EQToolShared.APIModels.PlayerControllerModels.Player> GetGetByName([FromQuery] PlayerRequest playerRequest)
        {
            return dbcontext.Players.Where(a => playerRequest.Players.Contains(a.Name) && a.Server == playerRequest.Server).ToList().Map();
        }

        [Route("getbynames"), HttpPost]
        public List<EQToolShared.APIModels.PlayerControllerModels.Player> GetByNames([FromBody] PlayerUpdateRequest playerRequest)
        {
            var names = playerRequest.Players.Select(a => a.Name).Distinct().ToList();
            return dbcontext.Players.Where(a => names.Contains(a.Name) && a.Server == playerRequest.Server).ToList().Map();
        }

        [Route("upsertplayers"), HttpPost]
        public void Update([FromBody] PlayerUpdateRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return;
            }

            var players = model.Players?.Select(a => a.Name).Distinct().ToList() ?? new List<string>();
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
                    p = new DB.Models.Player
                    {
                        GuildName = item.GuildName,
                        Level = (byte?)item.Level,
                        Name = item.Name,
                        PlayerClass = item.PlayerClass,
                        Server = model.Server
                    };
                    _ = dbcontext.Players.Add(p);
                    apilog.APIAction = APIAction.PlayerAdded;
                    apilog.LogMessage = item.Name;
                    _ = dbcontext.Add(apilog);
                }
                else
                {
                    apilog.APIAction = APIAction.PlayerUpdate;
                    if (item.Level > p.Level)
                    {
                        apilog.LogMessage += $"{item.Level}-{p.Level},";
                        p.Level = (byte?)item.Level;
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