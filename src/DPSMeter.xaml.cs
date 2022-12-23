using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.ComponentModel; 
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for DPSMeter.xaml
    /// </summary>
    public partial class DPSMeter : Window
    {
        private readonly System.Timers.Timer UITimer;
        private readonly LogParser logParser;
        private readonly DPSWindowViewModel dPSWindowViewModel;
        private readonly DPSLogParse dPSLogParse;
        private readonly LogDeathParse logDeathParse;
        private readonly EQToolSettings settings;

        public DPSMeter(DPSLogParse dPSLogParse, LogParser logParser, DPSWindowViewModel dPSWindowViewModel, EQToolSettings settings, LogDeathParse logDeathParse)
        {
            this.settings = settings;
            this.logDeathParse = logDeathParse;
            this.dPSLogParse = dPSLogParse;
            this.logParser = logParser;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            this.dPSWindowViewModel = dPSWindowViewModel;
            this.dPSWindowViewModel.EntityList = new System.Collections.ObjectModel.ObservableCollection<EntittyDPS>();
            DataContext = dPSWindowViewModel;
            InitializeComponent(); 
            if (settings.DpsWindowState != null && WindowBounds.isPointVisibleOnAScreen(settings.DpsWindowState.WindowRect))
            {
                this.Left = settings.DpsWindowState.WindowRect.Left;
                this.Top = settings.DpsWindowState.WindowRect.Top;
                this.Height = settings.DpsWindowState.WindowRect.Height;
                this.Width = settings.DpsWindowState.WindowRect.Width;
                this.WindowState = settings.DpsWindowState.State;
            }
            App.GlobalDPSWindowOpacity = settings.GlobalDPSWindowOpacity;
            Topmost = settings.TriggerWindowTopMost;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            DpsList.ItemsSource = dPSWindowViewModel.EntityList;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(dPSWindowViewModel.EntityList);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(EntittyDPS.TargetName)));
            view.LiveGroupingProperties.Add(nameof(EntittyDPS.TargetName));
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription(nameof(EntittyDPS.TargetName), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(EntittyDPS.TotalDamage), ListSortDirection.Descending));
            view.IsLiveSorting = true;
            view.LiveSortingProperties.Add(nameof(EntittyDPS.TotalDamage));
        }

        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = dPSLogParse.Match(e.Line);
            dPSWindowViewModel.TryAdd(matched);
            var targetdead = logDeathParse.GetDeadTarget(e.Line);
            dPSWindowViewModel.TargetDied(targetdead);
        } 

        protected override void OnClosing(CancelEventArgs e)
        {
            if (settings.DpsWindowState == null)
            {
                settings.DpsWindowState = new Models.WindowState();
            }
            settings.DpsWindowState.WindowRect = new Rect
            {
                X = this.Left,
                Y = this.Top,
                Height = this.Height,
                Width = this.Width
            };
            settings.DpsWindowState.State = this.WindowState;
            UITimer.Stop();
            UITimer.Dispose();
            logParser.LineReadEvent += LogParser_LineReadEvent;
            base.OnClosing(e);
        }

        private void PollUI(object sender, EventArgs e)
        {
            dPSWindowViewModel.UpdateDPS();
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Maximized)
            {
                WindowState = System.Windows.WindowState.Normal;
            }
            else
            {
                WindowState = System.Windows.WindowState.Maximized;
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (settings.DpsWindowState == null)
            {
                settings.DpsWindowState = new Models.WindowState();
            }
            settings.DpsWindowState.Closed = true;
            Close();
        }

        private void opendps(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenDPSWIndow();
        }

        private void opensettings(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSettingsWIndow();
        }

        protected bool isPointVisibleOnAScreen(Point p)
        {
            foreach (Screen s in Screen.AllScreens)
            {
                if (p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom)
                    return true;
            }
            return false;
        }
        private void openspells(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSpellsWIndow();
        } 
    }
}
