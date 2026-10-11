using EQTool.Services.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace EQTool.Services.IO
{
    public interface IFileReader
    {
        List<string> ReadNext(string filepath);
    }

    public class FileReader : IFileReader
    {
        // The time limit decides what is replayed; the byte window only has to be
        // large enough to hold 15 seconds of heavy combat spam.
        private const int RewindBytes = 256 * 1024;
        private static readonly TimeSpan RewindTime = TimeSpan.FromSeconds(15);

        private string LastLogFilename = string.Empty;
        private long? LastLogReadOffset { get; set; } = null;

        public FileReader()
        {
        }

        public List<string> ReadNext(string filepath)
        {
            if (filepath != LastLogFilename)
            {
                LastLogReadOffset = null;
                LastLogFilename = filepath;
            }
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var rewind = !LastLogReadOffset.HasValue || (LastLogReadOffset > stream.Length && stream.Length > 0);
                if (!rewind)
                {
                    return ReadCompleteLines(stream, LastLogReadOffset.Value);
                }

                Debug.WriteLine($"Player Switched or new Player detected {filepath} {stream.Length}");
                var cutoff = DateTime.Now - RewindTime;
                // also drops the partial first line the seek lands in, since its timestamp won't parse
                var recentlines = ReadCompleteLines(stream, Math.Max(0, stream.Length - RewindBytes))
                    .Where(line => line.Length >= 27 && LogFileDateTimeParse.TryParseDateTime(line.Substring(1, 24), out var timestamp) && timestamp >= cutoff)
                    .ToList();

                // Match the start of the message, not the whole line, so chat or a guild
                // MOTD that quotes one of these phrases is not mistaken for the system line.
                var lastfoundindex = 0;
                for (var lineindex = 0; lineindex < recentlines.Count; lineindex++)
                {
                    if (recentlines[lineindex].Substring(27).StartsWith("Welcome to EverQuest!"))
                    {
                        lastfoundindex = lineindex;
                    }
                }

                return recentlines.GetRange(lastfoundindex, recentlines.Count - lastfoundindex)
                    .Where(a =>
                    {
                        var message = a.Substring(27);
                        return message.StartsWith("Welcome to EverQuest!") ||
                            message.StartsWith("You have entered") ||
                            message.StartsWith("MESSAGE OF THE DAY") ||
                            message.StartsWith("GUILD MOTD");
                    }).ToList();
            }
        }

        // EQ may be partway through writing the last line, so stop after the last
        // newline and leave the rest for the next poll. The offset only moves once
        // the read has succeeded, so a failed read is simply retried.
        private List<string> ReadCompleteLines(FileStream stream, long start)
        {
            var bytes = new byte[Math.Max(0, stream.Length - start)];
            _ = stream.Seek(start, SeekOrigin.Begin);
            var read = 0;
            while (read < bytes.Length)
            {
                var count = stream.Read(bytes, read, bytes.Length - read);
                if (count == 0)
                {
                    break;
                }
                read += count;
            }

            var end = read - 1;
            while (end >= 0 && bytes[end] != '\n')
            {
                end--;
            }
            if (end < 0)
            {
                return new List<string>();
            }

            LastLogReadOffset = start + end + 1;
            return Encoding.UTF8.GetString(bytes, 0, end)
                .Split('\n')
                .Select(a => a.TrimEnd('\r'))
                .ToList();
        }
    }
}
