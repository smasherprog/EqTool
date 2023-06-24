using EQTool.ViewModels;
using EQToolShared.HubModels;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EQTool.Services.Map
{
    public class SignalRMapService
    {
        private HubConnection HubConnection;
        public event Action<PlayerLocation> PlayerLocationReceived;
        private readonly LogParser logParser;
        private readonly ActivePlayer activePlayer;

        public SignalRMapService(LogParser logParser, ActivePlayer activePlayer)
        {
            this.logParser = logParser;
            this.activePlayer = activePlayer;
            this.logParser.PlayerLocationEvent += LogParser_PlayerLocationEventAsync;
        }

        private void LogParser_PlayerLocationEventAsync(object sender, LogParser.PlayerLocationEventArgs e)
        {
            Debug.WriteLine("SendPlayerLocation");
            _ = HubConnection.SendAsync("SendPlayerLocation", new PlayerLocation
            {
                PlayerName = activePlayer.Player.Name,
                ZoneName = e.PlayerInfo.Zone,
                Server = e.PlayerInfo.Server,
                X = e.Location.X,
                Y = e.Location.Y,
                Z = e.Location.Z
            });
        }

        public async Task Connect()
        {
            if (HubConnection?.State == HubConnectionState.Connected)
            {
                return;
            }
            _ = (HubConnection?.DisposeAsync());
            try
            {
                HubConnection = new HubConnectionBuilder()
                    .WithUrl("https://pigparse.org/EqToolMap")
                    .Build();
                _ = HubConnection.On<PlayerLocation>("ReceivePlayerLocation", (playerLocation) => PlayerLocationReceived?.Invoke(playerLocation));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            HubConnection.Closed += async (error) =>
            {
                Debug.WriteLine("Signalr Connection Closed. Retrying");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await HubConnection.StartAsync();
            };

            await HubConnection.StartAsync().ContinueWith(t => { Debug.WriteLine("Signalr Connected!"); });
        }
    }
}
