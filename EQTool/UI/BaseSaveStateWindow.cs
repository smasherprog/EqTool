using EQTool.Models;
using EQTool.Services;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using EQTool.ViewModels;

namespace EQTool.UI
{
    public partial class BaseSaveStateWindow : Window
    {
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500),
            IsEnabled = false
        };
        protected readonly IAppDispatcher appDispatcher;
        private readonly BaseWindowViewModel baseViewModel;
        private readonly Models.WindowState windowState;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly EQToolSettings settings;
        
        private CancellationTokenSource hoverDebounceTs;
        private bool InitCalled = false;
        protected DateTime LastWindowInteraction = DateTime.Now;

        public BaseSaveStateWindow(IAppDispatcher appDispatcher, BaseWindowViewModel baseViewModel, Models.WindowState windowState, EQToolSettingsLoad toolSettingsLoad, EQToolSettings settings)
        {
            this.appDispatcher = appDispatcher;
            this.baseViewModel = baseViewModel;
            this.windowState = windowState;
            this.toolSettingsLoad = toolSettingsLoad;
            this.settings = settings;
            windowState.Closed = false;
        }

        protected void Init()
        {
            if (InitCalled)
                return;

            InitCalled = true;
            AdjustWindow();
            UpdateShowInTaskbar();
            timer.Tick += timer_Tick;
            SizeChanged += Window_SizeChanged;
            StateChanged += SpellWindow_StateChanged;
            LocationChanged += Window_LocationChanged;
            Loaded += Window_Loaded;
            windowState.Closed = false;
            SaveState();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.IsEnabled = false;
            SaveState();
        }

        private void DebounceSave()
        {
            timer.IsEnabled = true;
            timer.Stop();
            timer.Start();
        }

        public void SaveState()
        {
            SaveWindowState(windowState);
            toolSettingsLoad.Save(settings);
        }

        public void CloseWindow()
        {
            windowState.Closed = true;
            SaveState();
            Close();
        }
        
        protected virtual void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetClickThrough(settings.IsClickThroughMode && windowState.ClickThroughAllowed);
        }
        
        private void SpellWindow_StateChanged(object sender, EventArgs e)
        {
            LastWindowInteraction = DateTime.Now;
            DebounceSave();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            LastWindowInteraction = DateTime.Now;
            DebounceSave();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LastWindowInteraction = DateTime.Now;
            DebounceSave();
        }

        protected void Window_ToggleLock(object sender, RoutedEventArgs e)
        {
            baseViewModel.IsLocked = !baseViewModel.IsLocked;
            windowState.IsLocked = baseViewModel.IsLocked;
            
            LastWindowInteraction = DateTime.Now;
            DebounceSave();
            UpdateShowInTaskbar();
        }
        
        protected void DragWindow(object sender, MouseButtonEventArgs args)
        {
            if (baseViewModel.IsLocked)
                return;
            
            LastWindowInteraction = DateTime.Now;
            DragMove();
        }

        private void AdjustWindow()
        {
            Topmost = windowState.AlwaysOnTop;
            if (WindowBounds.isPointVisibleOnAScreen(windowState.WindowRect))
            {
                Left = windowState.WindowRect.Value.Left;
                Top = windowState.WindowRect.Value.Top;
                Height = windowState.WindowRect.Value.Height;
                Width = windowState.WindowRect.Value.Width;
                WindowState = windowState.State;
            }
            baseViewModel.IsLocked = windowState.IsLocked;
        }

        private void SaveWindowState(EQTool.Models.WindowState windowState)
        {
            windowState.WindowRect = new Rect
            {
                X = Left,
                Y = Top,
                Height = Height,
                Width = Width
            };
            windowState.State = WindowState;
            windowState.AlwaysOnTop = Topmost;
            windowState.IsLocked = baseViewModel.IsLocked;
        }
        
        public void SetClickThrough(bool enable)
        {
            this.ToggleClickThrough(enable);
            baseViewModel.IsCurrentlyClickThrough = enable;
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            if (timer != null)
            {
                timer.Tick -= timer_Tick;
            }
            timer?.Stop();
            SizeChanged -= Window_SizeChanged;
            StateChanged -= SpellWindow_StateChanged;
            LocationChanged -= Window_LocationChanged;
            base.OnClosing(e);
        }
        
        protected void openmobinfo(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenMobInfoWindow();
        }

        protected void opendps(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenDPSWindow();
        }

        protected void opensettings(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSettingsWindow();
        }
        
        protected void openmap(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenMapWindow();
        }

        protected void openspells(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSpellsWindow();
        }

        protected void CloseWindow(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        protected void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        protected void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == System.Windows.WindowState.Maximized ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized;
        }
        
        protected void HoverZone_MouseEnter(object sender, MouseEventArgs e)
        {
            hoverDebounceTs?.Cancel();
            hoverDebounceTs = new CancellationTokenSource();
            var debounceToken = hoverDebounceTs.Token;

            Task.Run(async () =>
            {
                await Task.Delay(150, debounceToken);
                appDispatcher.DispatchUI(() =>
                {
                    if (!debounceToken.IsCancellationRequested)
                    {
                        baseViewModel.IsMouseOverTitleArea = true;
                    }
                });
            }, debounceToken);
        }

        protected void HoverZone_MouseLeave(object sender, MouseEventArgs e)
        {
            hoverDebounceTs?.Cancel();
            hoverDebounceTs = new CancellationTokenSource();
            var debounceToken = hoverDebounceTs.Token;

            Task.Run(async () =>
            {
                await Task.Delay(150, debounceToken);
                appDispatcher.DispatchUI(() =>
                {
                    if (!debounceToken.IsCancellationRequested)
                    {
                        baseViewModel.IsMouseOverTitleArea = false;
                    }
                });
            }, debounceToken);
        }
        
        public void UpdateShowInTaskbar()
        {
            ShowInTaskbar = !(windowState.IsLocked && windowState.AlwaysOnTop);
        }
    }
}
