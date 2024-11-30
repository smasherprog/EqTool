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

        private List<FightEntry> fightEntries = new List<FightEntry>();
        public FightHistory()
        {

        }

        public void Add(string name, DateTime lastSeen)
        {
            var entry = this.fightEntries.FirstOrDefault(e => e.Name == name);
            if (entry != null)
            {
                entry.LastSeen = lastSeen;
            }
            else
            {
                this.fightEntries.Add(new FightEntry { Name = name, LastSeen = lastSeen });
            }
            clean(lastSeen);
        }

        public void Remove(string name, DateTime lastSeen)
        {
            this.fightEntries.RemoveAll(a => a.Name == name);
            clean(lastSeen);
        }

        public bool IsEngaged(string name, DateTime now)
        {
            clean(now);
            return this.fightEntries.Any(e => e.Name == name);
        }

        public List<string> IsEngaged(List<string> names, DateTime now)
        {
            clean(now);
            return this.fightEntries.Where(e => names.Contains(e.Name)).Select(a => a.Name).ToList();
        }

        public void clean(DateTime lastSeen)
        {
            var oldest = lastSeen.AddMinutes(-10);
            this.fightEntries.RemoveAll(a => a.LastSeen < oldest);
        }
    }
}
