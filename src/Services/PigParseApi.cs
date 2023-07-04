using EQToolShared.APIModels.ItemControllerModels;
using EQToolShared.APIModels.PlayerControllerModels;
using EQToolShared.APIModels.ZoneControllerModels;
using EQToolShared.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace EQTool.Services
{
    public class PigParseApi
    {
        public List<Item> GetData(List<string> names, Servers server)
        {
            try
            {
                var url = $"https://pigparse.azurewebsites.net/api/item/postmultiple";
                var json = JsonConvert.SerializeObject(new ItemsLookups
                {
                    Server = server,
                    Itemnames = names
                });
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var res = App.httpclient.PostAsync(url, data).Result;
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = res.Content.ReadAsStringAsync().Result;
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Item>>(response);
                }
            }
            catch
            {
            }

            return new List<Item>();
        }

        public void SendPlayerData(List<Player> players, Servers server)
        {
            if (!players.Any())
            {
                return;
            }
            Debug.WriteLine($"Sending {players.Count} Players");
            var url = $"https://pigparse.azurewebsites.net/api/player/upsertplayers";
            var json = JsonConvert.SerializeObject(new
            {
                Server = server,
                Players = players
            });
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var res = App.httpclient.PostAsync(url, data).Result;
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _ = res.Content.ReadAsStringAsync().Result;
            }
        }

        public void SendNPCActivity(NPCActivityRequest activity)
        {
            var url = $"https://pigparse.azurewebsites.net/api/zone/npcactivity";
            var json = JsonConvert.SerializeObject(activity);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var res = App.httpclient.PostAsync(url, data).Result;
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _ = res.Content.ReadAsStringAsync().Result;
            }
        }

        public List<Player> GetPlayerData(List<string> players, Servers server)
        {
            try
            {
                if (!players.Any())
                {
                    return new List<Player>();
                }
                Debug.WriteLine($"Sending {players.Count} Players");
                var url = $"https://pigparse.azurewebsites.net/api/player/getbynames";
                var json = JsonConvert.SerializeObject(new
                {
                    Server = server,
                    Players = players
                });
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var res = App.httpclient.PostAsync(url, data).Result;
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = res.Content.ReadAsStringAsync().Result;
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Player>>(response);
                }
            }
            catch
            {
            }

            return new List<Player>();
        }
    }
}
