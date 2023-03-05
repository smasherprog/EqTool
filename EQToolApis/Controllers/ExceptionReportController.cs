using EQToolApis.DB;
using Microsoft.AspNetCore.Mvc;

namespace EQToolApis.Controllers
{
    public class ExceptionRequest
    {
        public string Exception { get; set; }
    }

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
                DateCreated = DateTime.UtcNow
            });

            _ = dbcontext.SaveChanges();
        }
    }
}