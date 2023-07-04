using EQTool.ViewModels;
using EQToolShared.APIModels.ZoneControllerModels;
using System;
using System.Diagnostics;
using System.Windows.Media.Media3D;

namespace EQTool.Services
{
    public class ZoneActivityTrackingService
    {
        private readonly LogParser logParser;
        private readonly PigParseApi pigParseApi;
        private readonly ActivePlayer activePlayer;
        private readonly LoggingService loggingService;
        private Point3D? LastLocation = null;

        public ZoneActivityTrackingService(LogParser logParser, ActivePlayer activePlayer, PigParseApi pigParseApi, LoggingService loggingService)
        {
            _ = activePlayer.Update();
            this.logParser = logParser;
            this.logParser.DeadEvent += LogParser_DeadEvent;
            this.logParser.ConEvent += LogParser_ConEvent; ;
            this.logParser.PlayerLocationEvent += LogParser_PlayerLocationEvent;
            this.pigParseApi = pigParseApi;
            this.activePlayer = activePlayer;
            this.loggingService = loggingService;
        }

        private void LogParser_ConEvent(object sender, LogParser.ConEventArgs e)
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
                loggingService.Log(ex.ToString(), App.EventType.Error);
            }
        }

        private void LogParser_PlayerLocationEvent(object sender, LogParser.PlayerLocationEventArgs e)
        {
            LastLocation = e.Location;
        }

        private void LogParser_DeadEvent(object sender, LogParser.DeadEventArgs e)
        {
            if (activePlayer.Player?.Server == null)
            {
                return;
            }

            try
            {
                Debug.WriteLine($"Zone activity death: {e.Name}");
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
                loggingService.Log(ex.ToString(), App.EventType.Error);
            }
        }
    }
}
