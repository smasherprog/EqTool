using EQToolApis.DB;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.Controllers
{
    public class ExceptionRequest
    {
        [MaxLength(24), Required]
        public string? Version { get; set; }
        public string? Message { get; set; }
        public EventType? EventType { get; set; }
        public BuildType? BuildType { get; set; }
        public Servers? Server { get; set; }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public class ExceptionReportController : ControllerBase
    {
        private readonly EQToolContext dbcontext;
        private readonly List<string> allowedprefix = ["P99", "Linux", "Beta"];
        public ExceptionReportController(EQToolContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        [Route("api/eqtool/exception"), HttpPost]
        public void Update([FromBody] ExceptionRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (allowedprefix.Any(model.Version.Contains) || model.Server == Servers.Quarm)
            {
                _ = dbcontext.EqToolExceptions.Add(new DB.Models.EqToolException
                {
                    Exception = model.Message,
                    Version = model.Version,
                    DateCreated = DateTime.UtcNow,
                    EventType = model.EventType,
                    IpAddress = ip,
                    BuildType = model.BuildType,
                    Server = model.Server
                });
            }

            _ = dbcontext.SaveChanges();
        }
    }
}