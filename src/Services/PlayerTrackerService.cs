using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EQTool.Services
{
    public class PlayerTrackerService
    {
        private readonly LogParser logParser;
        private readonly PigParseApi pigParseApi;
        private readonly ActivePlayer activePlayer;
        private readonly LoggingService loggingService;
        private readonly Dictionary<string, PlayerWhoLogParse.PlayerInfo> Player = new Dictionary<string, PlayerWhoLogParse.PlayerInfo>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, PlayerWhoLogParse.PlayerInfo> PlayerZones = new Dictionary<string, PlayerWhoLogParse.PlayerInfo>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, PlayerWhoLogParse.PlayerInfo> DirtyPlayers = new Dictionary<string, PlayerWhoLogParse.PlayerInfo>(StringComparer.InvariantCultureIgnoreCase);
        private readonly string CurrentZone;
        private readonly System.Timers.Timer UITimer;
        private readonly object ContainerLock = new object();

        public PlayerTrackerService(LogParser logParser, ActivePlayer activePlayer, PigParseApi pigParseApi, LoggingService loggingService)
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
            this.pigParseApi = pigParseApi;
            this.activePlayer = activePlayer;
            this.loggingService = loggingService;
        }

        private void UITimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var playerstosync = new List<PlayerWhoLogParse.PlayerInfo>();
            lock (ContainerLock)
            {
                playerstosync = DirtyPlayers.Values.ToList();
                DirtyPlayers.Clear();
            }
            try
            {
                pigParseApi.SendData(playerstosync, activePlayer.Player.Server.Value);
            }
            catch (Exception ex)
            {
                loggingService.Log(ex.ToString(), App.EventType.Error);
            }
        }

        private void LogParser_WhoEvent(object sender, LogParser.WhoEventArgs e)
        {
            Debug.WriteLine("Clearing zone Players");
            lock (ContainerLock)
            {
                PlayerZones.Clear();
            }
        }

        private void LogParser_WhoPlayerEvent(object sender, LogParser.WhoPlayerEventArgs e)
        {
            lock (ContainerLock)
            {
                if (Player.TryGetValue(e.PlayerInfo.Name, out var playerinfo))
                {
                    if (
                        (playerinfo.Level != e.PlayerInfo.Level && e.PlayerInfo.Level.HasValue) ||
                        (playerinfo.GuildName != e.PlayerInfo.GuildName && !string.IsNullOrWhiteSpace(e.PlayerInfo.GuildName)) ||
                        (playerinfo.PlayerClass != e.PlayerInfo.PlayerClass && e.PlayerInfo.PlayerClass.HasValue)
                        )
                    {
                        if (!DirtyPlayers.ContainsKey(e.PlayerInfo.Name))
                        {
                            DirtyPlayers[e.PlayerInfo.Name] = e.PlayerInfo;
                        }
                    }
                    playerinfo.Level = playerinfo.Level ?? e.PlayerInfo.Level;
                    playerinfo.GuildName = playerinfo.GuildName ?? e.PlayerInfo.GuildName;
                    playerinfo.PlayerClass = playerinfo.PlayerClass ?? e.PlayerInfo.PlayerClass;
                    Debug.WriteLine($"Updating {playerinfo.Name} {playerinfo.Level} {playerinfo.GuildName} {playerinfo.PlayerClass}");
                }
                else
                {
                    Player.Add(e.PlayerInfo.Name, e.PlayerInfo);
                    if (!PlayerZones.ContainsKey(e.PlayerInfo.Name))
                    {
                        PlayerZones[e.PlayerInfo.Name] = e.PlayerInfo;
                    }
                    if (!DirtyPlayers.ContainsKey(e.PlayerInfo.Name))
                    {
                        DirtyPlayers[e.PlayerInfo.Name] = e.PlayerInfo;
                    }
                    Debug.WriteLine($"Adding {e.PlayerInfo.Name} {e.PlayerInfo.Level} {e.PlayerInfo.GuildName} {e.PlayerInfo.PlayerClass}");
                }
            }

        }

        private void LogParser_PlayerZonedEvent(object sender, LogParser.PlayerZonedEventArgs e)
        {
            if (CurrentZone != e.Zone)
            {
                Debug.WriteLine("Clearing zone Players");
                lock (ContainerLock)
                {
                    PlayerZones.Clear();
                }
            }
        }
    }
}
