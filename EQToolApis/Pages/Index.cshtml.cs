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
        private readonly EQToolContext eQToolContext;
        public List<NoteableNPC> GreenNoteableNPCs = new List<NoteableNPC>();

        public IndexModel(DBData allData, EQToolContext eQToolContext, NoteableNPCCache noteableNPCCache)
        {
            AllData = allData;
            this.eQToolContext = eQToolContext;
            this.noteableNPCCache = noteableNPCCache;
            var keyname = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("westwastes", "Scout Charisa"),
                new KeyValuePair<string, string>("westwastes", "A Kromzek Captain"),
                new KeyValuePair<string, string>("droga", "An angry goblin"),
                new KeyValuePair<string, string>("droga", "Warlord Skargus")
            };

            foreach (var item in keyname)
            {
                var def = new NoteableNPC { EQNotableNPCId = 0, Name = item.Value };
                if (noteableNPCCache.ServerData[(int)Servers.Green].Zones.TryGetValue(item.Key, out var npc))
                {
                    var n = npc.FirstOrDefault(a => a.Name == item.Value);
                    if (n != null)
                    {
                        GreenNoteableNPCs.Add(n);
                    }
                    else
                    {
                        GreenNoteableNPCs.Add(def);
                    }
                }
                else
                {
                    GreenNoteableNPCs.Add(def);
                }
            }
        }

        public ServerMessage ServerMessage { get; set; } = new ServerMessage();
        public IActionResult OnGet()
        {
            ServerMessage = eQToolContext.ServerMessages.FirstOrDefault();
            return Page();
        }
    }
}
