using EQTool.ViewModels;
using EQToolShared.Map;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public class DisambigData
        {
            public string Name { get; set; }
            public string ZonePart { get; set; }
        }

        public static string Disambig(string response, string zone)
        {
            var splits = response.Split('\n').Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)).Where(a => a.Contains("Disambig")).ToList();
            var data = new List<DisambigData>();
            foreach (var item in splits)
            {
                var r = new DisambigData();
                var indexof = item.IndexOf("[[");
                if (indexof == -1)
                {
                    continue;
                }
                var name = item.Substring(indexof + 2);
                indexof = name.IndexOf("]]");
                if (indexof == -1)
                {
                    continue;
                }
                name = name.Substring(0, indexof);
                r.Name = name?.Trim();
                r.ZonePart = r.Name;
                data.Add(r);
                indexof = name.IndexOf("(");
                if (indexof == -1)
                {
                    continue;
                }
                var zonefromdata = name.Substring(indexof + 1);
                indexof = zonefromdata.IndexOf(")");
                if (indexof == -1)
                {
                    continue;
                }
                name = zonefromdata.Substring(0, indexof);
                r.ZonePart = name;
            }
            zone = zone?.ToLower() ?? string.Empty;
            var zonnamemapper = EQToolShared.Map.ZoneParser.ZoneNameMapper.FirstOrDefault(a => a.Value == zone);

            foreach (var item in data)
            {
                if (item.ZonePart.Contains(zone))
                {
                    return item.Name;
                }
                else if (!string.IsNullOrWhiteSpace(zonnamemapper.Key) && item.ZonePart.ToLower().Contains(zonnamemapper.Key))
                {
                    return item.Name;
                }
            }

            data = data.Where(a => !ZoneParser.Zones.Any(b => a.ZonePart.ToLower().Contains(b))).ToList();
            data = data.Where(a => !ZoneParser.ZoneWhoMapper.Keys.Any(b => a.ZonePart.ToLower().Contains(b.ToLower()))).ToList();
            data = data.Where(a => !ZoneParser.ZoneNameMapper.Keys.Any(b => a.ZonePart.ToLower().Contains(b.ToLower()))).ToList();
            return data.FirstOrDefault()?.Name;
        }

        public string GetData(string name)
        {
            var currentzone = activePlayer?.Player?.Zone;
            if (name == "Snitch" && currentzone == "mischiefplane")
            {
                name = "Snitch_(PoM)";
            }
            else if (name == "Cazic Thule")
            {
                name = "Cazic_Thule_(God)";
            }
            else if (name == "Innoruuk")
            {
                name = "Innoruuk_(God)";
            }
            else if (name == "Bristlebane")
            {
                name = "Bristlebane_(God)";
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
                    else if (response.StartsWith("Disambig"))
                    {
                        name = Disambig(response, currentzone);
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            name = HttpUtility.UrlEncode(name.Replace(' ', '_'));
                            url = $"https://wiki.project1999.com/{name}?action=raw";
                            res = App.httpclient.GetAsync(url).Result;
                            if (res.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                return res.Content.ReadAsStringAsync().Result;
                            }
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
            catch (Exception e)
            {
                var msg = $"Zone: {name} ";
                throw new System.Exception(msg + e.Message);
            }

            return string.Empty;
        }
    }
}
