using EQTool.Models;
using EQToolShared;
using System;
using System.Diagnostics;

namespace EQTool.Services.Parsing
{
    public class YouZonedParser : IEqLogParseHandler
    {
        private const string Youhaveentered = "You have entered ";
        private const string Therearenoplayers = "There are no players ";
        private const string Thereare = "There are ";
        private const string Thereis = "There is ";
        private const string Youhaveenteredareapvp = "You have entered an Arena (PvP) area.";
        private const string spaceinspace = "in ";
        private readonly LogEvents logEvents;

        public YouZonedParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;

        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = ZoneChanged(line, timestamp, lineCounter);
            if (m != null)
            {
                logEvents.Handle(m);
                return true;
            }
            return false;
        }

        public YouZonedEvent ZoneChanged(string message, DateTime timestamp, int lineCounter)
        {
            var longName = _ZoneChanged(message);
            if (!string.IsNullOrWhiteSpace(longName))
            {
                var shortName = Zones.TranslateToMapName(longName);
                if (!string.IsNullOrWhiteSpace(shortName))
                {
                    Debug.WriteLine($"Zone Detected {longName}");
                    return new YouZonedEvent { LongName = longName, ShortName = shortName, TimeStamp = timestamp, Line = message, LineCounter = lineCounter };
                }
            }

            return null;
        }

        private string _ZoneChanged(string message)
        {
            if (message.StartsWith(Youhaveentered))
            {
                message = message.Replace(Youhaveentered, string.Empty).Trim().TrimEnd('.').ToLower();
                return message;
            }
            else if (message.StartsWith(Thereare))
            {
                message = message.Replace(Thereare, string.Empty).Trim();
                var inindex = message.IndexOf(spaceinspace);
                if (inindex != -1)
                {
                    message = message.Substring(inindex + spaceinspace.Length).Trim().TrimEnd('.').ToLower();
                    if (message != "everquest")
                    {
                        return message;
                    }
                }
            }
            else if (message.StartsWith(Thereis))
            {
                message = message.Replace(Thereis, string.Empty).Trim();
                var inindex = message.IndexOf(spaceinspace);
                if (inindex != -1)
                {
                    message = message.Substring(inindex + spaceinspace.Length).Trim().TrimEnd('.').ToLower();
                    if (message != "everquest")
                    {
                        return message;
                    }
                }
            }

            return string.Empty;
        }
    }
}
