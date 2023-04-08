using EQToolApis.DB;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EQToolApis.Controllers
{
    public class ExceptionRequest
    {
        [MaxLength(24)]
        public string Version { get; set; }
        public string Exception { get; set; }
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
            _ = dbcontext.EqToolExceptions.Add(new DB.Models.EqToolException
            {
                Exception = model.Exception,
                Version = model.Version,
                DateCreated = DateTime.UtcNow
            });

            _ = dbcontext.SaveChanges();
        }
    }
}