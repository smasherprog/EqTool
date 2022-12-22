using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for DPSMeter.xaml
    /// </summary>
    public partial class DPSMeter : Window
    {
        private readonly Timer UITimer;
        private readonly LogParser logParser;
        private readonly DPSWindowViewModel dPSWindowViewModel;
        private readonly DPSLogParse dPSLogParse;
        private readonly LogDeathParse logDeathParse;

        public DPSMeter(DPSLogParse dPSLogParse, LogParser logParser, DPSWindowViewModel dPSWindowViewModel, EQToolSettings settings, LogDeathParse logDeathParse)
        {
            this.logDeathParse = logDeathParse;
            this.dPSLogParse = dPSLogParse;
            this.logParser = logParser;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            this.dPSWindowViewModel = dPSWindowViewModel;
            this.dPSWindowViewModel.EntityList = new System.Collections.ObjectModel.ObservableCollection<EntittyDPS>();
            DataContext = dPSWindowViewModel;
            InitializeComponent();
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

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer.Stop();
            UITimer.Dispose();
            logParser.LineReadEvent += LogParser_LineReadEvent;
            base.OnClosing(e);
        }

        private void PollUI(object sender, EventArgs e)
        {
            dPSWindowViewModel.UpdateDPS();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void openspells(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSpellsWIndow();
        }
        private void opensettings(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenSettingsWIndow();
        }
    }
}
