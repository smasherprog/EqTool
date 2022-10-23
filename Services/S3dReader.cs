using EQTool.Models;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace EQTool.Services
{
    public class S3dReader
    {
        public List<SubS3bFile> Read()
        {
            using (var filestream = new FileStream(Properties.Settings.Default.DefaultEqDirectory + "/freportn.s3d", FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(filestream))
            {
                var directoryOffset = reader.ReadInt32();
                reader.BaseStream.Position = directoryOffset;

                var fileCount = reader.ReadInt32();
                var fileNames = new List<string>();
                var _files = new List<SubS3bFile>();

                for (var i = 0; i < fileCount; i++)
                {
                    var crc = reader.ReadUInt32();
                    var offset = reader.ReadUInt32();
                    var size = reader.ReadUInt32();

                    if (offset > reader.BaseStream.Length)
                    {
                        throw new System.Exception("Corrupted length detected!");
                    }

                    var cachedOffset = reader.BaseStream.Position;
                    var fileBytes = new byte[size];

                    reader.BaseStream.Position = offset;
                    uint inflatedSize = 0;
                    while (inflatedSize != size)
                    {
                        var deflatedLength = reader.ReadUInt32();
                        var inflatedLength = reader.ReadUInt32();
                        if (deflatedLength >= reader.BaseStream.Length)
                        {
                            throw new System.Exception("Corrupted length detected!");
                        }

                        var compressedBytes = reader.ReadBytes((int)deflatedLength);
                        var inflatedBytes = InflateBlock(compressedBytes);
                        inflatedBytes.CopyTo(fileBytes, inflatedSize);
                        if (inflatedLength != (uint)inflatedBytes.Length)
                        {
                            throw new System.Exception("Error occured inflating data!");
                        }

                        inflatedSize += inflatedLength;
                    }

                    if (crc == 0x61580AC9)
                    {
                        var dictionaryStream = new MemoryStream(fileBytes);
                        var dictionary = new BinaryReader(dictionaryStream);
                        var filenameCount = dictionary.ReadUInt32();

                        for (uint j = 0; j < filenameCount; ++j)
                        {
                            var fileNameLength = dictionary.ReadUInt32();
                            var filename = new string(dictionary.ReadChars((int)fileNameLength));
                            fileNames.Add(filename.Substring(0, filename.Length - 1));
                        }

                        reader.BaseStream.Position = cachedOffset;
                        continue;
                    }

                    _files.Add(new SubS3bFile(crc, size, offset, fileBytes));
                    reader.BaseStream.Position = cachedOffset;
                }

                _files.Sort((x, y) => x.Offset.CompareTo(y.Offset));
                for (var i = 0; i < _files.Count; ++i)
                {
                    _files[i].Name = fileNames[i];
                }

                return _files;
            }

        }
        private static byte[] InflateBlock(byte[] deflatedBytes)
        {
            using (var ms = new MemoryStream(deflatedBytes))
            {
                var msInner = new MemoryStream();
                // Read past the first two bytes of the zlib header
                _ = ms.Seek(2, SeekOrigin.Begin);

                using (var z = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    z.CopyTo(msInner);
                }
                return msInner.ToArray();
            }
        }
    }
}
