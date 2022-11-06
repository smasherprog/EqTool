using System.Collections.Generic;

namespace EQTool.Services.WLD.Fragments
{
    internal class UnusedWldFragment : WldFragment
    {
        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
        }
    }
}
