using EQTool.ViewModels;
using EQToolShared;
using System.Collections.Generic;
using System.Linq;

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
            try
            {
                var url = $"https://pigparse.azurewebsites.net/api/item/wiki/{name}?zonename={currentzone}";
                var res = App.httpclient.GetAsync(url).Result;
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = res.Content.ReadAsStringAsync().Result;
                    return response;
                }
            }
            catch
            {
            }

            return string.Empty;
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
