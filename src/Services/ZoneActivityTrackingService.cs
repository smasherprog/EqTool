using EQTool.ViewModels;
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
            this.logParser.PlayerLocationEvent += LogParser_PlayerLocationEvent;
            this.pigParseApi = pigParseApi;
            this.activePlayer = activePlayer;
            this.loggingService = loggingService;
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
                pigParseApi.SendDeath(new PigParseApi.DeathData
                {
                    LocX = LastLocation.HasValue ? LastLocation.Value.X : (double?)null,
                    LocY = LastLocation.HasValue ? LastLocation.Value.Y : (double?)null,
                    Zone = activePlayer.Player.Zone,
                    Name = e.Name
                }, activePlayer.Player.Server);
            }
            catch (Exception ex)
            {
                loggingService.Log(ex.ToString(), App.EventType.Error);
            }
        }
    }
}
