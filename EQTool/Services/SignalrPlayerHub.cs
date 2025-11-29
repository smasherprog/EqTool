using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared.HubModels;
using EQToolShared.Map;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Media3D;

namespace EQTool.Models
{
    public class SignalrPlayerHub
    {
        private readonly HubConnection connection;
        private readonly ActivePlayer activePlayer;
        private readonly LogParser logParser;
        private readonly LogEvents logEvents;
        private readonly System.Timers.Timer timer;
        private SignalrPlayerV2 LastPlayer;
        private readonly EQSpells spells;
        private readonly DebugOutput debugOutput;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public SignalrPlayerHub(EQSpells spells, LogEvents logEvents, LogParser logParser, ActivePlayer activePlayer, SpellWindowViewModel spellWindowViewModel, DebugOutput debugOutput)
        {
            this.spells = spells;
            this.logEvents = logEvents;
            this.activePlayer = activePlayer;
            this.logParser = logParser;
            this.debugOutput = debugOutput;
            this.spellWindowViewModel = spellWindowViewModel;
            var url = "https://www.pigparse.org/PP";
            connection = new HubConnectionBuilder()
              .WithUrl(url)
              .WithAutomaticReconnect()
              .Build();
            _ = connection.On("PlayerLocationEvent", (SignalrPlayerV2 p) =>
                {
                    if (!(p.Server == activePlayer?.Player?.Server && p.Name == activePlayer?.Player?.Name))
                    {
                        debugOutput.WriteLine($"{p.Zone}-{p.GuildName}-{p.Server}-{p.Sharing}-{p.Name}-({p.X},{p.Y},{p.Z})", OutputType.Map, MessageType.RemoteMessageReceived);
                        this.logEvents.Handle(new OtherPlayerLocationReceivedRemoteEvent { Player = p });
                    }
                });
            _ = connection.On("PlayerDisconnected", (SignalrPlayerV2 p) =>
            {
                if (!(p.Server == activePlayer?.Player?.Server && p.Name == activePlayer?.Player?.Name))
                {
                    debugOutput.WriteLine($"{p.Name}", OutputType.Map, MessageType.RemoteMessageReceived);
                    this.logEvents.Handle(new PlayerDisconnectReceivedRemoteEvent { Player = p });
                }
            });
            _ = connection.On("AddCustomTrigger", (SignalrCustomTimer p) =>
            {
                FactionPullReceivedRemotely(p);
            });
            _ = connection.On("DragonRoarEvent", (SignalRDragonRoar p) =>
            {
                DragonRoarReceivedRemotely(p);
            });
            connection.Closed += async (error) =>
            {
                this.debugOutput.WriteLine($"SignalR Close '{error?.ToString()}'", OutputType.Map, MessageType.Warning);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await SignalrConnectWithRetry();
            };
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

            this.logEvents.PlayerLocationEvent += SendMyLocationToOthers;
            this.logEvents.CampEvent += LogParser_CampEvent;
            this.logEvents.DragonRoarEvent += SendDragonRoarToOthers;
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
                if ((DateTime.Now - logParser.LastYouActivity).TotalMinutes > 5)
                {
                    LastPlayer = null;
                }
                else
                {
                    InvokeAsync("PlayerLocationEvent", LastPlayer);
                }
            }
        }

