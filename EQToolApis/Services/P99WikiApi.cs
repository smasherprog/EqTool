using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolShared;
using System.Globalization;
using System.Web;

namespace EQToolApis.Services
{
    public class P99WikiApi
    {
        private static readonly HttpClient httpclient = new();
        static P99WikiApi()
        {
            httpclient.DefaultRequestHeaders.Add("User-Agent", "request");
        }
        private readonly EQToolContext toolContext;
        public P99WikiApi(EQToolContext toolContext)
        {
            this.toolContext = toolContext;
        }
        public string GetData(string name, string zone)
        {
            name = name?.Trim() ?? string.Empty;
            zone = zone?.Trim() ?? null;

            if (!string.IsNullOrWhiteSpace(zone) && !Zones.ZoneNames.Any(b => string.Equals(b, zone, StringComparison.OrdinalIgnoreCase)))
            {
                return string.Empty;
            }

            if (name == "Snitch" && zone == "mischiefplane")
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
                name = name.Replace(' ', '_');
                if (!MasterNPCList.NPCs.Contains(name.Replace('_', ' ')) && !MasterItemList.ItemsFastLoopup.Contains(name.Replace('_', ' ')))
                {
                    return string.Empty;
                }
                var data = toolContext.P99WikiByNames.FirstOrDefault(a => a.Name == name && a.ZoneName == zone);
                if (data != null)
                {
                    return data.Data;
                }
                name = HttpUtility.UrlEncode(name);
                var url = $"https://wiki.project1999.com/{name}?action=raw";
                var res = httpclient.GetAsync(url).Result;
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = res.Content.ReadAsStringAsync().Result;
                    if (response.StartsWith("#REDIRECT"))
                    {
                        name = response.Replace("#REDIRECT", string.Empty)?.Replace("[[:", string.Empty)?.Replace("[[", string.Empty)?.Replace("]]", string.Empty)?.Trim();
                        name = HttpUtility.UrlEncode(name.Replace(' ', '_'));
                        url = $"https://wiki.project1999.com/{name}?action=raw";
                        res = httpclient.GetAsync(url).Result;
                        if (res.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var stringres = res.Content.ReadAsStringAsync().Result;
                            data = toolContext.P99WikiByNames.FirstOrDefault(a => a.Name == name && a.ZoneName == zone);
                            if (data != null)
                            {
                                return data.Data;
                            }
                            _ = toolContext.Add(new P99WikiByName
                            {
                                Name = name,
                                ZoneName = zone,
                                Data = stringres
                            });
                            _ = toolContext.SaveChanges();
                            return stringres;
                        }
                    }
                    else if (response.StartsWith("Disambig"))
                    {
                        name = Disambig(response, zone);
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            name = HttpUtility.UrlEncode(name.Replace(' ', '_'));
                            url = $"https://wiki.project1999.com/{name}?action=raw";
                            res = httpclient.GetAsync(url).Result;
                            if (res.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var stringres = res.Content.ReadAsStringAsync().Result;
                                data = toolContext.P99WikiByNames.FirstOrDefault(a => a.Name == name && a.ZoneName == zone);
                                if (data != null)
                                {
                                    return data.Data;
                                }
                                _ = toolContext.Add(new P99WikiByName
                                {
                                    Name = name,
                                    ZoneName = zone,
                                    Data = stringres
                                });
                                _ = toolContext.SaveChanges();
                                return stringres;
                            }
                        }
                    }
                    else
                    {
                        var stringres = res.Content.ReadAsStringAsync().Result;
                        data = toolContext.P99WikiByNames.FirstOrDefault(a => a.Name == name && a.ZoneName == zone);
                        if (data != null)
                        {
                            return data.Data;
                        }
                        _ = toolContext.Add(new P99WikiByName
                        {
                            Name = name,
                            ZoneName = zone,
                            Data = stringres
                        });
                        _ = toolContext.SaveChanges();
                        return stringres;
                    }
                }
                else if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var textInfo = new CultureInfo("en-US", false).TextInfo;
                    name = HttpUtility.UrlEncode(textInfo.ToTitleCase(name).Replace(' ', '_'));
                    url = $"https://wiki.project1999.com/{name}?action=raw";
                    res = httpclient.GetAsync(url).Result;
                    if (res.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var stringres = res.Content.ReadAsStringAsync().Result;
                        data = toolContext.P99WikiByNames.FirstOrDefault(a => a.Name == name && a.ZoneName == zone);
                        if (data != null)
                        {
                            return data.Data;
                        }
                        _ = toolContext.Add(new P99WikiByName
                        {
                            Name = name,
                            ZoneName = zone,
                            Data = stringres
                        });
                        _ = toolContext.SaveChanges();
                        return stringres;
                    }
                }
            }
            catch (Exception e)
            {
                var msg = $"Zone: {name} ";
                return e.Message;
            }

            return string.Empty;
        }

        private class DisambigData
        {
            public string Name { get; set; }
            public string ZonePart { get; set; }
        }
        private static string Disambig(string response, string zone)
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
                var name = item[(indexof + 2)..];
                indexof = name.IndexOf("]]");
                if (indexof == -1)
                {
                    continue;
                }
                name = name[..indexof];
                r.Name = name?.Trim();
                r.ZonePart = r.Name;
                data.Add(r);
                indexof = name.IndexOf("(");
                if (indexof == -1)
                {
                    continue;
                }
                var zonefromdata = name[(indexof + 1)..];
                indexof = zonefromdata.IndexOf(")");
                if (indexof == -1)
                {
                    continue;
                }
                name = zonefromdata[..indexof];
                r.ZonePart = name;
            }
            zone = zone?.ToLower() ?? string.Empty;
            var zonnamemapper = Zones.ZoneNameMapper.FirstOrDefault(a => a.Value == zone);

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

            data = data.Where(a => !Zones.ZoneNames.Any(b => a.ZonePart.ToLower().Contains(b))).ToList();
            data = data.Where(a => !Zones.ZoneWhoMapper.Keys.Any(b => a.ZonePart.ToLower().Contains(b.ToLower()))).ToList();
            data = data.Where(a => !Zones.ZoneNameMapper.Keys.Any(b => a.ZonePart.ToLower().Contains(b.ToLower()))).ToList();
            return data.FirstOrDefault()?.Name;
        }
    }
}
