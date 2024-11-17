using EQTool.Services;
using EQTool.Services.Factories;
using EQTool.ViewModels;
using EQToolShared;
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
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly ClientWebSocket NParseWebsocketConnection;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly SpellWindowViewModelFactory spellWindowViewModelFactory;

        public SignalrPlayerHub(LogEvents logEvents, SpellWindowViewModelFactory spellWindowViewModelFactory, IAppDispatcher appDispatcher, LogParser logParser, ActivePlayer activePlayer, SpellWindowViewModel spellWindowViewModel)
        {
            this.logEvents = logEvents;
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.logParser = logParser;
            this.spellWindowViewModelFactory = spellWindowViewModelFactory;
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
            _ = connection.On("TriggerEvent", (TriggerEvent p) =>
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
            this.logEvents.SpellCastEvent += LogParser_StartCastingEvent;
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
        private void LogParser_StartCastingEvent(object sender, SpellCastEvent e)
        {
            if (e.CastByYou &&
                LastPlayer != null &&
                !string.IsNullOrWhiteSpace(activePlayer?.Player?.Zone) &&
                !string.IsNullOrWhiteSpace(activePlayer?.Player?.Name) &&
                activePlayer.Player.Server.HasValue &&
                (e.Spell.type == SpellTypes.Detrimental || e.Spell.type == SpellTypes.Other)
             )
            {
                var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(e.Spell, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level));
                var isnpc = MasterNPCList.NPCs.Contains(e.TargetName);
                if (isnpc)
                {
                    var s = new TriggerEvent
                    {
                        Classes = e.Spell.Classes,
                        DurationInSeconds = (int)spellduration.TotalSeconds,
                        Zone = activePlayer.Player?.Zone,
                        GuildName = activePlayer.Player.GuildName,
                        MapLocationSharing = activePlayer.Player.MapLocationSharing,
                        Name = e.Spell.name,
                        Server = activePlayer.Player.Server.Value,
                        SpellNameIcon = e.Spell.name,
                        SpellType = e.Spell.type,
                        TargetName = e.TargetName,
                        X = LastPlayer.X,
                        Y = LastPlayer.Y,
                        Z = LastPlayer.Z
                    };

                    InvokeAsync("TriggerEvent", s);
                }
            }
        }

        public void AddCustomTrigger(SignalrCustomTimer p)
        {
            if (p.Server == activePlayer?.Player?.Server)
            {
                Debug.WriteLine($"AddCustomTrigger {p.Name}");
                appDispatcher.DispatchUI(() =>
                {
                    spellWindowViewModel.TryAdd(spellWindowViewModelFactory.Create(p));
                });
            }
        }

        public void AddCustomTrigger(TriggerEvent p)
        {
            if (LastPlayer == null || activePlayer?.Player == null || p.Server != activePlayer.Player.Server || p.Zone != activePlayer.Player.Zone || !activePlayer.Player.SpellDebuffShare)
            {
                return;
            }
            var loc1 = new Point3D(LastPlayer.X, LastPlayer.Y, LastPlayer.Z);
            var loc2 = new Point3D(p.X, p.Y, p.Z);
            var distanceSquared = (loc1 - loc2).LengthSquared;
            if (distanceSquared <= 500)
            {
                Debug.WriteLine($"AddCustomTrigger {p.TargetName}");
                appDispatcher.DispatchUI(() =>
                {
                    spellWindowViewModel.TryAdd(spellWindowViewModelFactory.Create(p));
                });
            }
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
                    PlayerClass = activePlayer.Player.PlayerClass,
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
