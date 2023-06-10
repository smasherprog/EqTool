using EQTool.Models;
using EQTool.Services.Spells.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace EQTool.Services
{
    public class PigParseApi
    {
        public class ItemsLookups
        {
            public Servers Server { get; set; }

            public List<string> Itemnames { get; set; }
        }
        public class Item
        {
            public int EQitemId { get; set; }
            public string ItemName { get; set; }

            public DateTimeOffset? LastWTBSeen { get; set; }

            public DateTimeOffset? LastWTSSeen { get; set; }

            public int TotalWTSAuctionCount { get; set; }

            public int TotalWTSAuctionAverage { get; set; }

            public int TotalWTSLast30DaysCount { get; set; }

            public int TotalWTSLast30DaysAverage { get; set; }

            public int TotalWTSLast60DaysCount { get; set; }

            public int TotalWTSLast60DaysAverage { get; set; }

            public int TotalWTSLast90DaysCount { get; set; }

            public int TotalWTSLast90DaysAverage { get; set; }

            public int TotalWTSLast6MonthsCount { get; set; }

            public int TotalWTSLast6MonthsAverage { get; set; }

            public int TotalWTSLastYearCount { get; set; }

            public int TotalWTSLastYearAverage { get; set; }

            public int TotalWTBAuctionCount { get; set; }

            public int TotalWTBAuctionAverage { get; set; }

            public int TotalWTBLast30DaysCount { get; set; }

            public int TotalWTBLast30DaysAverage { get; set; }

            public int TotalWTBLast60DaysCount { get; set; }

            public int TotalWTBLast60DaysAverage { get; set; }

            public int TotalWTBLast90DaysCount { get; set; }

            public int TotalWTBLast90DaysAverage { get; set; }

            public int TotalWTBLast6MonthsCount { get; set; }

            public int TotalWTBLast6MonthsAverage { get; set; }

            public int TotalWTBLastYearCount { get; set; }

            public int TotalWTBLastYearAverage { get; set; }

        }

        public PigParseApi()
        {
        }

        public List<Item> GetData(List<string> names, Models.Servers server)
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

        public void SendData(List<PlayerWhoLogParse.PlayerInfo> players, Models.Servers server)
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
    }
}
