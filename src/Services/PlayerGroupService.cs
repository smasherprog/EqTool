using EQTool.Services.Spells.Log;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.TextFormatting;

namespace EQTool.Services
{
    public class Group
    {
        public List<PlayerWhoLogParse.PlayerInfo> Players { get; set; } = new List<PlayerWhoLogParse.PlayerInfo>();
    }
    public enum GroupOptimization
    {
        Standard,
        HOT_Cleric_SameGroup,
        HOT_Cleric_SparseGroup
    }

    public class PlayerGroupService
    {
        public PlayerGroupService()
        {
        }
        private (List<Group> groups, List<PlayerWhoLogParse.PlayerInfo> players) Prepare(List<PlayerWhoLogParse.PlayerInfo> players)
        {
            var groups = new List<Group>();
            if (!players.Any())
            {
                return (groups, players);
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

            return (groups, players);
        }

        public List<Group> CreateHOT_Clerics_SparseGroups(List<PlayerWhoLogParse.PlayerInfo> players)
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

            var groupindex = 0;
            var healers = players.Where(a => a.PlayerClass == Models.PlayerClasses.Cleric).Take(3).ToList();
            groups[groupindex].Players.AddRange(healers);
            foreach (var item in healers)
            {
                _ = players.Remove(item);
            }

            var bards = players.Where(a => a.PlayerClass == Models.PlayerClasses.Bard).Take(1).ToList();
            groups[groupindex].Players.AddRange(bards);
            foreach (var item in bards)
            {
                _ = players.Remove(item);
            }

            healers = players.Where(a => a.PlayerClass == Models.PlayerClasses.Cleric || a.PlayerClass == Models.PlayerClasses.Druid || a.PlayerClass == Models.PlayerClasses.Shaman).ToList();
            groupindex = 1;//skip group 1
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
            bards = players.Where(a => a.PlayerClass == Models.PlayerClasses.Bard).ToList();
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

        public List<Group> CreateHOT_Clerics_SameGroups(List<PlayerWhoLogParse.PlayerInfo> p)
        {
            var obj = Prepare(p);

            var groupindex = 0;
            var healers = obj.players.Where(a => a.PlayerClass == Models.PlayerClasses.Cleric).Take(4).ToList();
            obj.groups[groupindex].Players.AddRange(healers);
            foreach (var item in healers)
            {
                _ = obj.players.Remove(item);
            }

            var bards = obj.players.Where(a => a.PlayerClass == Models.PlayerClasses.Bard).Take(1).ToList();
            obj.groups[groupindex].Players.AddRange(bards);
            foreach (var item in bards)
            {
                _ = obj.players.Remove(item);
            }
            var mage = obj.players.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Magician);
            if (mage != null)
            {
                obj.groups[groupindex].Players.Add(mage);
                _ = obj.players.Remove(mage);
            }
            else
            {
                var necro = obj.players.FirstOrDefault(a => a.PlayerClass == Models.PlayerClasses.Necromancer);
                obj.groups[groupindex].Players.Add(necro);
                _ = obj.players.Remove(necro);
            }

            return this.CreateStandardGroups(obj.players, obj.groups);
        }


        public List<Group> CreateStandardGroups(List<PlayerWhoLogParse.PlayerInfo> p)
        {
            var obj = Prepare(p);
            return this.CreateStandardGroups(obj.players, obj.groups);
        }

        private void AddPlayersToGroupsEvenly(List<PlayerWhoLogParse.PlayerInfo> masterlist, List<PlayerWhoLogParse.PlayerInfo> playerstoadd, List<Group> groups)
        {
            while (playerstoadd.Any())
            {
                var workdone = false;
                foreach (var group in groups)
                {
                    var player = playerstoadd.FirstOrDefault();
                    if (player != null && group.Players.Count < 6)
                    {
                        workdone = true;
                        group.Players.Add(player);
                        _ = playerstoadd.Remove(player);
                        _ = masterlist.Remove(player);
                    }
                }
                if (!workdone)
                {
                    return;
                }
            }
        }

