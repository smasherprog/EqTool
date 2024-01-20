using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace EQTool
{
    public class ChainOverlayData : ChainData
    {
        public Canvas Canvas { get; set; }
        public List<FrameworkElement> ChildrenInRow { get; set; }
        public string TargetName { get; set; }
        public int ActiveAnimations { get; set; } = 0;
        public RowDefinition RowDefinition { get; set; }
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
        private readonly List<ChainOverlayData> chainDatas = new List<ChainOverlayData>();
        private readonly PigParseApi pigParseApi;
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500),
            IsEnabled = false
        };

        public EventOverlay(LogParser logParser, EQToolSettings settings, PigParseApi pigParseApi, EQToolSettingsLoad toolSettingsLoad, ActivePlayer activePlayer, IAppDispatcher appDispatcher)
        {
            this.pigParseApi = pigParseApi;
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
            timer.Tick += timer_Tick;
            SizeChanged += Window_SizeChanged;
            StateChanged += SpellWindow_StateChanged;
            LocationChanged += Window_LocationChanged;
            logParser.EnrageEvent += LogParser_EnrageEvent;
            logParser.CHEvent += LogParser_CHEvent;
            logParser.LevEvent += LogParser_LevEvent;
            logParser.InvisEvent += LogParser_InvisEvent;
            logParser.FTEEvent += LogParser_FTEEvent;
            logParser.CharmBreakEvent += LogParser_CharmBreakEvent;
            logParser.FailedFeignEvent += LogParser_FailedFeignEvent;
            logParser.GroupInviteEvent += LogParser_GroupInviteEvent;
            logParser.StartCastingEvent += LogParser_StartCastingEvent;
            settings.OverlayWindowState.Closed = false;
        }

        private void LogParser_StartCastingEvent(object sender, LogParser.SpellEventArgs e)
        {
            var overlay = this.activePlayer?.Player?.DragonRoarOverlay ?? false;
            if (!overlay || e.Spell.Spell.name != "Dragon Roar")
            {
                return;
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(1000 * 30);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "Dragon Roar in 6 Seconds!";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "Dragon Roar in 5 Seconds!";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "Dragon Roar in 4 Seconds!";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "Dragon Roar in 3 Seconds!";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "Dragon Roar in 2 Seconds!";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "Dragon Roar in 1 Seconds!";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = string.Empty;
                    CenterText.Foreground = Brushes.Red;
                });
            });
        }

        private void LogParser_GroupInviteEvent(object sender, string e)
        {
            var overlay = this.activePlayer?.Player?.GroupInviteOverlay ?? false;
            if (!overlay)
            {
                return;
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = e;
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000 * 5);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = string.Empty;
                    CenterText.Foreground = Brushes.Red;
                });
            });
        }

        private void LogParser_FailedFeignEvent(object sender, string e)
        {
            var overlay = this.activePlayer?.Player?.FailedFeignOverlay ?? false;
            if (!overlay)
            {
                return;
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "Feign Failed Death!";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000 * 5);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = string.Empty;
                    CenterText.Foreground = Brushes.Red;
                });
            });
        }

        private void LogParser_CharmBreakEvent(object sender, LogParser.CharmBreakArgs e)
        {
            var overlay = this.activePlayer?.Player?.CharmBreakOverlay ?? false;
            if (!overlay)
            {
                return;
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "Charm Break";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000 * 5);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = string.Empty;
                    CenterText.Foreground = Brushes.Red;
                });
            });
        }

        private void LogParser_FTEEvent(object sender, FTEParser.FTEParserData e)
        {
            var overlay = this.activePlayer?.Player?.FTEOverlay ?? false;
            if (!overlay)
            {
                return;
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var fteperson = this.pigParseApi.GetPlayerData(e.FTEPerson, this.activePlayer.Player.Server.Value);
                this.appDispatcher.DispatchUI(() =>
                {
                    if (fteperson == null)
                    {
                        CenterText.Text = $"{e.FTEPerson} FTE {e.NPCName}";
                        CenterText.Foreground = Brushes.Yellow;
                    }
                    else
                    {
                        CenterText.Text = $"{fteperson.Name} <{fteperson.GuildName}> FTE {e.NPCName}";
                        CenterText.Foreground = Brushes.Yellow;
                    }
                });
                System.Threading.Thread.Sleep(1000 * 5);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = string.Empty;
                    CenterText.Foreground = Brushes.Yellow;
                });
            });
        }

        private void LogParser_InvisEvent(object sender, InvisParser.InvisStatus e)
        {
            var overlay = this.activePlayer?.Player?.InvisFadingOverlay ?? false;
            if (!overlay)
            {
                return;
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "Invis Fading";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000 * 5);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = string.Empty;
                    CenterText.Foreground = Brushes.Red;
                });
            });
        }

        private void LogParser_LevEvent(object sender, LevParser.LevStatus e)
        {
            var overlay = this.activePlayer?.Player?.LevFadingOverlay ?? false;
            if (!overlay)
            {
                return;
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "Levitate Fading";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000 * 5);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = string.Empty;
                    CenterText.Foreground = Brushes.Red;
                });
            });
        }

        private void LogParser_CHEvent(object sender, ChParser.ChParseData e)
        {
            var overlay = this.activePlayer?.Player?.ChChainOverlay ?? false;
            var warningoverlay = this.activePlayer?.Player?.ChChainWarningOverlay ?? false;
            if (!overlay && !warningoverlay)
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var chaindata = this.GetOrCreateChain(e.Recipient);
                if (warningoverlay)
                {
                    var shouldwarn = CHService.ShouldWarnOfChain(chaindata, e);
                    if (shouldwarn)
                    {
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            this.appDispatcher.DispatchUI(() =>
                            {
                                CenterText.Text = "CH Chain Warning";
                                CenterText.Foreground = Brushes.Red;
                            });
                            System.Threading.Thread.Sleep(1000 * 2);
                            this.appDispatcher.DispatchUI(() =>
                            {
                                CenterText.Text = string.Empty;
                                CenterText.Foreground = Brushes.Red;
                            });
                        });
                    }
                }

                var random = new Random(DateTime.Now.Millisecond);
                var color = System.Windows.Media.Color.FromRgb(
                    (byte)random.Next(0, 40),
                    (byte)random.Next(0, 40),
                    (byte)random.Next(0, 40));
                var colorA = System.Windows.Media.Color.FromArgb(140, color.R, color.G, color.B);
                var targetwidth = chaindata.Canvas.ActualWidth / 10.0;
                var target = new TextBlock
                {
                    FontSize = settings.FontSize.Value * 2,
                    Text = e.Position.ToString(),
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                var textborder = new Border { Height = 28, Width = targetwidth, Background = Brushes.ForestGreen, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
                textborder.Child = target;
                DoubleAnimation animation = new DoubleAnimation();
                animation.From = -targetwidth;
                animation.To = chaindata.Canvas.ActualWidth;
                animation.Duration = TimeSpan.FromSeconds(11);

                Storyboard.SetTarget(animation, textborder);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.RightProperty));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                chaindata.Canvas.Children.Add(textborder);

                storyboard.Completed += (s, ev) =>
                {
                    chaindata.ActiveAnimations--;
                    appDispatcher.DispatchUI(() =>
                    {
                        chaindata.Canvas.Children.Remove(textborder);
                    });
                    if (chaindata.ActiveAnimations <= 0)
                    {
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            System.Threading.Thread.Sleep(5000);
                            appDispatcher.DispatchUI(() =>
                            {
                                if (chaindata.ActiveAnimations <= 0)
                                {
                                    this.chainDatas.Remove(chaindata);
                                    foreach (var item in chaindata.ChildrenInRow)
                                    {
                                        this.ChainStackPanel.Children.Remove(item);
                                    }
                                    this.ChainStackPanel.RowDefinitions.Remove(chaindata.RowDefinition);
                                }
                            });
                        });
                    }
                };
                storyboard.Begin();
            });
        }

        private ChainOverlayData GetOrCreateChain(string targetname)
        {
            var chaindata = this.chainDatas.FirstOrDefault(a => a.TargetName == targetname);
            if (chaindata != null)
            {
                chaindata.ActiveAnimations += 1;
                return chaindata;
            }
            chaindata = new ChainOverlayData
            {
                Canvas = new Canvas(),
                ChildrenInRow = new List<FrameworkElement>(),
                TargetName = targetname,
                ActiveAnimations = 1,
                RowDefinition = new RowDefinition { MaxHeight = 30 }
            };
            chaindata.Canvas.IsHitTestVisible = false;
            chaindata.Canvas.Background = Brushes.Transparent;
            var target = new TextBlock
            {
                Height = 30,
                FontSize = settings.FontSize.Value * 1.5,
                Text = targetname,
                Padding = new Thickness(4),
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var textborder = new Border { CornerRadius = new CornerRadius(3), Background = Brushes.Chocolate, BorderThickness = new Thickness(2), BorderBrush = Brushes.Black };
            textborder.Child = target;
            var getrow = this.ChainStackPanel.RowDefinitions.Count;
            Grid.SetRow(textborder, getrow);
            Grid.SetZIndex(textborder, 1);
            Grid.SetColumn(textborder, 0);
            Grid.SetRow(chaindata.Canvas, getrow);
            Grid.SetColumn(chaindata.Canvas, 1);
            Grid.SetZIndex(chaindata.Canvas, 0);
            var stackpanel = new StackPanel { Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 10, 10, 10)), Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 30 };
            Grid.SetRow(stackpanel, getrow);
            Grid.SetColumn(stackpanel, 1);
            this.ChainStackPanel.RowDefinitions.Add(chaindata.RowDefinition);
            this.ChainStackPanel.Children.Add(textborder);
            this.ChainStackPanel.Children.Add(stackpanel);
            this.ChainStackPanel.Children.Add(chaindata.Canvas);
            chaindata.ChildrenInRow.Add(textborder);
            chaindata.ChildrenInRow.Add(stackpanel);
            chaindata.ChildrenInRow.Add(chaindata.Canvas);
            this.chainDatas.Add(chaindata);
            chaindata.Canvas.UpdateLayout();
            for (var i = 0; i < 10; i++)
            {
                stackpanel.Children.Add(new Border
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Height = 30,
                    Width = chaindata.Canvas.ActualWidth / 10,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    BorderBrush = Brushes.WhiteSmoke,
                    BorderThickness = new Thickness(1, 0, 1, 2),
                    Child = new TextBlock
                    {
                        Text = (i + 1).ToString(),
                        Foreground = Brushes.Red,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Width = chaindata.Canvas.ActualWidth / 10,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    }
                });

            }
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
                CenterText.Foreground = Brushes.Red;
            });

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(1000 * 10);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = "ENGRAGE OFF";
                    CenterText.Foreground = Brushes.Red;
                });
                System.Threading.Thread.Sleep(1000 * 3);
                this.appDispatcher.DispatchUI(() =>
                {
                    CenterText.Text = string.Empty;
                    CenterText.Foreground = Brushes.Red;
                });
            });
        }

        void timer_Tick(object sender, EventArgs e)
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

        private void SaveState()
        {
            Debug.WriteLine("Saving Overlay window State");
            WindowExtensions.SaveWindowState(settings.OverlayWindowState, this);
            toolSettingsLoad.Save(settings);
        }

        private void SpellWindow_StateChanged(object sender, EventArgs e)
        {
            LastWindowInteraction = DateTime.UtcNow;
            DebounceSave();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            LastWindowInteraction = DateTime.UtcNow;
            DebounceSave();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LastWindowInteraction = DateTime.UtcNow;
            DebounceSave();
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
            if (logParser != null)
            {
                logParser.EnrageEvent -= LogParser_EnrageEvent;
                logParser.CHEvent -= LogParser_CHEvent;
            }
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
