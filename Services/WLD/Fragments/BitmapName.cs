using System.Collections.Generic;

namespace EQTool.Services.WLD.Fragments
{
    /// <summary>
    /// BitmapName (0x03)
    /// Internal Name: None
    /// This fragment contains the name of a bitmap image. It supports more than one bitmap but this is never used.
    /// Fragment end is padded to end on a DWORD boundary.
    /// </summary>
    public class BitmapName : WldFragment
    {
        /// <summary>
        /// The filename of the referenced bitmap
        /// </summary>
        public string Filename { get; set; }

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);
            Name = stringHash[-Reader.ReadInt32()];

            // The client supports more than one bitmap reference but is never used
            _ = Reader.ReadInt32();

            int nameLength = Reader.ReadInt16();

            // Decode the bitmap name and trim the null character (c style strings)
            var nameBytes = Reader.ReadBytes(nameLength);
            Filename = WldStringDecoder.DecodeString(nameBytes);
            Filename = Filename.ToLower().Substring(0, Filename.Length - 1);
        }

        public string GetExportFilename()
        {
            return GetFilenameWithoutExtension() + ".png";
        }

        public string GetFilenameWithoutExtension()
        {
            return Filename.Substring(0, Filename.Length - 4);
        }
    }
}