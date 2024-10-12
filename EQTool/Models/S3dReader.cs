namespace EQTool.Models
{
    public class SubS3bFile
    {
        public uint Crc { get; }
        public uint Size { get; }
        public uint Offset { get; }
        public byte[] Bytes { get; }
        public string Name { get; set; }
        public SubS3bFile(uint crc, uint size, uint offset, byte[] bytes)
        {
            Crc = crc;
            Size = size;
            Offset = offset;
            Bytes = bytes;
        }
    }
}
