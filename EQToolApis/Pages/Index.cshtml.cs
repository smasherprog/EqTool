using EQToolApis.DB;
using EQToolApis.DB.Models;
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

        public IndexModel(DBData allData, EQToolContext eQToolContext, NoteableNPCCache noteableNPCCache)
        {
            AllData = allData;
            this.noteableNPCCache = noteableNPCCache;
            ServerMessage = eQToolContext.ServerMessages.FirstOrDefault();
            var keyname = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("westwastes", "Scout Charisa"),
                new KeyValuePair<string, string>("westwastes", "A Kromzek Captain"),
                new KeyValuePair<string, string>("droga", "An angry goblin"),
                new KeyValuePair<string, string>("droga", "Warlord Skargus"),
                new KeyValuePair<string, string>("warslikswood", "A shady goblin")

            };

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
                        else if (n.Name == "A Kromzek Captain" && def.EventTime.HasValue)
                        {
                            for (var i = 1; i <= 5; i++)
                            {
                                def.FixedTimeNPCDateTimes.Add(def.EventTime.Value.AddHours(10 * i));
                            }
                        }
                        else if (n.Name == "An angry goblin" && def.EventTime.HasValue)
                        {
                            def.RangeTimeNPCDateTime.Add(new RangeTimeNPCDateTime
                            {
                                BegWindow = def.EventTime.Value.AddMinutes(-432),
                                EndWindow = def.EventTime.Value.AddMinutes(432)
                            });
                        }
                        else if (n.Name == "Warlord Skargus" && def.EventTime.HasValue)
                        {
                            def.RangeTimeNPCDateTime.Add(new RangeTimeNPCDateTime
                            {
                                BegWindow = def.EventTime.Value.AddDays(3).AddMinutes(-432),
                                EndWindow = def.EventTime.Value.AddDays(3).AddMinutes(432)
                            });
                        }
                        else if (n.Name == "A shady goblin" && def.EventTime.HasValue)
                        {
                            for (var i = 1; i <= 5; i++)
                            {
                                def.FixedTimeNPCDateTimes.Add(def.EventTime.Value.AddHours(24 * i));
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
