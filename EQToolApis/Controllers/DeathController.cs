using EQToolApis.DB;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.Controllers
{
    public class DeathData
    {
        public string Name { get; set; }
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
        public void Update([FromBody] DeathDataRequest model)
        {

            _ = dbcontext.SaveChanges();
        }
    }
}