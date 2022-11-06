using System.Text;

namespace EQTool.Services.WLD
{
    public static class WldStringDecoder
    {
        private static readonly byte[] HashKey = { 0x95, 0x3A, 0xC5, 0x2A, 0x95, 0x7A, 0x95, 0x6A };

        public static string DecodeString(byte[] encodedString)
        {
            for (var i = 0; i < encodedString.Length; ++i)
            {
                encodedString[i] ^= HashKey[i % 8];
            }

            return Encoding.UTF8.GetString(encodedString, 0, encodedString.Length);
        }
    }
}
