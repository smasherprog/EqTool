using EQToolShared.Enums;
using System.Net.Http;
using System.Text;

namespace EQTool.Services
{
    public class LoggingService
    {
        private readonly HttpClient httpclient = new HttpClient();

        public void Log(string message, EventType eventType, Servers? server)
        {
            var build = BuildType.Release;
#if TEST
            return;
#elif DEBUG
            build = BuildType.Debug;
#elif BETA
            build = BuildType.Beta; 
#endif
            try
            {
                var msg = new App.ExceptionRequest
                {
                    Version = App.Version,
                    Message = message,
                    EventType = eventType,
                    BuildType = build,
                    Server = server
                };
                var msagasjson = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
                var content = new StringContent(msagasjson, Encoding.UTF8, "application/json");
                var result = httpclient.PostAsync("https://pigparse.azurewebsites.net/api/eqtool/exception", content).Result;
            }
            catch { }
        }
    }
}
