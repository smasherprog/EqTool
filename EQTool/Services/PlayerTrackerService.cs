using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EQTool.Services
{
    public class PlayerTrackerService
    {
        private readonly LogEvents logEvents;
        private readonly PigParseApi pigParseApi;
        private readonly ActivePlayer activePlayer;
        private readonly LoggingService loggingService;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly IAppDispatcher appDispatcher;

        private readonly Dictionary<string, EQToolShared.APIModels.PlayerControllerModels.Player> AllPlayers = new Dictionary<string, EQToolShared.APIModels.PlayerControllerModels.Player>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, EQToolShared.APIModels.PlayerControllerModels.Player> PlayersInZones = new Dictionary<string, EQToolShared.APIModels.PlayerControllerModels.Player>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, EQToolShared.APIModels.PlayerControllerModels.Player> DirtyPlayers = new Dictionary<string, EQToolShared.APIModels.PlayerControllerModels.Player>(StringComparer.InvariantCultureIgnoreCase);
        private string CurrentZone;
        private readonly System.Timers.Timer UITimer;
        private readonly object ContainerLock = new object();

        public PlayerTrackerService(IAppDispatcher appDispatcher, LogEvents logEvents, ActivePlayer activePlayer, PigParseApi pigParseApi, LoggingService loggingService, SpellWindowViewModel spellWindowViewModel)
        { 
            CurrentZone = activePlayer.Player?.Zone;
            this.logEvents = logEvents;
            this.logEvents.YouZonedEvent += LogParser_PlayerZonedEvent;
            this.logEvents.AfterPlayerChangedEvent += LogEvents_AfterPayerChangedEvent;
            this.logEvents.WhoPlayerEvent += LogParser_WhoPlayerEvent;
            UITimer = new System.Timers.Timer(20000);// every 20 seconds
            UITimer.Elapsed += UITimer_Elapsed;
            UITimer.Enabled = true;
            this.pigParseApi = pigParseApi;
            this.activePlayer = activePlayer;
            this.loggingService = loggingService;
            this.spellWindowViewModel = spellWindowViewModel;
            this.appDispatcher = appDispatcher;
        }

        private void LogEvents_AfterPayerChangedEvent(object sender, AfterPlayerChangedEvent e)
        {
            CurrentZone = activePlayer.Player?.Zone;
        }

        public bool IsPlayer(string name)
        {
            if (name == EQSpells.You || name == EQSpells.SpaceYou)
            {
                return true;
            }

            lock (ContainerLock)
            {
                return AllPlayers.ContainsKey(name);
            }
        }

        public EQToolShared.APIModels.PlayerControllerModels.Player GetPlayer(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith(" "))
            {
                return null;
            }
            lock (ContainerLock)
            {
                if (AllPlayers.TryGetValue(name, out var p))
                {
                    return p;
                }
                return null;
            }
        }

        private void UITimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var playerstosync = new List<EQToolShared.APIModels.PlayerControllerModels.Player>();
            if (activePlayer.Player?.Server == null)
            {
                return;
            }

            var playerswithoutclasses = new List<EQToolShared.APIModels.PlayerControllerModels.Player>();

            lock (ContainerLock)
            {
                playerstosync = DirtyPlayers.Values.ToList();
                DirtyPlayers.Clear();
                playerswithoutclasses = AllPlayers.Values.Where(a => !a.PlayerClass.HasValue).ToList();
            }
            try
            {
                pigParseApi.SendPlayerData(playerstosync, activePlayer.Player.Server.Value);
            }
            catch (Exception ex)
            {
                loggingService.Log(ex.ToString(), EventType.Error, activePlayer?.Player?.Server);
            }
            try
            {
                var players = pigParseApi.GetPlayerData(playerswithoutclasses.Select(a => a.Name).Distinct().ToList(), activePlayer.Player.Server.Value);
                lock (ContainerLock)
                {
                    foreach (var item in players)
                    {
                        if (AllPlayers.TryGetValue(item.Name, out var player))
                        {
                            player.PlayerClass = item.PlayerClass;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService.Log(ex.ToString(), EventType.Error, activePlayer?.Player?.Server);
            }
            appDispatcher.DispatchUI(() =>
            {
                var spellsmissingclasses = spellWindowViewModel.SpellList
                .Where(a => !a.GroupName.StartsWith(" ") && a.SpellViewModelType == ViewModels.SpellWindow.SpellViewModelType.Spell)
                .Cast<SpellViewModel>()
                .ToList();
                var missingplayernames = spellsmissingclasses.Select(a => a.GroupName).Distinct().ToList();
                if (missingplayernames.Any())
                {
                    _ = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var playersapi = pigParseApi.GetPlayerData(missingplayernames, activePlayer.Player.Server.Value);
                            lock (ContainerLock)
                            {
                                foreach (var item in playersapi)
                                {
                                    if (AllPlayers.TryGetValue(item.Name, out var player))
                                    {
                                        player.PlayerClass = item.PlayerClass;
                                    }
                                    else
                                    {
                                        AllPlayers.Add(item.Name, item);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            loggingService.Log(ex.ToString(), EventType.Error, activePlayer?.Player?.Server);
                        }
                    });
                }
                var players = new List<EQToolShared.APIModels.PlayerControllerModels.Player>();
                lock (ContainerLock)
                {
                    players = AllPlayers.Values.Where(a => missingplayernames.Contains(a.Name)).ToList();
                }
                foreach (var item in players)
                {
                    foreach (var missingclass in spellsmissingclasses.Where(a => a.GroupName == item.Name))
                    {
                        missingclass.TargetClass = item.PlayerClass;
                    }
                }
            });
        }

        private void LogParser_WhoPlayerEvent(object sender, WhoPlayerEvent e)
        {
            if (activePlayer.Player != null && e.PlayerInfo.Name == activePlayer.Player.Name && !string.IsNullOrWhiteSpace(e.PlayerInfo.GuildName))
            {
                activePlayer.Player.GuildName = e.PlayerInfo.GuildName;
            }

            lock (ContainerLock)
            {
                if (AllPlayers.TryGetValue(e.PlayerInfo.Name, out var playerinfo))
                {
                    if (
                        (playerinfo.Level != e.PlayerInfo.Level && e.PlayerInfo.Level.HasValue) ||
                        (playerinfo.GuildName != e.PlayerInfo.GuildName && !string.IsNullOrWhiteSpace(e.PlayerInfo.GuildName)) ||
                        (playerinfo.PlayerClass != e.PlayerInfo.PlayerClass && e.PlayerInfo.PlayerClass.HasValue)
                        )
                    {
                        if (!DirtyPlayers.ContainsKey(e.PlayerInfo.Name))
                        {
                            Debug.WriteLine($"DirtyPlayer Add {e.PlayerInfo.Name}");
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
                    AllPlayers.Add(e.PlayerInfo.Name, e.PlayerInfo);
                    if (!DirtyPlayers.ContainsKey(e.PlayerInfo.Name))
                    {
                        DirtyPlayers[e.PlayerInfo.Name] = e.PlayerInfo;
                        Debug.WriteLine($"DirtyPlayer Add {e.PlayerInfo.Name}");
                    }
                    Debug.WriteLine($"Adding {e.PlayerInfo.Name} {e.PlayerInfo.Level} {e.PlayerInfo.GuildName} {e.PlayerInfo.PlayerClass}");
                }

                if (!PlayersInZones.ContainsKey(e.PlayerInfo.Name))
                {
                    PlayersInZones[e.PlayerInfo.Name] = e.PlayerInfo;
                }
            }

        }

        private void LogParser_PlayerZonedEvent(object sender, YouZonedEvent e)
        {
            lock (ContainerLock)
            {
                if (CurrentZone != activePlayer.Player?.Zone)
                {
                    CurrentZone = activePlayer.Player?.Zone;
                    Debug.WriteLine("Clearing zone Players");
                    PlayersInZones.Clear();
                }
            }
        }
    }
}
