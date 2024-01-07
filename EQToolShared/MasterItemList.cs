using System.Collections.Generic;
using System.Linq;

namespace EQToolShared
{
    public static class MasterItemList
    {
        public static string[] Items = new string[0];
        public static List<char> ValidChars = new List<char>();
        static MasterItemList()
        {
            Items = Properties.Resources.MasterItemList.Split(',');
            Items = Items.Distinct().OrderBy(s => !char.IsDigit(s.FirstOrDefault())).ThenByDescending(a => a.Length).ToArray();
            for (var i = 0; i < Items.Length; i++)
            {
                foreach (var c in Items[i])
                {
                    if (!ValidChars.Contains(c))
                    {
                        ValidChars.Add(c);
                    }
                }
            }
        }
    }
}
