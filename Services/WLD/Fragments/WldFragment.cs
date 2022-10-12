using System.Collections.Generic;
using System.IO;

namespace EQTool.Services.WLD.Fragments
{
    public abstract class WldFragment
    {
        public int Index { get; private set; }
        public int Size { get; private set; }
        public string Name { get; set; }

        public BinaryReader Reader { get; set; }
        public virtual void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            Index = index;
            Size = size;
            Reader = new BinaryReader(new MemoryStream(data));
        }
    }
}
