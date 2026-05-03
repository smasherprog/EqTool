using EQTool.Models;
using EQTool.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace EQTool.UI
{
    public partial class BaseSaveStateWindow : Window
    {
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500),
            IsEnabled = false
        };

        private readonly DispatcherTimer _transparencyTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5),
            IsEnabled = false
        };

        private bool _isTransparent = false;
        private Thickness _savedBorderThickness;
        private Brush _savedWindowBackground;

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out WindowExtensions.RECT lpRect);

        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly EQToolSettings settings;
        private readonly Models.WindowState windowState;
        private bool InitCalled = false;
        protected DateTime LastWindowInteraction = DateTime.Now;
        public BaseSaveStateWindow(Models.WindowState windowState, EQToolSettingsLoad toolSettingsLoad, EQToolSettings settings)
        {
            this.windowState = windowState;
            this.toolSettingsLoad = toolSettingsLoad;
            this.settings = settings;
            windowState.Closed = false;
        }

        protected void Init()
        {
            if (InitCalled)
            {
                return;
            }

            InitCalled = true;
            AdjustWindow();
            timer.Tick += timer_Tick;
            SizeChanged += Window_SizeChanged;
            StateChanged += SpellWindow_StateChanged;
            LocationChanged += Window_LocationChanged;
            Activated += OnWindowActivated;
            Deactivated += OnWindowDeactivated;
            _transparencyTimer.Tick += OnTransparencyTimerTick;
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

        protected void DragWindow(object sender, MouseButtonEventArgs args)
        {
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
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            _transparencyTimer.Stop();
            RestoreChrome();
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            _transparencyTimer.Stop();
            _transparencyTimer.Start();
        }

        private void OnTransparencyTimerTick(object sender, EventArgs e)
        {
            _transparencyTimer.Stop();
            if (IsOverlappingEQGame())
            {
                HideChrome();
            }
        }

        private void HideChrome()
        {
            if (_isTransparent)
            {
                return;
            }

            _isTransparent = true;
            _savedWindowBackground = Background;
            Background = Brushes.Transparent;
            if (FindName("WindowOuterBorder") is Border outerBorder)
            {
                _savedBorderThickness = outerBorder.BorderThickness;
                outerBorder.BorderThickness = new Thickness(0);
            }
            if (FindName("TitleBarBorder") is Border titleBar)
            {
                titleBar.Opacity = 0.0;
            }
        }

        private void RestoreChrome()
        {
            if (!_isTransparent)
            {
                return;
            }

            _isTransparent = false;
            Background = _savedWindowBackground;
            if (FindName("WindowOuterBorder") is Border outerBorder)
            {
                outerBorder.BorderThickness = _savedBorderThickness;
            }
            if (FindName("TitleBarBorder") is Border titleBar)
            {
                titleBar.Opacity = 1.0;
            }
        }

        private bool IsOverlappingEQGame()
        {
            var processes = Process.GetProcessesByName("eqgame");
            if (processes.Length == 0)
            {
                return false;
            }

            var eqHandle = processes[0].MainWindowHandle;
            if (eqHandle == IntPtr.Zero)
            {
                return false;
            }

            if (!GetWindowRect(eqHandle, out var eqRect))
            {
                return false;
            }

            var thisHandle = new WindowInteropHelper(this).Handle;
            if (thisHandle == IntPtr.Zero || !GetWindowRect(thisHandle, out var thisRect))
            {
                return false;
            }

            // Both rects are in physical pixels — safe to compare directly
            return thisRect.Left < eqRect.Right && thisRect.Right > eqRect.Left &&
                   thisRect.Top < eqRect.Bottom && thisRect.Bottom > eqRect.Top;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (timer != null)
            {
                timer.Tick -= timer_Tick;
            }
            timer?.Stop();
            _transparencyTimer.Tick -= OnTransparencyTimerTick;
            _transparencyTimer.Stop();
            SizeChanged -= Window_SizeChanged;
            StateChanged -= SpellWindow_StateChanged;
            LocationChanged -= Window_LocationChanged;
            Activated -= OnWindowActivated;
            Deactivated -= OnWindowDeactivated;
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
    }
}
