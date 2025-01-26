using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EQTool.Services.IO
{
    public interface IFileReader
    {
        List<string> ReadNext(string filepath);
    }

    public class FileReader : IFileReader
    {
        private string LastLogFilename = string.Empty;
        private long? LastLogReadOffset { get; set; } = null;
        private readonly LogEvents logEvents;

        public FileReader(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public List<string> ReadNext(string filepath)
        {
            var newPlayerEventEmitted = false;
            if (filepath != LastLogFilename)
            {
                newPlayerEventEmitted = true;
                LastLogReadOffset = null;
                LastLogFilename = filepath;
            }
            var fileinfo = new FileInfo(filepath);
            if (!LastLogReadOffset.HasValue || (LastLogReadOffset > fileinfo.Length && fileinfo.Length > 0))
            {
                newPlayerEventEmitted = true;
                Debug.WriteLine($"Player Switched or new Player detected {filepath} {fileinfo.Length}");
                LastLogReadOffset = fileinfo.Length;
                logEvents.Handle(new PayerChangedEvent { TimeStamp = DateTime.Now });
            }
            var linelist = new List<string>();
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                if (newPlayerEventEmitted)
                {
                    var gobackBytes = LastLogReadOffset - 4000;
                    if (gobackBytes < 0)
                    {
                        gobackBytes = 0;
                    }
                    _ = stream.Seek((long)gobackBytes, SeekOrigin.Begin);
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line.IndexOf("Welcome to EverQuest!") != -1)
                        {
                            linelist.Add(line);
                            break;
                        }
                    }
                }
                else
                {
                    _ = stream.Seek(LastLogReadOffset.Value, SeekOrigin.Begin);
                }

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    linelist.Add(line);
                    LastLogReadOffset = stream.Position;
                }
            }

            return linelist;
        }
    }
}
