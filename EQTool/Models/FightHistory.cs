using EQToolShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Models
{
    public class FightHistory
    {
        private class FightEntry
        {
            public string Name { get; set; }
            public DateTime LastSeen { get; set; }
        }

        private readonly List<FightEntry> fightEntries = new List<FightEntry>();
        public FightHistory()
        {

        }

        public void Add(string name, DateTime lastSeen)
        {
            if (string.Equals(name, EQSpells.You, StringComparison.OrdinalIgnoreCase) || string.Equals(name, EQSpells.You.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var entry = fightEntries.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
            if (entry != null)
            {
                entry.LastSeen = lastSeen;
            }
            else
            {
                fightEntries.Add(new FightEntry { Name = name, LastSeen = lastSeen });
            }
            clean(lastSeen);
        }

        public void Remove(string name, DateTime lastSeen)
        {
            _ = fightEntries.RemoveAll(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
            clean(lastSeen);
        }

        public bool IsEngaged(string name, DateTime now)
        {
            clean(now);
            return fightEntries.Any(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public string GetMostRecentTarget(DateTime now)
        {
            clean(now);
            foreach (var item in fightEntries.OrderByDescending(e => e.LastSeen).Take(5))
            {
                if (MasterNPCList.NPCs.Contains(item.Name))
                {
                    return item.Name;
                }
            }
            return string.Empty;
        }

        public List<string> IsEngaged(List<string> names, DateTime now)
        {
            clean(now);
            return fightEntries.Where(e => names.Any(a => string.Equals(a, e.Name, StringComparison.OrdinalIgnoreCase))).Select(a => a.Name).ToList();
        }

        public void clean(DateTime lastSeen)
        {
            var oldest = lastSeen.AddMinutes(-10);
            _ = fightEntries.RemoveAll(a => a.LastSeen < oldest);
        }
    }
}
