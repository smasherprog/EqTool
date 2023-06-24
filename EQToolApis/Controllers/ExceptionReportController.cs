using EQToolApis.DB;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.Controllers
{
    public class ExceptionRequest
    {
        [MaxLength(24)]
        public string? Version { get; set; }
        public string? Message { get; set; }
        public EventType? EventType { get; set; }
        public BuildType? BuildType { get; set; }

    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public class ExceptionReportController : ControllerBase
    {
        private readonly EQToolContext dbcontext;
        public ExceptionReportController(EQToolContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        [Route("api/eqtool/exception"), HttpPost]
        public void Update([FromBody] ExceptionRequest model)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            _ = dbcontext.EqToolExceptions.Add(new DB.Models.EqToolException
            {
                Exception = model.Message,
                Version = model.Version,
                DateCreated = DateTime.UtcNow,
                EventType = model.EventType,
                IpAddress = ip,
                BuildType = model.BuildType
            });

            _ = dbcontext.SaveChanges();
        }
    }
}