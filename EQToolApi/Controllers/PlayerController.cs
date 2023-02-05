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
        public Player Get([FromUri] PlayerRequest playerRequest)
        {
            return dbcontext.Players.FirstOrDefault(a => a.Name == playerRequest.Name && a.Server == playerRequest.Server);
        }

        public void Post([FromBody] PlayerCreate model)
        {

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            dbcontext.Dispose();
        }
    }
}