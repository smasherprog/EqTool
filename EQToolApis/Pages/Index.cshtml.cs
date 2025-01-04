using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Hubs;
using EQToolApis.Models;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EQToolApis.Pages
{
    public class IndexModel : PageModel
    {
        public readonly DBData AllData;
        public readonly NoteableNPCCache noteableNPCCache;
        public List<List<TODModel>> NoteableNPCs =
        [
            [],
            []
        ];
        public ServerMessage ServerMessage { get; set; } = new ServerMessage();
        public List<DateTimeOffset> Quakes = [];
        public static PigParseStats[] PigParseStats = new PigParseStats[(int)Servers.MaxServers]
        {
            new(){ Server = Servers.Green, CssClass = "success"},
            new(){ Server = Servers.Blue , CssClass = "primary"},
            new(){ Server = Servers.Red , CssClass = "danger"},
            new(){ Server = Servers.Quarm , CssClass = "warning"},
        };
        private static DateTime LastPigParseStat = DateTime.UtcNow.AddMinutes(-10);

        public NoteableNPCZone[] ServerData { get; set; } = new NoteableNPCZone[(int)Servers.MaxServers];
        public IndexModel(DBData allData, EQToolContext eQToolContext, NoteableNPCCache noteableNPCCache)
        {
            this.noteableNPCCache = noteableNPCCache;
            AllData = allData;
            if ((DateTime.UtcNow - LastPigParseStat).TotalSeconds > 20)
            {
                lock (PigParseStats)
                {
                    if ((DateTime.UtcNow - LastPigParseStat).TotalSeconds > 20)
                    {
                        LastPigParseStat = DateTime.UtcNow;
                        var p99list = PPHub.connections.ToArray();
                        var quarmlist = MapHub.connections.ToArray();
                        foreach (var server in Enum.GetValues<Servers>().Where(a => a != Servers.MaxServers))
                        {
                            if (server == Servers.Quarm)
                            {
                                PigParseStats[(int)server].PigParsePlayerCount = quarmlist.Count(a => a.Value.Server == server);
                                PigParseStats[(int)server].zoneStats = quarmlist
                                    .Select(a => a.Value)
                                    .Where(a => a.Server == server)
                                    .GroupBy(a => a.Zone)
                                    .Select(a => new PigParseZoneStat { Zone = a.Key, Count = a.Count() })
                                    .Where(a => a.Zone is not "fearplane" and not "hateplane" and not "sleeper")
                                    .ToList();
                                var lasthour = DateTime.UtcNow.AddHours(-1);
                                PigParseStats[(int)server].PigParseUniquePlayerCount = eQToolContext.EqToolExceptions.Where(a => a.Server == server && a.DateCreated > lasthour).Select(a => a.IpAddress).Distinct().Count();
                            }
                            else
                            {
                                PigParseStats[(int)server].PigParsePlayerCount = p99list.Count(a => a.Value.Server == server);
                                PigParseStats[(int)server].zoneStats = p99list
                                    .Select(a => a.Value)
                                    .Where(a => a.Server == server)
                                    .GroupBy(a => a.Zone)
                                    .Select(a => new PigParseZoneStat { Zone = a.Key, Count = a.Count() })
                                    .Where(a => a.Zone is not "fearplane" and not "hateplane" and not "sleeper")
                                    .ToList();
                                var lasthour = DateTime.UtcNow.AddHours(-1);
                                PigParseStats[(int)server].PigParseUniquePlayerCount = eQToolContext.EqToolExceptions.Where(a => a.Server == server && a.DateCreated > lasthour).Select(a => a.IpAddress).Distinct().Count();
                            }
                        }
                    }
                }
            }

            ServerMessage = eQToolContext.ServerMessages.FirstOrDefault();
            var keyname = new List<KeyValuePair<string, string>>()
            {
                new("westwastes", "Scout Charisa"),
                new("westwastes", "a Kromzek Captain")
            };
            Quakes = eQToolContext.QuakeTimes.OrderByDescending(a => a.DateTime).Take(3).Select(a => a.DateTime).ToList();
            for (var server = 0; server < NoteableNPCs.Count; server++)
            {
                foreach (var item in keyname)
                {
                    var def = new TODModel
                    {
                        FixedTimeNPCDateTimes = [],
                        RangeTimeNPCDateTime = [],
                        Name = item.Value
                    };
                    NoteableNPCs[server].Add(def);
                    if (noteableNPCCache.ServerData[server].Zones.TryGetValue(item.Key, out var npc))
                    {
                        var n = npc.FirstOrDefault(a => a.Name == item.Value);
                        if (n != null)
                        {
                            def.EventTime = n.LastDeath ?? n.LastSeen ?? null;
                            if (n.Name == "Scout Charisa" && def.EventTime.HasValue)
                            {
                                for (var i = 1; i <= 5; i++)
                                {
                                    def.FixedTimeNPCDateTimes.Add(def.EventTime.Value.AddHours(10 * i));
                                }
                            }
                            else if (n.Name == "a Kromzek Captain" && def.EventTime.HasValue)
                            {
                                for (var i = 1; i <= 5; i++)
                                {
                                    def.FixedTimeNPCDateTimes.Add(def.EventTime.Value.AddHours(10 * i));
                                }
                            }
                        }
                    }
                }
            }

        }


        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
