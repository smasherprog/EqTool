using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared.APIModels.ZoneControllerModels;
using EQToolShared.Enums;
using System;
using System.Diagnostics;
using System.Windows.Media.Media3D;

namespace EQTool.Services
{
    public class ZoneActivityTrackingService
    {
        private readonly LogEvents logParser;
        private readonly PigParseApi pigParseApi;
        private readonly ActivePlayer activePlayer;
        private readonly LoggingService loggingService;
        private Point3D? LastLocation = null;

        public ZoneActivityTrackingService(LogEvents logParser, ActivePlayer activePlayer, PigParseApi pigParseApi, LoggingService loggingService)
        {
            _ = activePlayer.Update();
            this.logParser = logParser;
            this.logParser.DeathEvent += LogParser_DeathEvent;
            this.logParser.ConEvent += LogParser_ConEvent; ;
            this.logParser.PlayerLocationEvent += LogParser_PlayerLocationEvent;
            this.pigParseApi = pigParseApi;
            this.activePlayer = activePlayer;
            this.loggingService = loggingService;
        }

        private void LogParser_ConEvent(object sender, ConEvent e)
        {
            if (activePlayer.Player?.Server == null)
            {
                return;
            }

            try
            {
                Debug.WriteLine($"Zone activity seen: {e.Name}");
                pigParseApi.SendNPCActivity(new NPCActivityRequest
                {
                    NPCData = new NPCData
                    {
                        LocX = LastLocation.HasValue ? LastLocation.Value.X : (double?)null,
                        LocY = LastLocation.HasValue ? LastLocation.Value.Y : (double?)null,
                        Zone = activePlayer.Player.Zone,
                        Name = e.Name
                    },
                    Server = activePlayer.Player.Server.Value,
                    IsDeath = false
                });
            }
            catch (Exception ex)
            {
                loggingService.Log(ex.ToString(), EventType.Error, activePlayer?.Player?.Server);
            }
        }

        private void LogParser_PlayerLocationEvent(object sender, PlayerLocationEvent e)
        {
            LastLocation = e.Location;
        }

        private void LogParser_DeathEvent(object sender, DeathEvent e)
        {
            if (activePlayer.Player?.Server == null)
            {
                return;
            }

            try
            {
                Debug.WriteLine($"Zone activity death: {e.Victim}");
                pigParseApi.SendNPCActivity(new NPCActivityRequest
                {
                    NPCData = new NPCData
                    {
                        LocX = LastLocation.HasValue ? LastLocation.Value.X : (double?)null,
                        LocY = LastLocation.HasValue ? LastLocation.Value.Y : (double?)null,
                        Zone = activePlayer.Player.Zone,
                        Name = e.Victim
                    },
                    Server = activePlayer.Player.Server.Value,
                    IsDeath = true
                });
            }
            catch (Exception ex)
            {
                loggingService.Log(ex.ToString(), EventType.Error, activePlayer?.Player?.Server);
            }
        }
    }
}
