using EQTool.ViewModels;
using System.Net;
using System.Net.Http;
using System.Web;

namespace EQTool.Services
{
    public class WikiApi
    {
        private readonly ActivePlayer activePlayer;
        public WikiApi(ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
        }

        public string GetData(string name)
        {
            var currentzone = activePlayer?.Player?.Zone;
            if (name == "Snitch" && currentzone == "mischiefplane")
            {
                name = "Snitch_(PoM)";
            }

            try
            {
                name = HttpUtility.UrlEncode(name.Trim().Replace(' ', '_'));
                var url = $"https://wiki.project1999.com/{name}?action=raw";
                var res = App.httpclient.GetAsync(url).Result;
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = res.Content.ReadAsStringAsync().Result;
                    if (response.StartsWith("#REDIRECT"))
                    {
                        name = response.Replace("#REDIRECT", string.Empty)?.Replace("[[:", string.Empty)?.Replace("[[", string.Empty)?.Replace("]]", string.Empty)?.Trim();
                        name = HttpUtility.UrlEncode(name.Replace(' ', '_'));
                        url = $"https://wiki.project1999.com/{name}?action=raw";
                        res = App.httpclient.GetAsync(url).Result;
                        if (res.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            return res.Content.ReadAsStringAsync().Result;
                        }
                    }
                    else
                    {
                        return response;
                    }
                }
            }
            catch (System.AggregateException er)
            {
                if (er.InnerException != null && er.InnerException.GetType() == typeof(HttpRequestException))
                {
                    var err = er.InnerException as HttpRequestException;
                    if (err.InnerException?.GetType() == typeof(WebException))
                    {
                        var innererr = err.InnerException as WebException;
                        throw new System.Exception(innererr.Message);
                    }
                    else
                    {
                        throw new System.Exception(err.Message);
                    }
                }
            }

            return string.Empty;
        }
    }
}