        private List<Group> CreateStandardGroups(List<PlayerWhoLogParse.PlayerInfo> p, List<Group> groups)
        {
            if (!p.Any())
            {
                return groups;
            }
            var players = p.OrderByDescending(a => a.Level).ToList();
            var numberofgroups = groups.Count;
            var healertypes = new List<Models.PlayerClasses?>()
            {
                Models.PlayerClasses.Cleric,
                Models.PlayerClasses.Druid,
                Models.PlayerClasses.Shaman
            };

            var bards = players.Where(a => a.PlayerClass == Models.PlayerClasses.Bard).ToList();
            AddPlayersToGroupsEvenly(players, bards, groups);
             
            var healers = players.Where(a => a.PlayerClass == Models.PlayerClasses.Cleric).ToList();
            AddPlayersToGroupsEvenly(players, healers, groups);

            var groupswithoutclerics = groups.Where(a => a.Players.Count < 6 && !a.Players.Any(b => b.PlayerClass == Models.PlayerClasses.Cleric)).ToList();
            healers = players.Where(a => a.PlayerClass == Models.PlayerClasses.Shaman && a.Level == 60).ToList();
            AddPlayersToGroupsEvenly(players, healers, groupswithoutclerics);

            var groupswithouthealer = groups.Where(a => a.Players.Count < 6 && !a.Players.Any(b => healertypes.Contains(b.PlayerClass))).ToList();
            healers = players.Where(a => a.PlayerClass == Models.PlayerClasses.Druid).ToList();
            AddPlayersToGroupsEvenly(players, healers, groupswithouthealer);

            groupswithouthealer = groups.Where(a => a.Players.Count < 6 && !a.Players.Any(b => healertypes.Contains(b.PlayerClass))).ToList();
            healers = players.Where(a => a.PlayerClass == Models.PlayerClasses.Shaman && a.Level != 60).ToList();
            AddPlayersToGroupsEvenly(players, healers, groupswithouthealer);

            var groupswithclerics = groups.Where(a => a.Players.Count < 6 && a.Players.Any(b => b.PlayerClass == Models.PlayerClasses.Cleric)).ToList();
            var warriors = players.Where(a => a.PlayerClass == Models.PlayerClasses.Warrior).ToList();
            AddPlayersToGroupsEvenly(players, warriors, groupswithclerics);

            var groupswithhealers = groups.Where(a => a.Players.Count < 6 && a.Players.Any(b => healertypes.Contains(b.PlayerClass))).ToList();
            var enchanters = players.Where(a => a.PlayerClass == Models.PlayerClasses.Enchanter).ToList();
            AddPlayersToGroupsEvenly(players, enchanters, groupswithhealers);

            groupswithhealers = groups.Where(a => a.Players.Count < 6 && a.Players.Any(b => healertypes.Contains(b.PlayerClass))).ToList();
            var rogues = players.Where(a => a.PlayerClass == Models.PlayerClasses.Rogue).ToList();
            AddPlayersToGroupsEvenly(players, rogues, groupswithhealers);

            groupswithhealers = groups.Where(a => a.Players.Count < 6 && a.Players.Any(b => healertypes.Contains(b.PlayerClass))).ToList();
            var meleetypes = new List<Models.PlayerClasses?>()
            {
                Models.PlayerClasses.Warrior,
                Models.PlayerClasses.Rogue,
                Models.PlayerClasses.Ranger,
                Models.PlayerClasses.Paladin,
                Models.PlayerClasses.ShadowKnight
            };

            var melees = players.Where(a => meleetypes.Contains(a.PlayerClass)).ToList();
            AddPlayersToGroupsEvenly(players, melees, groupswithhealers);

            AddPlayersToGroupsEvenly(players, players, groups);
            return groups;
        }
    }
}
