using System;
using System.Collections.Generic;

namespace EQToolShared
{
    public static class MasterNPCList
    {
        public static HashSet<string> NPCs = new HashSet<string>();
        static MasterNPCList()
        {
            NPCs = new HashSet<string>(Properties.Resources.MasterNPCList.Split(','), StringComparer.OrdinalIgnoreCase);
        }
    }
}
