using EQTool.Models;
using EQTool.ViewModels;
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
        private readonly ActivePlayer activePlayer;

        public YouZonedParser(LogEvents logEvents, ActivePlayer activePlayer)
        {
            this.logEvents = logEvents; 
            this.activePlayer = activePlayer;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = ZoneChanged(line);
            if (!string.IsNullOrWhiteSpace(m))
            {
                logEvents.Handle(new YouZonedEvent { ZoneName = m, TimeStamp = timestamp });
                return true;
            }
            return false;
        }

        public string ZoneChanged(string message)
        {
            var matchedzone = this._ZoneChanged(message);
            if (!string.IsNullOrWhiteSpace(matchedzone))
            {
                matchedzone = Zones.TranslateToMapName(matchedzone);
                Debug.WriteLine($"Zone Detected {matchedzone}");
                if(this.activePlayer.Player.Zone != matchedzone)
                { 
                    return matchedzone;
                }
            }
            return string.Empty;
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
