using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared.HubModels;
using EQToolShared.Map;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Media3D;

namespace EQTool.Models
{

    public interface ISignalrPlayerHub : IDisposable
    {
        event EventHandler<SignalrPlayerV2> PlayerLocationEvent;
        event EventHandler<SignalrPlayerV2> PlayerDisconnected;
        void PushPlayerLocationEvent(SignalrPlayerV2 player);
        void PushPlayerDisconnected(SignalrPlayerV2 player);
    }

    public class SignalrPlayerHub : ISignalrPlayerHub
    {
        private readonly HubConnection connection;
        private readonly ActivePlayer activePlayer;
        private readonly LogParser logParser;
        private readonly LogEvents logEvents;
        private readonly IAppDispatcher appDispatcher;
        private readonly System.Timers.Timer timer;
        private SignalrPlayerV2 LastPlayer;
        private readonly EQSpells spells;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public SignalrPlayerHub(EQSpells spells, LogEvents logEvents, IAppDispatcher appDispatcher, LogParser logParser, ActivePlayer activePlayer, SpellWindowViewModel spellWindowViewModel)
        {
            this.spells = spells;
            this.logEvents = logEvents;
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.logParser = logParser;
            this.spellWindowViewModel = spellWindowViewModel;
            var url = "https://www.pigparse.org/PP";
            connection = new HubConnectionBuilder()
              .WithUrl(url)
              .WithAutomaticReconnect()
              .Build();
            _ = connection.On("PlayerLocationEvent", (SignalrPlayerV2 p) =>
                {
                    PushPlayerLocationEvent(p);
                });
            _ = connection.On("PlayerDisconnected", (SignalrPlayerV2 p) =>
            {
                PushPlayerDisconnected(p);
            });
            _ = connection.On("AddCustomTrigger", (SignalrCustomTimer p) =>
            {
                AddCustomTrigger(p);
            });
            _ = connection.On("DragonRoarEvent", (SignalRDragonRoar p) =>
            {
                DragonRoar(p);
            });
            connection.Closed += async (error) =>
              {
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

        public event EventHandler<SignalrPlayerV2> PlayerLocationEvent;
        public event EventHandler<SignalrPlayerV2> PlayerDisconnected;

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

        public void PushPlayerDisconnected(SignalrPlayerV2 p)
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

        private void LogParser_PlayerLocationEvent(object sender, PlayerLocationEvent e)
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

                InvokeAsync("PlayerLocationEvent", LastPlayer);
            }
        }

        private void LogEvents_DragonRoarEvent(object sender, DragonRoarEvent e)
        {
            if (activePlayer?.Player != null &&
                !string.IsNullOrWhiteSpace(activePlayer.Player.Zone) &&
                activePlayer.Player.ShareTimers &&
                activePlayer.Player.Server.HasValue)
            {
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
            }
        }

        private void DragonRoar(SignalRDragonRoar p)
        {
            if (activePlayer?.Player != null &&
                p.Server == activePlayer.Player.Server &&
                activePlayer.Player.ShareTimers)
            {
                Debug.WriteLine($"SignalRDragonRoar {p.SpellName}");
                Point3D? Location = null;
                if (p.X.HasValue && p.Y.HasValue && p.Z.HasValue)
                {
                    Location = new Point3D(p.X.Value, p.Y.Value, p.Z.Value);
                }
                this.logEvents.Handle(new DragonRoarRemoteEvent
                {
                    SpellName = p.SpellName,
                    Location = Location
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
                    spellWindowViewModel.TryAdd(Create(p));
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

        public void PushPlayerLocationEvent(SignalrPlayerV2 p)
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

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
