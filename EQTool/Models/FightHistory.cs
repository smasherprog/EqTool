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
            var entry = fightEntries.FirstOrDefault(e => e.Name == name);
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
            _ = fightEntries.RemoveAll(a => a.Name == name);
            clean(lastSeen);
        }

        public bool IsEngaged(string name, DateTime now)
        {
            clean(now);
            return fightEntries.Any(e => e.Name == name);
        }

        public string GetMostRecentTarget(DateTime now)
        {
            clean(now);
            return fightEntries.OrderByDescending(e => e.LastSeen).FirstOrDefault()?.Name;
        }

        public List<string> IsEngaged(List<string> names, DateTime now)
        {
            clean(now);
            return fightEntries.Where(e => names.Contains(e.Name)).Select(a => a.Name).ToList();
        }

        public void clean(DateTime lastSeen)
        {
            var oldest = lastSeen.AddMinutes(-10);
            _ = fightEntries.RemoveAll(a => a.LastSeen < oldest);
        }
    }
}
