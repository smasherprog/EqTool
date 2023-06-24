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
        internal readonly ActivePlayer activePlayer;
        private readonly LoggingService loggingService;
        private readonly PlayerGroupService playerGroupService;
        private readonly Dictionary<string, PlayerWhoLogParse.PlayerInfo> Player = new Dictionary<string, PlayerWhoLogParse.PlayerInfo>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, PlayerWhoLogParse.PlayerInfo> PlayerZones = new Dictionary<string, PlayerWhoLogParse.PlayerInfo>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, PlayerWhoLogParse.PlayerInfo> DirtyPlayers = new Dictionary<string, PlayerWhoLogParse.PlayerInfo>(StringComparer.InvariantCultureIgnoreCase);
        private string CurrentZone;
        private readonly System.Timers.Timer UITimer;
        private readonly object ContainerLock = new object();

        public PlayerTrackerService(LogParser logParser, ActivePlayer activePlayer, PigParseApi pigParseApi, LoggingService loggingService, PlayerGroupService playerGroupService)
        {
            _ = activePlayer.Update();
            CurrentZone = activePlayer.Player?.Zone;
            this.logParser = logParser;
            this.playerGroupService = playerGroupService;
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
            if (activePlayer.Player?.Server == null)
            {
                return;
            }

            lock (ContainerLock)
            {
                playerstosync = DirtyPlayers.Values.ToList();
                DirtyPlayers.Clear();
            }
            try
            {
                pigParseApi.SendPlayerData(playerstosync, activePlayer.Player.Server);
            }
            catch (Exception ex)
            {
                loggingService.Log(ex.ToString(), App.EventType.Error);
            }
        }

        private void LogParser_WhoEvent(object sender, LogParser.WhoEventArgs e)
        {
            lock (ContainerLock)
            {
                if (CurrentZone != activePlayer.Player?.Zone)
                {
                    CurrentZone = activePlayer.Player?.Zone;
                    Debug.WriteLine("Clearing zone Players");
                    PlayerZones.Clear();
                }
                else
                {
                    Debug.WriteLine("NOT Clearing zone Players");
                }
            }
        }

        private void LogParser_WhoPlayerEvent(object sender, LogParser.WhoPlayerEventArgs e)
        {
            if (activePlayer.Player != null && e.PlayerInfo.Name == activePlayer.Player.Name && !string.IsNullOrWhiteSpace(e.PlayerInfo.GuildName))
            {
                activePlayer.Player.GuildName = e.PlayerInfo.GuildName;
            }

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
                    if (!DirtyPlayers.ContainsKey(e.PlayerInfo.Name))
                    {
                        DirtyPlayers[e.PlayerInfo.Name] = e.PlayerInfo;
                    }
                    Debug.WriteLine($"Adding {e.PlayerInfo.Name} {e.PlayerInfo.Level} {e.PlayerInfo.GuildName} {e.PlayerInfo.PlayerClass}");
                }

                if (!PlayerZones.ContainsKey(e.PlayerInfo.Name))
                {
                    PlayerZones[e.PlayerInfo.Name] = e.PlayerInfo;
                }
            }

        }

        private void LogParser_PlayerZonedEvent(object sender, LogParser.PlayerZonedEventArgs e)
        {
            if (CurrentZone != e.Zone)
            {
                if (CurrentZone != activePlayer.Player?.Zone)
                {
                    CurrentZone = activePlayer.Player?.Zone;
                    Debug.WriteLine("Clearing zone Players");
                    PlayerZones.Clear();
                }
                else
                {
                    Debug.WriteLine("NOT Clearing zone Players");
                }
            }
        }

        public List<Group> CreateGroups(GroupOptimization groupOptimization)
        {
            if (string.IsNullOrWhiteSpace(activePlayer.Player?.GuildName))
            {
                return new List<Group>();
            }

            var players = new List<PlayerWhoLogParse.PlayerInfo>();
            lock (ContainerLock)
            {
                players = PlayerZones.Values.ToList();
            }

            var uknownplayerdata = players.Where(a => !a.PlayerClass.HasValue || !a.Level.HasValue).Select(a => a.Name).ToList();
            var playerdatafromserver = pigParseApi.GetPlayerData(uknownplayerdata, activePlayer.Player.Server);
            foreach (var item in playerdatafromserver)
            {
                var playerlocally = players.FirstOrDefault(a => a.Name == item.Name);
                if (playerlocally != null)
                {
                    playerlocally.Level = playerlocally.Level ?? item.Level;
                    playerlocally.PlayerClass = playerlocally.PlayerClass ?? item.PlayerClass;
                }
            }

            players = players.Where(a => a.GuildName == activePlayer.Player.GuildName).ToList();
            switch (groupOptimization)
            {
                case GroupOptimization.HOT_Cleric_SparseGroup:
                    return playerGroupService.CreateHOT_Clerics_SparseGroups(players);
                case GroupOptimization.HOT_Cleric_SameGroup:
                    return playerGroupService.CreateHOT_Clerics_SameGroups(players);
                case GroupOptimization.Standard:
                    return playerGroupService.CreateStandardGroups(players);
                default:
                    return new List<Group>();
            }
        }

    }
}
