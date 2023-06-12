using EQTool.Services.Spells.Log;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services
{
    public class Group
    {
        public List<PlayerWhoLogParse.PlayerInfo> Players { get; set; } = new List<PlayerWhoLogParse.PlayerInfo>();
    }
    public enum GroupOptimization
    {
        Standard,
        CHChain,
    }

    public class PlayerGroupService
    {
        public PlayerGroupService()
        {
        }

        public List<Group> CreateChChainGroups(List<PlayerWhoLogParse.PlayerInfo> players)
        {
            var groups = new List<Group>();
            return groups;
        }

        public List<Group> CreateStandardGroups(List<PlayerWhoLogParse.PlayerInfo> players)
        {
            var groups = new List<Group>();
            if (!players.Any())
            {
                return groups;
            }
            players = players.OrderByDescending(a => a.Level).ToList();
            var numberofgroups = players.Count / 6;
            if (players.Count % 6 != 0)
            {
                numberofgroups += 1;
            }

            for (var i = 0; i < numberofgroups; i++)
            {
                groups.Add(new Group());
            }
            //first add a healer to each group round robin style circling back adding 2 healers 
            var groupindex = 0;
            var healers = players.Where(a => a.PlayerClass == Models.PlayerClasses.Cleric || a.PlayerClass == Models.PlayerClasses.Druid || a.PlayerClass == Models.PlayerClasses.Shaman).ToList();
            while (healers.Any())
            {
                if (groupindex >= numberofgroups)
                {
                    groupindex = 0;
                }
                var healer = healers.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Cleric);
                if (healer == null)
                {
                    healer = healers.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Shaman);
                }
                if (healer == null)
                {
                    healer = healers.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Druid);
                }

                if (healer != null)
                {
                    groups[groupindex++].Players.Add(healer);
                    _ = players.Remove(healer);
                    _ = healers.Remove(healer);
                }
            }

            groupindex = 0;
            var bards = players.Where(a => a.PlayerClass == Models.PlayerClasses.Bard).ToList();
            while (bards.Any())
            {
                if (groupindex >= numberofgroups)
                {
                    groupindex = 0;
                }
                var bard = bards.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Bard);
                if (bard != null)
                {
                    groups[groupindex++].Players.Add(bard);
                    _ = players.Remove(bard);
                    _ = bards.Remove(bard);
                }
            }

            //add one round of ench to make sure they are paired with a healer
            groupindex = 0;
            var enchs = players.Where(a => a.PlayerClass == Models.PlayerClasses.Enchanter).ToList();
            while (enchs.Any())
            {
                if (groupindex >= numberofgroups)
                {
                    break;
                }
                var ench = enchs.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Enchanter);
                if (ench != null)
                {
                    var group = groups[groupindex++].Players;
                    if (group.Any(a => a.PlayerClass == Models.PlayerClasses.Cleric || a.PlayerClass == Models.PlayerClasses.Druid || a.PlayerClass == Models.PlayerClasses.Shaman))
                    {
                        group.Add(ench);
                        _ = players.Remove(ench);
                    }
                    _ = enchs.Remove(ench);
                }
            }

            groupindex = 0;
            var melees = players.Where(a => a.PlayerClass == Models.PlayerClasses.Warrior || a.PlayerClass == Models.PlayerClasses.Paladin || a.PlayerClass == Models.PlayerClasses.Ranger || a.PlayerClass == Models.PlayerClasses.Rogue).ToList();
            while (melees.Any())
            {
                if (groupindex >= numberofgroups)
                {
                    groupindex = 0;
                }
                var melee = melees.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Warrior);
                if (melee == null)
                {
                    melee = melees.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Rogue);
                }
                if (melee == null)
                {
                    melee = melees.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Ranger);
                }
                if (melee == null)
                {
                    melee = melees.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Paladin);
                }

                if (melee != null)
                {
                    groups[groupindex++].Players.Add(melee);
                    _ = players.Remove(melee);
                    _ = melees.Remove(melee);
                }
            }

            groupindex = 0;
            var casters = players.Where(a => a.PlayerClass == Models.PlayerClasses.Warrior || a.PlayerClass == Models.PlayerClasses.Paladin || a.PlayerClass == Models.PlayerClasses.Ranger || a.PlayerClass == Models.PlayerClasses.Rogue).ToList();
            while (melees.Any())
            {
                if (groupindex >= numberofgroups)
                {
                    groupindex = 0;
                }
                var melee = melees.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Warrior);
                if (melee == null)
                {
                    melee = melees.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Rogue);
                }
                if (melee == null)
                {
                    melee = melees.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Ranger);
                }
                if (melee == null)
                {
                    melee = melees.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Paladin);
                }

                if (melee != null)
                {
                    groups[groupindex++].Players.Add(melee);
                    _ = players.Remove(melee);
                    _ = melees.Remove(melee);
                }
            }

            while (players.Any())
            {
                if (groupindex >= numberofgroups)
                {
                    groupindex = 0;
                }
                var melee = players.FirstOrDefault();

                if (melee != null)
                {
                    groups[groupindex++].Players.Add(melee);
                    _ = players.Remove(melee);
                }
            }

            return groups;
        }
    }
}
