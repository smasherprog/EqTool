using EQToolApi.DB;
using EQToolApi.DB.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace EQToolApi.Controllers
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

    public class PlayerController : ApiController
    {
        private readonly EQToolContext dbcontext = new EQToolContext();

        [Route("api/player/getall")]
        public List<Player> Get()
        {
            return dbcontext.Players.ToList();
        }

        [Route("api/player/getbyname")]
        public Player GetGetByName([FromUri] PlayerRequest playerRequest)
        {
            return dbcontext.Players.FirstOrDefault(a => a.Name == playerRequest.Name && a.Server == playerRequest.Server);
        }

        [Route("api/player/getbynames"), HttpPost]
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

        [Route("api/player/upsertplayer"), HttpPost]
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            dbcontext.Dispose();
        }
    }
}