        private async Task SignalrConnectWithRetry()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    debugOutput.WriteLine($"SignalR StartAsync", OutputType.Map);
                    await connection.StartAsync();
                    try
                    {
                        InvokeAsync("JoinServerGroup", activePlayer?.Player?.Server);
                    }
                    catch { }
                    debugOutput.WriteLine($"SignalR Connected", OutputType.Map, MessageType.Success);
                    return;
                }
                catch (Exception)
                {
                    debugOutput.WriteLine($"SignalR Connected", OutputType.Map, MessageType.Warning);
                    await Task.Delay(5000);
                }
            }
        }

        private void SendMyLocationToOthers(object sender, PlayerLocationEvent e)
        {
            if (activePlayer?.Player?.Server != null)
            {
                LastPlayer = new SignalrPlayerV2
                {
                    Zone = activePlayer.Player.Zone,
                    GuildName = activePlayer.Player.GuildName,
                    Server = activePlayer.Player.Server.Value,
                    Sharing = activePlayer.Player.MapLocationSharing,
                    Name = activePlayer.Player.Name,
                    TrackingDistance = activePlayer.Player.TrackingDistance,
                    X = e.Location.X,
                    Y = e.Location.Y,
                    Z = e.Location.Z
                };
                debugOutput.WriteLine($"{LastPlayer.Zone}-{LastPlayer.GuildName}-{LastPlayer.Server}-{LastPlayer.Sharing}-{LastPlayer.Name}-({LastPlayer.X},{LastPlayer.Y},{LastPlayer.Z})", OutputType.Map, MessageType.RemoteMessageSent);
                InvokeAsync("PlayerLocationEvent", LastPlayer);
            }
        }

        private readonly List<DragonRoarEvent> LastDragonRoars = new List<DragonRoarEvent>();

        private void SendDragonRoarToOthers(object sender, DragonRoarEvent e)
        {
            var recentlyEmittedSameRoar = LastDragonRoars.Any(a => a.Spell.name == e.Spell.name && (e.TimeStamp - a.TimeStamp).TotalSeconds < 4);
            if (activePlayer?.Player != null &&
                !string.IsNullOrWhiteSpace(activePlayer.Player.Zone) &&
                activePlayer.Player.ShareTimers &&
                activePlayer.Player.Server.HasValue &&
                !recentlyEmittedSameRoar
               )
            {
                LastDragonRoars.Add(e);
                debugOutput.WriteLine($"{e.Spell.name}-({LastPlayer.X},{LastPlayer.Y},{LastPlayer.Z})", OutputType.Map, MessageType.RemoteMessageSent);
                InvokeAsync("DragonRoarEvent", new SignalRDragonRoar
                {
                    SpellName = e.Spell.name,
                    Server = activePlayer.Player.Server.Value,
                    Zone = activePlayer.Player.Zone,
                    Sharing = activePlayer.Player.MapLocationSharing,
                    GuildName = activePlayer.Player.GuildName,
                    X = LastPlayer?.X,
                    Y = LastPlayer?.Y,
                    Z = LastPlayer?.Z
                });
                var now = DateTime.Now;
                _ = LastDragonRoars.RemoveAll(a => (now - a.TimeStamp).TotalSeconds > 45);
            }
        }

        private void DragonRoarReceivedRemotely(SignalRDragonRoar p)
        {
            if (activePlayer?.Player != null &&
                p.Server == activePlayer.Player.Server &&
                activePlayer.Player.ShareTimers)
            {
                debugOutput.WriteLine($"{p.SpellName}-({p.X},{p.Y},{p.Z})", OutputType.Map, MessageType.RemoteMessageReceived);
                Point3D? Location = null;
                if (p.X.HasValue && p.Y.HasValue && p.Z.HasValue)
                {
                    Location = new Point3D(p.X.Value, p.Y.Value, p.Z.Value);
                }
                logEvents.Handle(new DragonRoarRemoteEvent
                {
                    SpellName = p.SpellName,
                    Location = Location
                });
            }
        }

        public void FactionPullReceivedRemotely(SignalrCustomTimer p)
        {
            if (p.Server == activePlayer?.Player?.Server)
            {
                debugOutput.WriteLine($"{p.Name}-{p.Server}", OutputType.Map, MessageType.RemoteMessageReceived);
                spellWindowViewModel.TryAdd(Create(p));
            }
        }

        private TimerViewModel Create(HubCustomTimer e)
        {
            if (string.IsNullOrWhiteSpace(e.SpellNameIcon) || !spells.AllSpells.TryGetValue(e.SpellNameIcon, out var spellicon))
            {
                _ = spells.AllSpells.TryGetValue("Feign Death", out spellicon);
            }

            return new TimerViewModel
            {
                PercentLeft = 100,
                Target = CustomTimer.CustomerTime,
                Id = e.Name,
                Rect = spellicon.Rect,
                Icon = spellicon.SpellIcon,
                TotalDuration = TimeSpan.FromSeconds(e.DurationInSeconds),
                TotalRemainingDuration = TimeSpan.FromSeconds(e.DurationInSeconds),
                UpdatedDateTime = DateTime.Now
            };
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
