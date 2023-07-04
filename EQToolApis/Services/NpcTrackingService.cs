using EQToolApis.DB;
using EQToolApis.DB.Models;
using EQToolApis.Models;
using EQToolShared.APIModels.ZoneControllerModels;

namespace EQToolApis.Services
{
    public class NpcTrackingService
    {
        private readonly EQToolContext dbcontext;
        private readonly NoteableNPCCache noteableNPCCache;

        public NpcTrackingService(EQToolContext dbcontext, NoteableNPCCache noteableNPCCache)
        {
            this.dbcontext = dbcontext;
            this.noteableNPCCache = noteableNPCCache;
        }

        public void Add(NPCActivityRequest npcdata, string ipAddress)
        {
            if (npcdata.IsDeath)
            {
                AddDeath(npcdata, ipAddress);
            }
            else
            {
                AddSeen(npcdata, ipAddress);
            }
        }

        private void AddSeen(NPCActivityRequest npcdata, string ipAddress)
        {
            var zone = dbcontext.EQZones.FirstOrDefault(a => a.Name == npcdata.NPCData.Zone);
            if (zone != null && noteableNPCCache.ServerData[(int)npcdata.Server].Zones.TryGetValue(zone.Name, out var cache))
            {
                var cacheentry = cache.FirstOrDefault(a => a.Name == npcdata.NPCData.Name);
                if (cacheentry != null)
                {
                    var dbupdateneeded = false;
                    if (cacheentry.LastSeen == null || cacheentry.LastSeen.Value < DateTimeOffset.Now.AddMinutes(-1))
                    {
                        dbupdateneeded = true;
                        cacheentry.LastSeen = DateTimeOffset.Now;
                    }

                    if (dbupdateneeded)
                    {
                        _ = dbcontext.Add(new EQNotableActivity
                        {
                            ActivityTime = DateTimeOffset.Now,
                            EQNotableNPCId = cacheentry.EQNotableNPCId,
                            IsDeath = false,
                            LocX = npcdata.NPCData.LocX,
                            LocY = npcdata.NPCData.LocY,
                            Server = npcdata.Server
                        });
                        _ = dbcontext.Add(new APILog
                        {
                            APIAction = APIAction.NPCActivity,
                            IpAddress = ipAddress,
                            LogMessage = $"{npcdata.NPCData.Zone}-{npcdata.NPCData.Name}"
                        });
                        _ = dbcontext.SaveChanges();
                    }
                }
            }

        }

        private void AddDeath(NPCActivityRequest npcdata, string ipAddress)
        {

            var zone = dbcontext.EQZones.FirstOrDefault(a => a.Name == npcdata.NPCData.Zone);
            if (zone != null)
            {
                var apilog = new APILog
                {
                    IpAddress = ipAddress,
                    APIAction = APIAction.DeathActivity,
                    LogMessage = $"{npcdata.NPCData.Name}-{npcdata.NPCData.Zone}"
                };
                apilog.APIAction = APIAction.DeathActivity;
                _ = dbcontext.Add(apilog);
                _ = dbcontext.Add(new EQDeath
                {
                    EQDeathTime = DateTime.UtcNow,
                    EQZoneId = zone.EQZoneId,
                    LocX = npcdata.NPCData.LocX,
                    LocY = npcdata.NPCData.LocY,
                    Name = npcdata.NPCData.Name,
                    Server = npcdata.Server
                });

                if (noteableNPCCache.ServerData[(int)npcdata.Server].Zones.TryGetValue(zone.Name, out var cache))
                {
                    var cacheentry = cache.FirstOrDefault(a => a.Name == npcdata.NPCData.Name);
                    if (cacheentry != null)
                    {
                        var dbupdateneeded = false;
                        if (cacheentry.LastSeen == null || cacheentry.LastSeen.Value < DateTimeOffset.Now.AddMinutes(-1))
                        {
                            dbupdateneeded = true;
                            cacheentry.LastSeen = DateTimeOffset.Now;
                        }
                        if (cacheentry.LastDeath == null || cacheentry.LastDeath.Value < DateTimeOffset.Now.AddMinutes(-1))
                        {
                            dbupdateneeded = true;
                            cacheentry.LastDeath = DateTimeOffset.Now;
                        }

                        if (dbupdateneeded)
                        {
                            _ = dbcontext.Add(new EQNotableActivity
                            {
                                ActivityTime = DateTimeOffset.Now,
                                EQNotableNPCId = cacheentry.EQNotableNPCId,
                                IsDeath = true,
                                LocX = npcdata.NPCData.LocX,
                                LocY = npcdata.NPCData.LocY,
                                Server = npcdata.Server
                            });
                            _ = dbcontext.SaveChanges();
                        }
                    }
                }
                _ = dbcontext.SaveChanges();
            }
        }
    }
}
