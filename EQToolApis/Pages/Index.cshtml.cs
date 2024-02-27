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
        public List<TODModel> GreenNoteableNPCs = new List<TODModel>();
        public ServerMessage ServerMessage { get; set; } = new ServerMessage();
        public List<DateTimeOffset> Quakes = new List<DateTimeOffset>();
        public static PigParseStats[] PigParseStats = new PigParseStats[(int)Servers.MaxServers]
        {
            new PigParseStats(){ Server = Servers.Green },
            new PigParseStats(){ Server = Servers.Blue },
            new PigParseStats(){ Server = Servers.Red },
            new PigParseStats(){ Server = Servers.Quarm },
        };
        private static DateTime LastPigParseStat = DateTime.UtcNow;

        public NoteableNPCZone[] ServerData { get; set; } = new NoteableNPCZone[(int)Servers.MaxServers];
        public IndexModel(DBData allData, EQToolContext eQToolContext, NoteableNPCCache noteableNPCCache)
        {
            if ((DateTime.UtcNow - LastPigParseStat).TotalSeconds > 20)
            {
                lock (PigParseStats)
                {
                    if ((DateTime.UtcNow - LastPigParseStat).TotalSeconds > 20)
                    {
                        LastPigParseStat = DateTime.UtcNow;
                        var list = MapHub.connections.ToArray();
                        foreach (var server in Enum.GetValues<Servers>().Where(a => a != Servers.MaxServers))
                        {
                            PigParseStats[(int)server].PigParsePlayerCount = list.Count(a => a.Value.Server == server);
                            PigParseStats[(int)server].zoneStats = list
                                .Select(a => a.Value)
                                .Where(a => a.Server == server)
                                .GroupBy(a => a.Zone)
                                .Select(a => new PigParseZoneStat { Zone = a.Key, Count = a.Count() })
                                .Where(a => a.Zone != "fearplane" && a.Zone != "hateplane" && a.Zone != "sleeper")
                                .ToList();
                        }
                    }
                }
            }
            AllData = allData;
            this.noteableNPCCache = noteableNPCCache;
            ServerMessage = eQToolContext.ServerMessages.FirstOrDefault();
            var keyname = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("westwastes", "Scout Charisa"),
                new KeyValuePair<string, string>("westwastes", "a Kromzek Captain")
            };
            Quakes = eQToolContext.QuakeTimes.OrderByDescending(a => a.DateTime).Take(3).Select(a => a.DateTime).ToList();
            foreach (var item in keyname)
            {
                var def = new TODModel
                {
                    FixedTimeNPCDateTimes = new List<DateTimeOffset>(),
                    RangeTimeNPCDateTime = new List<RangeTimeNPCDateTime>(),
                    Name = item.Value
                };
                GreenNoteableNPCs.Add(def);
                if (noteableNPCCache.ServerData[(int)Servers.Green].Zones.TryGetValue(item.Key, out var npc))
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


        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
