using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared.Enums;
using EQToolShared.HubModels;
using EQToolShared.Map;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace EQTool.Models
{

    public interface ISignalrPlayerHub : IDisposable
    {
        event EventHandler<SignalrPlayer> PlayerLocationEvent;
        event EventHandler<SignalrPlayer> PlayerDisconnected;
        void PushPlayerLocationEvent(SignalrPlayer player);
        void PushPlayerDisconnected(SignalrPlayer player);
    }

    public class SignalrPlayerHub : ISignalrPlayerHub
    {
        private readonly HubConnection connection;
        private readonly ActivePlayer activePlayer;
        private readonly LogParser logParser;
        private readonly IAppDispatcher appDispatcher;
        private readonly System.Timers.Timer timer;
        private SignalrPlayer LastPlayer;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private Servers? LastServer;
        private ClientWebSocket NParseWebsocketConnection;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public SignalrPlayerHub(IAppDispatcher appDispatcher, LogParser logParser, ActivePlayer activePlayer, SpellWindowViewModel spellWindowViewModel)
        {
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.logParser = logParser;
            this.spellWindowViewModel = spellWindowViewModel;
            var url = "https://www.pigparse.org/EqToolMap";
            connection = new HubConnectionBuilder()
              .WithUrl(url)
              .WithAutomaticReconnect()
              .Build();
            connection.On("PlayerLocationEvent", (SignalrPlayer p) =>
                {
                    this.PushPlayerLocationEvent(p);
                });
            connection.On("PlayerDisconnected", (SignalrPlayer p) =>
            {
                this.PushPlayerDisconnected(p);
            });
            connection.On("AddCustomTrigger", (SignalrCustomTimer p) =>
            {
                this.AddCustomTrigger(p);
            });
            connection.Closed += async (error) =>
              {
                  await Task.Delay(new Random().Next(0, 5) * 1000);
                  await SignalrConnectWithRetry();
              };
            NParseWebsocketConnection = new ClientWebSocket();
            try
            {
                Task.Factory.StartNew(async () =>
                {
                    await NparseConnectWithRetry();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            try
            {
                Task.Factory.StartNew(async () =>
                {
                    await SignalrConnectWithRetry();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            this.logParser.PlayerLocationEvent += LogParser_PlayerLocationEvent;
            this.logParser.CampEvent += LogParser_CampEvent;
            timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1000 * 10;
            timer.Start();
        }

        private void InvokeAsync<T>(string name, T obj)
        {
            if (connection.State == HubConnectionState.Connected)
            {
                connection.InvokeAsync(name, obj);
            }
        }

        private void LogParser_CampEvent(object sender, LogParser.CampEventArgs e)
        {
            this.LastPlayer = null;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.LastPlayer != null && connection?.State == HubConnectionState.Connected && this.activePlayer?.Player?.Server != null)
            {
                if ((DateTime.UtcNow - this.logParser.LastYouActivity).TotalMinutes > 5)
                {
                    this.LastPlayer = null;
                }
                else
                {
                    InvokeAsync("PlayerLocationEvent", this.LastPlayer);
                }
            }
        }

        public event EventHandler<SignalrPlayer> PlayerLocationEvent;
        public event EventHandler<SignalrPlayer> PlayerDisconnected;

        private async Task SignalrConnectWithRetry()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    Debug.WriteLine("Beg StartAsync");
                    await connection.StartAsync();
                    Debug.WriteLine("Connected StartAsync");
                    return;
                }
                catch
                {
                    Debug.WriteLine("Failed StartAsync");
                    await Task.Delay(5000);
                }
            }
        }

        private async Task NparseConnectWithRetry()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (NParseWebsocketConnection.State == WebSocketState.Closed || NParseWebsocketConnection.State == WebSocketState.None)
                    {
                        Debug.WriteLine("Beg Nparse StartAsync");
                        await this.NParseWebsocketConnection.ConnectAsync(new Uri("ws://sheeplauncher.net:8424"), cancellationTokenSource.Token);
                        Debug.WriteLine("Connected Nparse StartAsync");
                    }

                    await NparseStartReceiveLoopAsync();
                }
                catch
                {
                    Debug.WriteLine("Failed Nparse StartAsync");
                    if (!cancellationTokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(5000);
                    }
                }
            }
        }
        private async Task NparseStartReceiveLoopAsync()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                if (NParseWebsocketConnection.State == WebSocketState.Open)
                {
                    try
                    {
                        var buffer = new ArraySegment<byte>(new byte[1024]);
                        var msg = string.Empty;
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await NParseWebsocketConnection.ReceiveAsync(buffer, cancellationTokenSource.Token);
                            msg += Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        }
                        while (!result.EndOfMessage);
                        Console.WriteLine("Received message: " + msg);

                        var test = Newtonsoft.Json.JsonConvert.DeserializeObject<NParseStateData>(msg);
                        var playername = this.activePlayer?.Player?.Name;
                        var playerzone = this.activePlayer?.Player?.Zone;
                        if (!string.IsNullOrWhiteSpace(playername) && !string.IsNullOrWhiteSpace(playerzone))
                        {
                            var nparsezonename = TranslateZoneNameToNParse(playerzone);
                            foreach (var item in test.locations)
                            {
                                if (item.Key.Equals(nparsezonename, StringComparison.OrdinalIgnoreCase))
                                {
                                    foreach (var player in item.Value)
                                    {
                                        if (player.Key != playername && player.Key.IndexOf(" (PP)", StringComparison.OrdinalIgnoreCase) == -1)
                                        {
                                            PushPlayerLocationEvent(new SignalrPlayer
                                            {
                                                GuildName = string.Empty,
                                                MapLocationSharing = MapLocationSharing.Everyone,
                                                Name = player.Key + " (NP)",
                                                Server = Servers.Green,
                                                X = player.Value.x,
                                                Y = player.Value.y,
                                                Z = player.Value.z,
                                                Zone = playerzone
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
                else if (NParseWebsocketConnection.State == WebSocketState.Closed)
                {
                    throw new Exception();
                }
            }
        }
        public void PushPlayerDisconnected(SignalrPlayer p)
        {
            if (!(p.Server == this.activePlayer?.Player?.Server && p.Name == this.activePlayer?.Player?.Name))
            {
                Debug.WriteLine($"PlayerDisconnected {p.Name}");
                this.appDispatcher.DispatchUI(() =>
                {
                    PlayerDisconnected?.Invoke(this, p);
                });
            }
        }

        public void AddCustomTrigger(SignalrCustomTimer p)
        {
            if (p.Server == this.activePlayer?.Player?.Server)
            {
                Debug.WriteLine($"AddCustomTrigger {p.Name}");
                this.appDispatcher.DispatchUI(() =>
                {
                    this.spellWindowViewModel.TryAddCustom(p);
                });
            }
        }

        public void PushPlayerLocationEvent(SignalrPlayer p)
        {
            if (!(p.Server == this.activePlayer?.Player?.Server && p.Name == this.activePlayer?.Player?.Name))
            {
                Debug.WriteLine($"PlayerLocationEvent {p.Name}");
                this.appDispatcher.DispatchUI(() =>
                {
                    PlayerLocationEvent?.Invoke(this, p);
                });
            }
        }
        private string TranslateZoneNameToNParse(string zoneName)
        {
            if (zoneName == "cazicthule")
            {
                return "lost temple of cazic-thule";
            }
            else if (zoneName == "neriakb")
            {
                return "neriak commons";
            }
            else
            {
                foreach (var item in ZoneParser.ZoneNameMapper)
                {
                    if (string.Equals(item.Value, zoneName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (item.Value.Contains("("))
                        {
                            continue;
                        }
                        return item.Key;
                    }
                }
            }
            return zoneName;
        }
        private void LogParser_PlayerLocationEvent(object sender, LogParser.PlayerLocationEventArgs e)
        {
            if (this.activePlayer?.Player?.Server != null)
            {
                this.LastPlayer = new SignalrPlayer
                {
                    Zone = this.activePlayer.Player.Zone,
                    GuildName = this.activePlayer.Player.GuildName,
                    PlayerClass = this.activePlayer.Player.PlayerClass,
                    Server = this.activePlayer.Player.Server.Value,
                    MapLocationSharing = this.activePlayer.Player.MapLocationSharing.Value,
                    Name = this.activePlayer.Player.Name,
                    TrackingDistance = this.activePlayer.Player.TrackingDistance,
                    X = e.Location.X,
                    Y = e.Location.Y,
                    Z = e.Location.Z
                };

                InvokeAsync("PlayerLocationEvent", this.LastPlayer);

                if (this.NParseWebsocketConnection.State == WebSocketState.Open && this.activePlayer?.Player?.MapLocationSharing == MapLocationSharing.Everyone)
                {
                    var nparsezonename = TranslateZoneNameToNParse(LastPlayer.Zone);
                    var sendMessage = Newtonsoft.Json.JsonConvert.SerializeObject(new NParseLocationEvent
                    {
                        group_key = "public",
                        type = "location",
                        location = new NParseLocation
                        {
                            x = Math.Round(LastPlayer.X, 2),
                            y = Math.Round(LastPlayer.Y, 2),
                            z = Math.Round(LastPlayer.Z, 2),
                            zone = LastPlayer.Zone,
                            player = LastPlayer.Name + " (PP)",
                        }
                    });
                    var sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(sendMessage));
                    Task.Factory.StartNew(async () =>
                    {
                        await this.NParseWebsocketConnection.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    });
                }
            }
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
