using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EQTool
{
    public class ChainData
    {
        public Canvas Canvas { get; set; }
        public Grid Grid { get; set; }
        public string Destination { get; set; }
        public int ActiveAnimations { get; set; } = 0;
    }

    public partial class EventOverlay : Window
    {
        private readonly System.Timers.Timer UITimer;
        private readonly LogParser logParser;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private DateTime LastWindowInteraction = DateTime.UtcNow;
        private readonly List<ChainData> chainDatas = new List<ChainData>();

        public EventOverlay(LogParser logParser, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad, LoggingService loggingService, ActivePlayer activePlayer, IAppDispatcher appDispatcher)
        {
            this.appDispatcher = appDispatcher;
            this.activePlayer = activePlayer;
            this.settings = settings;
            this.logParser = logParser;
            InitializeComponent();
            WindowExtensions.AdjustWindow(settings.OverlayWindowState, this);
            this.Topmost = true;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            this.toolSettingsLoad = toolSettingsLoad;
            SizeChanged += Window_SizeChanged;
            StateChanged += SpellWindow_StateChanged;
            LocationChanged += Window_LocationChanged;
            logParser.EnrageEvent += LogParser_EnrageEvent;
            logParser.CHEvent += LogParser_CHEvent;
            settings.OverlayWindowState.Closed = false;
            SaveState();
        }

        private void LogParser_CHEvent(object sender, ChParser.ChParseData e)
        {
            var overlay = this.activePlayer?.Player?.ChChainOverlay ?? false;
            if (!overlay)
            {
                return;
            }
            appDispatcher.DispatchUI(() =>
            {
                var chaindata = this.GetOrCreateChain(e.Recipient);
                var target = new TextBlock
                {
                    Height = 30,
                    FontSize = settings.FontSize.Value * 2,
                    Width = chaindata.Canvas.ActualWidth / 10,
                    Text = e.Position.ToString(),
                    Background = Brushes.Red,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center
                };

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = -target.Width;
                animation.To = chaindata.Canvas.ActualWidth;
                animation.Duration = TimeSpan.FromSeconds(11); // Adjust duration as needed

                Storyboard.SetTarget(animation, target);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.RightProperty));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                chaindata.Canvas.Children.Add(target);
                storyboard.Completed += (s, ev) =>
                {
                    chaindata.ActiveAnimations--;
                    if (chaindata.ActiveAnimations <= 0)
                    {
                        this.chainDatas.Remove(chaindata);
                        this.ChainStackPanel.Children.Remove(chaindata.Grid);
                    }
                };
                storyboard.Begin();
            });
        }

        private ChainData GetOrCreateChain(string destination)
        {
            var chaindata = this.chainDatas.FirstOrDefault(a => a.Destination == destination);
            if (chaindata != null)
            {
                chaindata.ActiveAnimations += 1;
                return chaindata;
            }
            chaindata = new ChainData
            {
                Canvas = new Canvas(),
                Grid = new Grid(),
                Destination = destination,
                ActiveAnimations = 1
            };
            chaindata.Canvas.IsHitTestVisible = false;
            chaindata.Canvas.Background = Brushes.Transparent;
            chaindata.Grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            chaindata.Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            chaindata.Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var target = new TextBlock
            {
                Height = 30,
                FontSize = settings.FontSize.Value * 1.5,
                Text = destination,
                Background = Brushes.Blue,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetRow(target, 0);
            Grid.SetZIndex(target, 1);
            Grid.SetColumn(target, 0);
            Grid.SetRow(chaindata.Canvas, 0);
            Grid.SetColumn(chaindata.Canvas, 1);
            Grid.SetZIndex(chaindata.Canvas, 0);
            chaindata.Grid.Children.Add(target);
            chaindata.Grid.Children.Add(chaindata.Canvas);
            this.chainDatas.Add(chaindata);
            this.ChainStackPanel.Children.Add(chaindata.Grid);
            chaindata.Canvas.UpdateLayout();
            return chaindata;
        }

        private void LogParser_EnrageEvent(object sender, EnrageParser.EnrageEvent e)
        {
            var overlay = this.activePlayer?.Player?.EnrageOverlay ?? false;
            if (!overlay)
            {
                return;
            }
            appDispatcher.DispatchUI(() =>
            {
                CenterText.Text = e.NpcName + " ENRAGED";
            });

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(1000 * 10);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "ENGRAGE OFF";
                });
                System.Threading.Thread.Sleep(1000 * 3);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = string.Empty;
                });
            });
        }

        private void SaveState()
        {
            WindowExtensions.SaveWindowState(settings.OverlayWindowState, this);
            toolSettingsLoad.Save(settings);
        }

        private void SpellWindow_StateChanged(object sender, EventArgs e)
        {
            LastWindowInteraction = DateTime.UtcNow;
            SaveState();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            LastWindowInteraction = DateTime.UtcNow;
            SaveState();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LastWindowInteraction = DateTime.UtcNow;
            SaveState();
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            LastWindowInteraction = DateTime.UtcNow;
            DragMove();
        }

        private void PollUI(object sender, EventArgs e)
        {

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer?.Stop();
            UITimer?.Dispose();
            SizeChanged -= Window_SizeChanged;
            StateChanged -= SpellWindow_StateChanged;
            LocationChanged -= Window_LocationChanged;
            logParser.EnrageEvent -= LogParser_EnrageEvent;
            logParser.CHEvent -= LogParser_CHEvent;
            SaveState();
            base.OnClosing(e);
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            WindowResizeChrome.ResizeBorderThickness = new Thickness(8);
            WindowBorder.BorderThickness = new Thickness(1, 1, 1, 1);
            LastWindowInteraction = DateTime.UtcNow;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (Math.Abs((DateTime.UtcNow - LastWindowInteraction).TotalSeconds) < 10)
                {
                    System.Threading.Thread.Sleep(1000 * 1);
                }
                this.appDispatcher.DispatchUI(() =>
                {
                    WindowResizeChrome.ResizeBorderThickness = new Thickness(0);
                    WindowBorder.BorderThickness = new Thickness(0, 0, 0, 0);
                });
            });
        }
    }
}
