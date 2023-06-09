using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EQTool.Services
{
    public class PlayerTrackerService
    {
        private readonly LogParser logParser;
        private readonly Dictionary<string, PlayerWhoLogParse.PlayerInfo> Player = new Dictionary<string, PlayerWhoLogParse.PlayerInfo>(StringComparer.InvariantCultureIgnoreCase);
        private readonly List<PlayerWhoLogParse.PlayerInfo> PlayerZones = new List<PlayerWhoLogParse.PlayerInfo>();
        private readonly string CurrentZone;
        private readonly System.Timers.Timer UITimer;

        public PlayerTrackerService(LogParser logParser, ActivePlayer activePlayer)
        {
            _ = activePlayer.Update();
            CurrentZone = activePlayer.Player?.Zone;
            this.logParser = logParser;
            this.logParser.PlayerZonedEvent += LogParser_PlayerZonedEvent;
            this.logParser.WhoEvent += LogParser_WhoEvent;
            this.logParser.WhoPlayerEvent += LogParser_WhoPlayerEvent;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += UITimer_Elapsed; ;
            UITimer.Enabled = true;
        }

        private void UITimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }

        private void LogParser_WhoEvent(object sender, LogParser.WhoEventArgs e)
        {
            Debug.WriteLine("Clearing zone Players");
            PlayerZones.Clear();
        }

        private void LogParser_WhoPlayerEvent(object sender, LogParser.WhoPlayerEventArgs e)
        {
            if (Player.TryGetValue(e.PlayerInfo.Name, out var playerinfo))
            {
                playerinfo.Level = playerinfo.Level ?? e.PlayerInfo.Level;
                playerinfo.GuildName = playerinfo.GuildName ?? e.PlayerInfo.GuildName;
                playerinfo.Class = playerinfo.Class ?? e.PlayerInfo.Class;
                PlayerZones.Add(playerinfo);
                Debug.WriteLine($"Updating {playerinfo.Name} {playerinfo.Level} {playerinfo.GuildName} {playerinfo.Class}");
            }
            else
            {
                Player.Add(e.PlayerInfo.Name, e.PlayerInfo);
                PlayerZones.Add(e.PlayerInfo);
                Debug.WriteLine($"Adding {e.PlayerInfo.Name} {e.PlayerInfo.Level} {e.PlayerInfo.GuildName} {e.PlayerInfo.Class}");
            }
        }

        private void LogParser_PlayerZonedEvent(object sender, LogParser.PlayerZonedEventArgs e)
        {
            if (CurrentZone != e.Zone)
            {
                Debug.WriteLine("Clearing zone Players");
                PlayerZones.Clear();
            }
        }
    }
}
