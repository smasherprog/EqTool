using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared;
using EQToolShared.Enums;
using EQToolShared.Map;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace EQTool.UI
{
    public partial class MappingWindow : BaseSaveStateWindow
    {
        private readonly LogEvents logEvents;
        private readonly MapViewModel mapViewModel;
        private readonly ActivePlayer activePlayer;
        private readonly PlayerTrackerService playerTrackerService;
        private readonly IAppDispatcher appDispatcher;
        private readonly ISignalrPlayerHub signalrPlayerHub;
        private readonly System.Timers.Timer UITimer;

        public MappingWindow(
            ISignalrPlayerHub signalrPlayerHub,
            MapViewModel mapViewModel,
            ActivePlayer activePlayer,
            LogEvents logEvents,
            EQToolSettings settings,
            PlayerTrackerService playerTrackerService,
            EQToolSettingsLoad toolSettingsLoad,
            IAppDispatcher appDispatcher,
            LoggingService loggingService) : base(settings.MapWindowState, toolSettingsLoad, settings)
        {
            loggingService.Log(string.Empty, EventType.OpenMap, activePlayer?.Player?.Server);
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
            this.signalrPlayerHub = signalrPlayerHub;
            this.playerTrackerService = playerTrackerService;
            this.appDispatcher = appDispatcher;
            DataContext = this.mapViewModel = mapViewModel;
            InitializeComponent();
            base.Init();
            _ = mapViewModel.LoadDefaultMap(Map);
            Map.ZoneName = mapViewModel.ZoneName;
            Map.Height = Math.Abs(mapViewModel.AABB.MaxHeight);
            Map.Width = Math.Abs(mapViewModel.AABB.MaxWidth);
            this.logEvents.PlayerLocationEvent += LogParser_PlayerLocationEvent;
            this.logEvents.YouZonedEvent += LogParser_PlayerZonedEvent;
            this.logEvents.EnteredWorldEvent += LogParser_EnteredWorldEvent;
            this.logEvents.SlainEvent += LogParser_DeathEvent;
            this.logEvents.StartTimerEvent += LogParser_StartTimerEvent;
            this.logEvents.CancelTimerEvent += LogParser_CancelTimerEvent;
            KeyDown += PanAndZoomCanvas_KeyDown;
            Map.StartTimerEvent += Map_StartTimerEvent;
            Map.CancelTimerEvent += Map_CancelTimerEvent;
            Map.TimerMenu_ClosedEvent += Map_TimerMenu_ClosedEvent;
            Map.TimerMenu_OpenedEvent += Map_TimerMenu_OpenedEvent;
            this.signalrPlayerHub.PlayerLocationEvent += SignalrPlayerHub_PlayerLocationEvent;
            this.signalrPlayerHub.PlayerDisconnected += SignalrPlayerHub_PlayerDisconnected;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += UITimer_Elapsed;
            UITimer.Enabled = true;
            //   this.SetCenerMap();
        }

        private void SignalrPlayerHub_PlayerDisconnected(object sender, SignalrPlayer e)
        {
            mapViewModel.PlayerDisconnected(e);
        }

        private void SignalrPlayerHub_PlayerLocationEvent(object sender, SignalrPlayer e)
        {
            mapViewModel.PlayerLocationEvent(e);
        }

        private void Map_PanAndZoomCanvas_MouseDownEvent(object sender, MouseButtonEventArgs e)
        {
            var mousePostion = e.GetPosition(Map);
            mapViewModel.PanAndZoomCanvas_MouseDown(mousePostion, e);
        }

        private void Map_TimerMenu_OpenedEvent(object sender, RoutedEventArgs e)
        {
            mapViewModel.TimerMenu_Opened();
        }

        private void Map_TimerMenu_ClosedEvent(object sender, RoutedEventArgs e)
        {
            mapViewModel.TimerMenu_Closed();
        }

        private void LogParser_StartTimerEvent(object sender, StartTimerEvent e)
        {
            var mw = mapViewModel.AddTimer(TimeSpan.FromSeconds(e.CustomTimer.DurationInSeconds), e.CustomTimer.Name, false);
            mapViewModel.MoveToPlayerLocation(mw);
        }

        private void LogParser_CancelTimerEvent(object sender, CancelTimerEvent e)
        {
            mapViewModel.DeleteSelectedTimerByName(e.Name);
        }

        private void Map_StartTimerEvent(object sender, StartTimerEvent e)
        {
            mapViewModel.TimerMenu_Closed();
            _ = mapViewModel.AddTimer(TimeSpan.FromSeconds(e.CustomTimer.DurationInSeconds), e.CustomTimer.Name, true);
        }

        private void Map_CancelTimerEvent(object sender, EventArgs e)
        {
            _ = mapViewModel.DeleteSelectedTimer();
        }

        private void LogParser_DeathEvent(object sender, SlainEvent e)
        {
            if (playerTrackerService.IsPlayer(e.Victim))
            {
                return;
            }

            if (activePlayer.Player?.MapKillTimers == true)
            {
                var zonetimer = ZoneSpawnTimes.GetSpawnTime(e.Victim, mapViewModel.ZoneName);
                var mw = mapViewModel.AddTimer(zonetimer, e.Victim, true);
                mapViewModel.MoveToPlayerLocation(mw);
            }
        }

        private void UITimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            appDispatcher.DispatchUI(() => mapViewModel.UpdateTimerWidgest());
        }

        private void PanAndZoomCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            var scale = MathHelper.ChangeRange(Math.Max(mapViewModel.AABB.MaxWidth, mapViewModel.AABB.MaxHeight), 500, 35000, 60, 300);
            switch (e.Key)
            {
                case Key.Left:
                case Key.A:
                    mapViewModel.MoveMap(scale, 0);
                    break;
                case Key.Right:
                case Key.D:
                    mapViewModel.MoveMap(-scale, 0);
                    break;
                case Key.Up:
                case Key.W:
                    mapViewModel.MoveMap(0, scale);
                    break;
                case Key.Down:
                case Key.S:
                    mapViewModel.MoveMap(0, -scale);
                    break;
                default:
                    return;

            }
        }

        private void LogParser_EnteredWorldEvent(object sender, EnteredWorldEvent e)
        {
            if (mapViewModel.LoadDefaultMap(Map))
            {
                Map.ZoneName = mapViewModel.ZoneName;
                Map.Height = Math.Abs(mapViewModel.AABB.MaxHeight);
                Map.Width = Math.Abs(mapViewModel.AABB.MaxWidth);
            }
        }

        private void LogParser_PlayerZonedEvent(object sender, YouZonedEvent e)
        {
            if (mapViewModel.LoadMap(e.ShortName, Map))
            {
                Map.ZoneName = mapViewModel.ZoneName;
                Map.Height = Math.Abs(mapViewModel.AABB.MaxHeight);
                Map.Width = Math.Abs(mapViewModel.AABB.MaxWidth);
            }
        }

        private void LogParser_PlayerLocationEvent(object sender, PlayerLocationEvent e)
        {
            mapViewModel.UpdateLocation(e.Location);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer?.Stop();
            UITimer?.Dispose();
            if (logEvents != null)
            {
                logEvents.PlayerLocationEvent -= LogParser_PlayerLocationEvent;
                logEvents.YouZonedEvent -= LogParser_PlayerZonedEvent;
                logEvents.EnteredWorldEvent -= LogParser_EnteredWorldEvent;
                logEvents.SlainEvent -= LogParser_DeathEvent;
                logEvents.StartTimerEvent -= LogParser_StartTimerEvent;
                logEvents.CancelTimerEvent -= LogParser_CancelTimerEvent;
            }

            KeyDown -= PanAndZoomCanvas_KeyDown;
            if (Map != null)
            {
                Map.StartTimerEvent -= Map_StartTimerEvent;
                Map.CancelTimerEvent -= Map_CancelTimerEvent;
                Map.TimerMenu_ClosedEvent -= Map_TimerMenu_ClosedEvent;
                Map.TimerMenu_OpenedEvent -= Map_TimerMenu_OpenedEvent;
            }
            if (signalrPlayerHub != null)
            {
                signalrPlayerHub.PlayerLocationEvent -= SignalrPlayerHub_PlayerLocationEvent;
                signalrPlayerHub.PlayerDisconnected -= SignalrPlayerHub_PlayerDisconnected;
            }
            base.OnClosing(e);
        }

        private void PanAndZoomCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mapViewModel.PanAndZoomCanvas_MouseUp(e.GetPosition(Map));
        }

        private void PanAndZoomCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            mapViewModel.PanAndZoomCanvas_MouseMove(e.GetPosition(Map), e.LeftButton);
        }

        private void PanAndZoomCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mapViewModel.PanAndZoomCanvas_MouseWheel(e.GetPosition(Map), e.Delta);
            mapViewModel.CenterMapOnPlayer();
        }

        protected void toggleCenterOnyou(object sender, RoutedEventArgs e)
        {
            mapViewModel.ToggleCenter();
        }
    }
}
