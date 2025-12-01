using EQTool.Models;
using EQToolShared;
using System;
using System.Diagnostics;
using EQTool.ViewModels;

namespace EQTool.Services.Parsing
{
    public class YouZonedParser : IEqLogParser
    {
        private const string Youhaveentered = "You have entered ";
        private const string Therearenoplayers = "There are no players ";
        private const string Thereare = "There are ";
        private const string Thereis = "There is ";
        private const string Youhaveenteredareapvp = "You have entered an Arena (PvP) area.";
        private const string spaceinspace = "in ";
        
        private readonly ActivePlayer player;
        private readonly LogEvents logEvents;

        public YouZonedParser(ActivePlayer player, LogEvents logEvents)
        {
            this.player = player;
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
            var longName = GetNameFromZoneChangedLine(message);
            
            if (string.IsNullOrWhiteSpace(longName) && player.Player?.Zone == null)
                longName = GetNameFromZoneWhoLine(message); // If we don't know what zone we're in and there's a /who, try and parse the zone
            
            if (!string.IsNullOrWhiteSpace(longName))
            {
                var shortName = Zones.TranslateToMapName(longName);
                if (!string.IsNullOrWhiteSpace(shortName))
                {
                    Debug.WriteLine($"Zone Detected {longName}");
                    return new YouZonedEvent {LongName = longName, ShortName = shortName, TimeStamp = timestamp, Line = message, LineCounter = lineCounter};
                }
            }

            return null;
        }

        private string GetNameFromZoneChangedLine(string message)
        {
            if (message == Youhaveenteredareapvp)
                return string.Empty;

            if (message.StartsWith(Youhaveentered))
                return message.Replace(Youhaveentered, string.Empty).Trim().TrimEnd('.').ToLower();

            return string.Empty;
        }
        
        private string GetNameFromZoneWhoLine(string message)
        {
            if (message.StartsWith(Therearenoplayers))
                return string.Empty;
            
            if (message.StartsWith(Thereare))
            {
                message = message.Replace(Thereare, string.Empty).Trim();
                var inindex = message.IndexOf(spaceinspace);
                if (inindex != -1)
                {
                    message = message.Substring(inindex + spaceinspace.Length).Trim().TrimEnd('.').ToLower();
                    if (message != "everquest")
                        return message;
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
                        return message;
                }
            }

            return string.Empty;
        }
    }
}
