using System;
using System.Collections.Generic;
using System.Linq;

namespace EQToolShared
{
    public static class MasterItemList
    {
        public static HashSet<string> ItemsFastLoopup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public static List<string> ItemsFastLoop = new List<string>();
        public static List<char> ValidChars = new List<char>();
        static MasterItemList()
        {
            var temp = Properties.Resources.MasterItemList.Split(',');
            ItemsFastLoop = temp.Distinct().OrderBy(s => !char.IsDigit(s.FirstOrDefault())).ThenByDescending(a => a.Length).ToList();
            ItemsFastLoopup = new HashSet<string>(ItemsFastLoop, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < ItemsFastLoop.Count; i++)
            {
                foreach (var c in ItemsFastLoop[i])
                {
                    if (!ValidChars.Contains(c))
                    {
                        ValidChars.Add(c);
                    }
                }
            }
            ItemsFastLoop.Add("Talisen, Bow of the Trailblazer");
            _ = ItemsFastLoopup.Add("Talisen, Bow of the Trailblazer");
        }
    }
}
