using EQToolApis.DB;
using EQToolApis.DB.Models;
using Microsoft.AspNetCore.Mvc;

namespace EQToolApis.Controllers
{
    public class PlayerRequest
    {
        public string Name { get; set; }
        public Servers Server { get; set; }
    }
    public class PlayerCreate
    {
        public string Name { get; set; }
        public Servers Server { get; set; }
        public int Level { get; set; }
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
        public Player GetGetByName([FromQuery] PlayerRequest playerRequest)
        {
            return dbcontext.Players.FirstOrDefault(a => a.Name == playerRequest.Name && a.Server == playerRequest.Server);
        }

        [Route("getbynames"), HttpPost]
        public List<Player> GetByNames([FromBody] List<PlayerRequest> models)
        {
            var ret = new List<Player>();
            foreach (var item in models.GroupBy(a => a.Server))
            {
                var names = item.Select(a => a.Name).ToList();
                ret.AddRange(dbcontext.Players.Where(a => names.Contains(a.Name) && a.Server == item.Key).ToList());
            }

            return ret;
        }

        [Route("upsertplayer"), HttpPost]
        public void Update([FromBody] Player model)
        {
            var p = dbcontext.Players.FirstOrDefault(a => a.Name == model.Name && a.Server == model.Server);
            if (p == null)
            {
                _ = dbcontext.Players.Add(model);
            }
            else if (model.Level > p.Level)
            {
                p.Level = model.Level;
            }
            _ = dbcontext.SaveChanges();
        }
    }
}