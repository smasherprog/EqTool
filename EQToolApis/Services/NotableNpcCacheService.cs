using EQToolApis.DB;
using EQToolApis.Models;
using EQToolShared.Enums;
using EQToolShared.Map;

namespace EQToolApis.Services
{
    public class NotableNpcCacheService
    {
        private readonly EQToolContext toolContext;
        private readonly NoteableNPCCache noteableNPCCache;
        public NotableNpcCacheService(EQToolContext toolContext, NoteableNPCCache noteableNPCCache)
        {
            this.toolContext = toolContext;
            this.noteableNPCCache = noteableNPCCache;
        }

        public void BuildCache()
        {
            var newnoteableNPCCache = new NoteableNPCCache();
            var zones = ZoneParser.ZoneInfoMap;
            var dbzones = toolContext.EQZones.ToList();
            var notablenpcs = toolContext.EQNotableNPCs.ToList();
            foreach (Servers server in Enum.GetValues(typeof(Servers)))
            {
                var deathnpcs = toolContext.EQNotableActivities
                .Where(a => a.Server == server && a.IsDeath)
                .GroupBy(a => a.EQNotableNPCId)
                .Select(a => new
                {
                    LastDeath = a.OrderByDescending(a => a.EQNotableActivityId).Select(b => b.ActivityTime).FirstOrDefault(),
                    a.FirstOrDefault().EQNotableNPC.Name
                }).ToList();

                var lastseennpcs = toolContext.EQNotableActivities
                   .Where(a => a.Server == server && !a.IsDeath)
                   .GroupBy(a => a.EQNotableNPCId)
                   .Select(a => new
                   {
                       LastSeen = a.OrderByDescending(a => a.EQNotableActivityId).Select(b => b.ActivityTime).FirstOrDefault(),
                       a.FirstOrDefault().EQNotableNPC.Name
                   }).ToList();

                foreach (var zone in zones)
                {
                    var dbzone = dbzones.FirstOrDefault(a => a.Name == zone.Key);
                    if (dbzone == null)
                    {
                        continue;
                    }

                    var npcdata = new List<NoteableNPC>();
                    foreach (var npc in zone.Value.NotableNPCs.Where(a => !string.IsNullOrWhiteSpace(a)))
                    {
                        npcdata.Add(new NoteableNPC
                        {
                            LastDeath = deathnpcs.FirstOrDefault(a => a.Name == npc)?.LastDeath,
                            LastSeen = lastseennpcs.FirstOrDefault(a => a.Name == npc)?.LastSeen,
                            Name = npc,
                            EQNotableNPCId = notablenpcs.FirstOrDefault(a => a.Name == npc).EQNotableNPCId
                        });
                    }
                    newnoteableNPCCache.ServerData[(int)server].Zones.Add(zone.Key, npcdata);
                }
            }
            noteableNPCCache.ServerData = newnoteableNPCCache.ServerData;
        }
    }
}
