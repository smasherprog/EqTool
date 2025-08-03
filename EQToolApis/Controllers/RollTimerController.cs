using EQToolApis.DB;
using EQToolApis.Models;
using EQToolShared.APIModels;
using EQToolShared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EQToolApis.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/rolltimer")]
    public class RollTimerController : ControllerBase
    {
        private readonly EQToolContext dbcontext;
        private readonly NoteableNPCCache noteableNPCCache;

        public RollTimerController(EQToolContext dbcontext, NoteableNPCCache noteableNPCCache)
        {
            this.dbcontext = dbcontext;
            this.noteableNPCCache = noteableNPCCache;
        }

        [Route("timers/{server}"), HttpGet]
        public List<RollTimerModel> timers(Servers server)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip) || dbcontext.IPBans.Any(a => a.IpAddress == ip))
            {
                return [];
            }

            var keyname = new List<KeyValuePair<string, string>>()
            {
                new("westwastes", "Scout Charisa"),
                new("westwastes", "a Kromzek Captain")
            };
            var quake = dbcontext.QuakeTimes.Where(a => a.Server == server).OrderByDescending(a => a.DateTime).FirstOrDefault();
            var ret = new List<RollTimerModel>();
            if (quake != null)
            {
                ret.Add(new RollTimerModel
                {
                    Name = "Quake",
                    DateTime = quake.DateTime,
                    Guess = false,
                    RollTimerType = RollTimerType.Quake
                });
            }

            foreach (var item in keyname)
            {
                if (noteableNPCCache.ServerData[(int)server].Zones.TryGetValue(item.Key, out var npc))
                {
                    var n = npc.FirstOrDefault(a => a.Name == item.Value);
                    if (n != null)
                    {
                        var eventTime = n.LastDeath ?? n.LastSeen ?? null;
                        var events = new List<(DateTimeOffset d, bool guess)>();
                        if (n.Name == "Scout Charisa" && eventTime.HasValue)
                        {
                            var d = eventTime.Value.AddHours(10 * 2);
                            if (d > DateTimeOffset.Now)
                            {
                                events.Add((d, false));
                            }
                            else
                            {
                                events.Add((d, true));
                            }
                        }
                        else if (n.Name == "a Kromzek Captain" && eventTime.HasValue)
                        {
                            var d = eventTime.Value.AddHours(10 * 2);
                            if (d > DateTimeOffset.Now)
                            {
                                events.Add((d, false));
                            }
                            else
                            {
                                events.Add((d, true));
                            }
                        }

                        var closestNotGuess = events.Where(a => !a.guess).ToList();
                        if (closestNotGuess.Any())
                        {
                            var (d, guess) = closestNotGuess.OrderBy(a => a.d).FirstOrDefault();
                            ret.Add(new RollTimerModel
                            {
                                Name = item.Value,
                                DateTime = d,
                                Guess = false,
                                RollTimerType = RollTimerType.Scout
                            });
                        }
                        else
                        {
                            closestNotGuess = events.Where(a => a.guess).ToList();
                            var (d, guess) = closestNotGuess.OrderBy(a => a.d).FirstOrDefault();
                            ret.Add(new RollTimerModel
                            {
                                Name = item.Value,
                                DateTime = d,
                                Guess = true,
                                RollTimerType = RollTimerType.Scout
                            });
                        }
                    }
                }
            }

            return ret;
        }
    }
}