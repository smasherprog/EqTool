using System.Net.Http;
using System.Text;
using static EQTool.App;

namespace EQTool.Services
{
    public class LoggingService
    {
        public HttpClient httpclient = new HttpClient();

        public void Log(string message, EventType eventType)
        {
            var build = BuildType.Release;
#if TEST
            build =  BuildType.Test;
#elif DEBUG
            build = BuildType.Debug;
#elif BETA
            build = BuildType.Beta;
#endif
            try
            {
                var msg = new ExceptionRequest
                {
                    Version = App.Version,
                    Exception = message,
                    EventType = EventType.Error,
                    BuildType = build
                };
                var msagasjson = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
                var content = new StringContent(msagasjson, Encoding.UTF8, "application/json");
                var result = httpclient.PostAsync("https://pigparse.azurewebsites.net/api/eqtool/exception", content).Result;
            }
            catch { }
        }
    }
}
