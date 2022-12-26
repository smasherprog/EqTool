using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
                Left = settings.DpsWindowState.WindowRect.Left;
                Top = settings.DpsWindowState.WindowRect.Top;
                Height = settings.DpsWindowState.WindowRect.Height;
                Width = settings.DpsWindowState.WindowRect.Width;
                WindowState = settings.DpsWindowState.State;
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
                X = Left,
                Y = Top,
                Height = Height,
                Width = Width
            };
            settings.DpsWindowState.State = WindowState;
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
            WindowState = WindowState == System.Windows.WindowState.Maximized ? System.Windows.WindowState.Normal : System.Windows.WindowState.Maximized;
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
            (App.Current as App).OpenDPSWindow();
        }

        private void opensettings(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSettingsWindow();
        }

        private void openmap(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenMapWindow();
        }

        private void openspells(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSpellsWindow();
        }

        private void copytoclipboard(object sender, RoutedEventArgs e)
        {
            var name = ((sender as Button).DataContext as dynamic)?.Name as string;

            var items = dPSWindowViewModel.EntityList.Where(a => a.TargetName == name);
            var fights = new List<string>();
            foreach (var item in items)
            {
                fights.Add($"{item.SourceName} {item.PercentOfTotalDamage}% DPS:{item.TotalDPS} DMG:{item.TotalDamage}");
            }
            var fightdetails = "Fight Details Against " + name + "   " + string.Join(" / ", fights);
            System.Windows.Forms.Clipboard.SetText(fightdetails);
        }
    }
}
