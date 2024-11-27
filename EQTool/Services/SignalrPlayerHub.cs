using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using EQToolShared.Enums;
using EQToolShared.HubModels;
using EQToolShared.Map;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Media3D;

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
        private readonly LogEvents logEvents;
        private readonly IAppDispatcher appDispatcher;
        private readonly System.Timers.Timer timer;
        private SignalrPlayer LastPlayer;
        private readonly EQSpells spells;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly ClientWebSocket NParseWebsocketConnection;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public SignalrPlayerHub(EQSpells spells, LogEvents logEvents, IAppDispatcher appDispatcher, LogParser logParser, ActivePlayer activePlayer, SpellWindowViewModel spellWindowViewModel)
        {
            this.spells = spells;
            this.logEvents = logEvents;
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.logParser = logParser;
            this.spellWindowViewModel = spellWindowViewModel;
            var url = "https://www.pigparse.org/EqToolMap";
            connection = new HubConnectionBuilder()
              .WithUrl(url)
              .WithAutomaticReconnect()
              .Build();
            _ = connection.On("PlayerLocationEvent", (SignalrPlayer p) =>
                {
                    PushPlayerLocationEvent(p);
                });
            _ = connection.On("PlayerDisconnected", (SignalrPlayer p) =>
            {
                PushPlayerDisconnected(p);
            });
            _ = connection.On("AddCustomTrigger", (SignalrCustomTimer p) =>
            {
                AddCustomTrigger(p);
            }); 
            connection.Closed += async (error) =>
              {
                  await Task.Delay(new Random().Next(0, 5) * 1000);
                  await SignalrConnectWithRetry();
              };
            NParseWebsocketConnection = new ClientWebSocket();
            try
            {
                _ = Task.Factory.StartNew(async () =>
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
                _ = Task.Factory.StartNew(async () =>
                {
                    await SignalrConnectWithRetry();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            this.logEvents.PlayerLocationEvent += LogParser_PlayerLocationEvent;
            this.logEvents.CampEvent += LogParser_CampEvent;
            this.logEvents.DragonRoarEvent += LogEvents_DragonRoarEvent;
            timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1000 * 10;
            timer.Start();
        }

        private void InvokeAsync<T>(string name, T obj)
        {
            if (connection.State == HubConnectionState.Connected && obj != null)
            {
                _ = connection.InvokeAsync(name, obj);
            }
        }

        private void LogParser_CampEvent(object sender, CampEvent e)
        {
            LastPlayer = null;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (LastPlayer != null && activePlayer?.Player?.Server != null)
            {
                if ((DateTime.UtcNow - logParser.LastYouActivity).TotalMinutes > 5)
                {
                    LastPlayer = null;
                }
                else
                {
                    InvokeAsync("PlayerLocationEvent", LastPlayer);
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
                    try
                    {
                        InvokeAsync("JoinServerGroup", activePlayer?.Player?.Server);
                    }
                    catch { }
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
                    if (NParseWebsocketConnection.State == WebSocketState.Closed ||
                        NParseWebsocketConnection.State == WebSocketState.CloseReceived ||
                        NParseWebsocketConnection.State == WebSocketState.None)
                    {
                        Debug.WriteLine("Beg Nparse StartAsync");
                        await NParseWebsocketConnection.ConnectAsync(new Uri("ws://sheeplauncher.net:8424"), cancellationTokenSource.Token);
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
                Thread.Sleep(100);
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
                        //Console.WriteLine("Received message: " + msg);
                        if (string.IsNullOrWhiteSpace(msg))
                        {
                            continue;
                        }

                        var test = Newtonsoft.Json.JsonConvert.DeserializeObject<NParseStateData>(msg);
                        var playername = activePlayer?.Player?.Name;
                        var playerzone = activePlayer?.Player?.Zone;
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
                else if (NParseWebsocketConnection.State != WebSocketState.Connecting)
                {
                    Debug.WriteLine($"WebSocket {NParseWebsocketConnection.State} Reconnecting");
                    throw new Exception();
                }
            }
        }

        public void PushPlayerDisconnected(SignalrPlayer p)
        {
            if (!(p.Server == activePlayer?.Player?.Server && p.Name == activePlayer?.Player?.Name))
            {
                Debug.WriteLine($"PlayerDisconnected {p.Name}");
                appDispatcher.DispatchUI(() =>
                {
                    PlayerDisconnected?.Invoke(this, p);
                });
            }
        }

        private void LogEvents_DragonRoarEvent(object sender, DragonRoarEvent e)
        {
            if (!string.IsNullOrWhiteSpace(activePlayer?.Player?.Zone) &&
                activePlayer.Player.SpellDebuffShare && 
                LastPlayer.X != 0 && LastPlayer.Y != 0 && LastPlayer.Z != 0 &&
                activePlayer.Player.Server.HasValue)
            {
                InvokeAsync("DragonRoarEvent", new SignalRDragonRoar
                {
                    SpellName = e.Spell.name,
                     
                });
            }
        }

        public void AddCustomTrigger(SignalrCustomTimer p)
        {
            if (p.Server == activePlayer?.Player?.Server)
            {
                Debug.WriteLine($"AddCustomTrigger {p.Name}");
                appDispatcher.DispatchUI(() =>
                {
                    var s = Create(p);
                    var existingtimer = spellWindowViewModel.SpellList.FirstOrDefault(x => x.Name == s.Name && x.GroupName == s.GroupName);
                    if (existingtimer != null)
                    {
                        _ = spellWindowViewModel.SpellList.Remove(existingtimer);
                    }
                    spellWindowViewModel.TryAdd(s);
                });
            }
        }

        private TimerViewModel Create(HubCustomTimer e)
        {
            var spellicon = spells.AllSpells.FirstOrDefault(a => a.name == e.SpellNameIcon);
            return new TimerViewModel
            {
                PercentLeft = 100,
                GroupName = CustomTimer.CustomerTime,
                Name = e.Name,
                Rect = spellicon.Rect,
                Icon = spellicon.SpellIcon,
                TotalDuration = TimeSpan.FromSeconds(e.DurationInSeconds),
                TotalRemainingDuration = TimeSpan.FromSeconds(e.DurationInSeconds),
                UpdatedDateTime = DateTime.Now
            };
        }
         
        public void PushPlayerLocationEvent(SignalrPlayer p)
        {
            if (!(p.Server == activePlayer?.Player?.Server && p.Name == activePlayer?.Player?.Name))
            {
                Debug.WriteLine($"PlayerLocationEvent {p.Name}");
                appDispatcher.DispatchUI(() =>
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
                foreach (var item in Zones.ZoneNameMapper)
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

        private void LogParser_PlayerLocationEvent(object sender, PlayerLocationEvent e)
        {
            if (activePlayer?.Player?.Server != null)
            {
                LastPlayer = new SignalrPlayer
                {
                    Zone = activePlayer.Player.Zone,
                    GuildName = activePlayer.Player.GuildName, 
                    Server = activePlayer.Player.Server.Value,
                    MapLocationSharing = activePlayer.Player.MapLocationSharing,
                    Name = activePlayer.Player.Name,
                    TrackingDistance = activePlayer.Player.TrackingDistance,
                    X = e.Location.X,
                    Y = e.Location.Y,
                    Z = e.Location.Z
                };

                InvokeAsync("PlayerLocationEvent", LastPlayer);

                if (NParseWebsocketConnection.State == WebSocketState.Open && activePlayer?.Player?.MapLocationSharing == MapLocationSharing.Everyone)
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
                    _ = Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            await NParseWebsocketConnection.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        catch { }
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
