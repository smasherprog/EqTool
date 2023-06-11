using EQToolApis.DB;
using EQToolApis.DB.Models;
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
            var players = model.Players.Select(a => a.Name).Distinct().ToList();
            var dbplayers = dbcontext.Players.Where(a => players.Contains(a.Name) && a.Server == model.Server).ToList();
            foreach (var item in model.Players)
            {
                item.Server = model.Server;
                var p = dbplayers.FirstOrDefault(a => a.Name.ToLower() == item.Name.ToLower());
                if (p == null)
                {
                    _ = dbcontext.Players.Add(item);
                    p = item;
                }

                if (item.Level > p.Level)
                {
                    p.Level = item.Level;
                }

                if (p.GuildName != item.GuildName && string.IsNullOrWhiteSpace(item.GuildName))
                {
                    p.GuildName = item.GuildName;
                }

                if (p.PlayerClass != item.PlayerClass && item.PlayerClass.HasValue)
                {
                    p.PlayerClass = item.PlayerClass;
                }
            }

            _ = dbcontext.SaveChanges();
        }
    }
}