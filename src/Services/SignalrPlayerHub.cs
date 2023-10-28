using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared.Map;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EQTool.Models
{

    public interface ISignalrPlayerHub
    {
    }

    public class SignalrPlayerHub : ISignalrPlayerHub
    {
        private readonly HubConnection connection;
        private readonly ActivePlayer activePlayer;
        private readonly LogParser logParser;

        public SignalrPlayerHub(ActivePlayer activePlayer, LogParser logParser)
        {
            this.activePlayer = activePlayer;
            this.logParser = logParser;
            var url = "https://localhost:7056/EqToolMap";
#if !DEBUG
            url ="https://www.pigparse.org/EqToolMap";
 
#endif
            connection = new HubConnectionBuilder()
              .WithUrl(url)
              .WithAutomaticReconnect()
              .Build();
            connection.On("PlayerLocationEvent", (SignalrPlayer p) =>
            {
                Debug.WriteLine("PlayerLocationEvent Receivec");
            });
            connection.On("PlayerDisconnected", (SignalrPlayer p) =>
            {
                Debug.WriteLine("PlayerDisconnected Receivec");
            });

            connection.Closed += async (error) =>
              {
                  await Task.Delay(new Random().Next(0, 5) * 1000);
                  ConnectWithRetryAsync(connection).Wait();
              };
            try
            {
                Task.Factory.StartNew(() =>
                {
                    ConnectWithRetryAsync(connection).Wait();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            this.logParser.PlayerLocationEvent += LogParser_PlayerLocationEvent;
        }

        private static async Task<bool> ConnectWithRetryAsync(HubConnection connection)
        {
            while (true)
            {
                try
                {
                    Debug.WriteLine("Beg StartAsync");
                    await connection.StartAsync();
                    Debug.WriteLine("Connected StartAsync");
                    return true;
                }
                catch
                {
                    Debug.WriteLine("Failed StartAsync");
                    await Task.Delay(5000);
                }
            }
        }

        private void LogParser_PlayerLocationEvent(object sender, LogParser.PlayerLocationEventArgs e)
        {
            if (this.activePlayer?.Player?.Server != null)
            {
                connection.InvokeAsync("PlayerLocationEvent", new SignalrPlayer
                {
                    Zone = this.activePlayer.Player.Zone,
                    GuildName = this.activePlayer.Player.GuildName,
                    PlayerClass = this.activePlayer.Player.PlayerClass,
                    Server = this.activePlayer.Player.Server.Value,
                    MapLocationSharing = this.activePlayer.Player.MapLocationSharing.Value,
                    Name = this.activePlayer.Player.Name,
                    X = e.Location.X,
                    Y = e.Location.Y,
                    Z = e.Location.Z
                });
            }
        }
    }
}
