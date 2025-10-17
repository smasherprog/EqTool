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

namespace EQTool.UI
{
    public class ChainOverlayData : ChainData
    {
        public Canvas Canvas { get; set; }
        public List<FrameworkElement> ChildrenInRow { get; set; }
        public string TargetName { get; set; }
        public int ActiveAnimations { get; set; } = 0;
        public RowDefinition RowDefinition { get; set; }
        public System.Timers.Timer CHTimer { get; set; }
    }

    public partial class EventOverlay : BaseSaveStateWindow
    {
        private readonly EventOverlayViewModel eventOverlayViewModel;
        private readonly EQToolSettings settings;
        private readonly ActivePlayer activePlayer;
        private readonly List<ChainOverlayData> chainDatas = new List<ChainOverlayData>();
        private readonly PigParseApi pigParseApi;
        private readonly LogEvents logEvents;

        public EventOverlay(IAppDispatcher appDispatcher, EventOverlayViewModel eventOverlayViewModel, LogEvents logEvents, EQToolSettings settings, PigParseApi pigParseApi, EQToolSettingsLoad toolSettingsLoad, ActivePlayer activePlayer)
            : base(appDispatcher, eventOverlayViewModel, settings.OverlayWindowState, toolSettingsLoad, settings)
        {
            this.eventOverlayViewModel = eventOverlayViewModel;
            this.logEvents = logEvents;
            this.pigParseApi = pigParseApi;
            this.activePlayer = activePlayer;
            this.settings = settings;
            InitializeComponent();
            base.Init();
            Topmost = true;
            SaveState();
            logEvents.OverlayEvent += LogEvents_OverlayEvent;
            logEvents.CompleteHealEvent += LogParser_CHEvent;
        }

        private void LogEvents_OverlayEvent(object sender, OverlayEvent e)
        {
            appDispatcher.DispatchUI(() =>
            {
                if (e.Reset)
                {
                    if (CenterText.Text == e.Text && CenterText.Foreground == e.ForeGround)
                    {
                        CenterText.Text = string.Empty;
                        CenterText.Foreground = Brushes.Red;
                    }
                }
                else
                {
                    CenterText.Text = e.Text;
                    CenterText.Foreground = e.ForeGround;
                }
            });
        }

        private void LogParser_CHEvent(object sender, CompleteHealEvent e)
        {
            var overlay = activePlayer?.Player?.ChChainOverlay ?? false;
            var warningoverlay = activePlayer?.Player?.ChChainWarningOverlay ?? false;
            if (!overlay && !warningoverlay)
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var chaindata = GetOrCreateChain(e.Recipient);
                if (warningoverlay)
                {
                    var shouldwarn = CHService.ShouldWarnOfChain(chaindata, e);
                    if (shouldwarn)
                    {
                        _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            appDispatcher.DispatchUI(() =>
                            {
                                CenterText.Text = "CH Chain Warning";
                                CenterText.Foreground = Brushes.Red;
                            });
                            System.Threading.Thread.Sleep(1000 * 2);
                            appDispatcher.DispatchUI(() =>
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
                var textborder = new Border
                {
                    Height = 28,
                    Width = targetwidth,
                    Background = Brushes.ForestGreen,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Child = target
                };
                var animation = new DoubleAnimation
                {
                    From = -targetwidth,
                    To = chaindata.Canvas.ActualWidth,
                    Duration = TimeSpan.FromSeconds(11)
                };

                Storyboard.SetTarget(animation, textborder);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.RightProperty));

                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                _ = chaindata.Canvas.Children.Add(textborder);

                storyboard.Completed += (s, ev) =>
                {
                    chaindata.ActiveAnimations--;
                    appDispatcher.DispatchUI(() =>
                    {
                        chaindata.Canvas.Children.Remove(textborder);
                    });
                    if (chaindata.ActiveAnimations <= 0)
                    {
                        _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            System.Threading.Thread.Sleep(5000);
                            appDispatcher.DispatchUI(() =>
                            {
                                if (chaindata.ActiveAnimations <= 0)
                                {
                                    var rowremoved = Grid.GetRow(chaindata.ChildrenInRow.FirstOrDefault());
                                    Debug.WriteLine($"Removing Row {rowremoved}");
                                    _ = chainDatas.Remove(chaindata);
                                    foreach (var item in chaindata.ChildrenInRow)
                                    {
                                        ChainStackPanel.Children.Remove(item);
                                    }
                                    _ = ChainStackPanel.RowDefinitions.Remove(chaindata.RowDefinition);
                                    foreach (var item in chainDatas)
                                    {
                                        foreach (var cell in item.ChildrenInRow)
                                        {
                                            var itemrow = Grid.GetRow(cell);
                                            if (itemrow > rowremoved)
                                            {
                                                Debug.WriteLine($"Updating Row {itemrow} to {itemrow - 1}");
                                                Grid.SetRow(cell, itemrow - 1);
                                            }
                                        }
                                    }
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
            var chaindata = chainDatas.FirstOrDefault(a => a.TargetName == targetname);
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
            var textborder = new Border
            {
                CornerRadius = new CornerRadius(3),
                Background = Brushes.Chocolate,
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.Black,
                Child = target
            };
            var getrow = ChainStackPanel.RowDefinitions.Count;
            Debug.WriteLine($"Adding row {getrow}");
            Grid.SetRow(textborder, getrow);
            Grid.SetZIndex(textborder, 1);
            Grid.SetColumn(textborder, 0);
            Grid.SetRow(chaindata.Canvas, getrow);
            Grid.SetColumn(chaindata.Canvas, 1);
            Grid.SetZIndex(chaindata.Canvas, 0);
            var stackpanel = new StackPanel { Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 10, 10, 10)), Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 30 };
            Grid.SetRow(stackpanel, getrow);
            Grid.SetColumn(stackpanel, 1);
            ChainStackPanel.RowDefinitions.Add(chaindata.RowDefinition);
            _ = ChainStackPanel.Children.Add(textborder);
            _ = ChainStackPanel.Children.Add(stackpanel);
            _ = ChainStackPanel.Children.Add(chaindata.Canvas);
            chaindata.ChildrenInRow.Add(textborder);
            chaindata.ChildrenInRow.Add(stackpanel);
            chaindata.ChildrenInRow.Add(chaindata.Canvas);
            chainDatas.Add(chaindata);
            chaindata.Canvas.UpdateLayout();
            for (var i = 0; i < 10; i++)
            {
                _ = stackpanel.Children.Add(new Border
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

        protected override void OnClosing(CancelEventArgs e)
        {
            if (logEvents != null)
            {
                logEvents.OverlayEvent -= LogEvents_OverlayEvent;
                logEvents.CompleteHealEvent -= LogParser_CHEvent;
            }

            base.OnClosing(e);
        }

        protected override void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Override default Click-Through logic and do nothing
        }
        
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            WindowResizeChrome.ResizeBorderThickness = new Thickness(8);
            WindowBorder.BorderThickness = new Thickness(1, 1, 1, 1);
            LastWindowInteraction = DateTime.Now;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (Math.Abs((DateTime.Now - LastWindowInteraction).TotalSeconds) < 10)
                {
                    System.Threading.Thread.Sleep(1000 * 1);
                }
                appDispatcher.DispatchUI(() =>
                {
                    WindowResizeChrome.ResizeBorderThickness = new Thickness(0);
                    WindowBorder.BorderThickness = new Thickness(0, 0, 0, 0);
                });
            });
        }
    }
}
