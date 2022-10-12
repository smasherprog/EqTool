using System.Collections.Generic;

namespace EQTool.Services.WLD.Fragments
{
    /// <summary>
    /// BitmapInfo (0x04)
    /// Internal name: _SPRITE
    /// This fragment contains a reference to a 0x03 fragment and information about animation.
    /// </summary>
    public class BitmapInfo : WldFragment
    {
        /// <summary>
        /// Is the texture animated?
        /// </summary>
        public bool IsAnimated { get; private set; }

        /// <summary>
        /// The bitmap names referenced. 
        /// </summary>
        public List<BitmapName> BitmapNames { get; private set; }

        /// <summary>
        /// The number of milliseconds before the next texture is swapped.
        /// </summary>
        public int AnimationDelayMs { get; private set; }

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
            Name = stringHash[-Reader.ReadInt32()];
            var flags = Reader.ReadInt32();
            var bitAnalyzer = new BitAnalyzer(flags);
            IsAnimated = bitAnalyzer.IsBitSet(3);
            var bitmapCount = Reader.ReadInt32();

            BitmapNames = new List<BitmapName>();

            if (IsAnimated)
            {
                AnimationDelayMs = Reader.ReadInt32();
            }

            for (var i = 0; i < bitmapCount; ++i)
            {
                BitmapNames.Add(fragments[Reader.ReadInt32() - 1] as BitmapName);
            }
        }
    }
}