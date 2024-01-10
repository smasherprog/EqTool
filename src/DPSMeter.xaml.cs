using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared.Enums;
using EQToolShared.Map;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

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
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly ActivePlayer activePlayer;
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500),
            IsEnabled = false
        };

        public DPSMeter(LogParser logParser, DPSWindowViewModel dPSWindowViewModel, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad, LoggingService loggingService, ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
            loggingService.Log(string.Empty, EventType.OpenDPS, activePlayer?.Player?.Server);
            this.settings = settings;
            this.logParser = logParser;
            this.logParser.FightHitEvent += LogParser_FightHitEvent;
            this.logParser.DeadEvent += LogParser_DeadEvent;
            this.dPSWindowViewModel = dPSWindowViewModel;
            this.dPSWindowViewModel.EntityList = new System.Collections.ObjectModel.ObservableCollection<EntittyDPS>();
            DataContext = dPSWindowViewModel;
            InitializeComponent();
            WindowExtensions.AdjustWindow(settings.DpsWindowState, this);
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
            this.toolSettingsLoad = toolSettingsLoad;
            timer.Tick += timer_Tick;
            SizeChanged += DPSMeter_SizeChanged;
            StateChanged += SpellWindow_StateChanged;
            LocationChanged += DPSMeter_LocationChanged;
            settings.DpsWindowState.Closed = false;
        }

        private void LogParser_FightHitEvent(object sender, LogParser.FightHitEventArgs e)
        {
            dPSWindowViewModel.TryAdd(e.HitInformation);
        }

        private void LogParser_DeadEvent(object sender, LogParser.DeadEventArgs e)
        {
            var zone = this.activePlayer?.Player?.Zone;
            if (!string.IsNullOrWhiteSpace(zone) && ZoneParser.ZoneInfoMap.TryGetValue(zone, out var fzone))
            {
                if (fzone.NotableNPCs.Any(a => a == e.Name))
                {
                    this.copytoclipboard(e.Name);
                }
            }

            dPSWindowViewModel.TargetDied(e.Name);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer?.Stop();
            UITimer?.Dispose();
            SizeChanged -= DPSMeter_SizeChanged;
            StateChanged -= SpellWindow_StateChanged;
            LocationChanged -= DPSMeter_LocationChanged;
            if (this.logParser != null)
            {
                logParser.DeadEvent -= LogParser_DeadEvent;
                logParser.FightHitEvent -= LogParser_FightHitEvent;
            }
            base.OnClosing(e);
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

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            settings.DpsWindowState.Closed = true;
            SaveState();
            Close();
        }

        private void SaveState()
        {
            Debug.WriteLine("Saving DPS window State");
            WindowExtensions.SaveWindowState(settings.DpsWindowState, this);
            toolSettingsLoad.Save(settings);
        }

        private void SpellWindow_StateChanged(object sender, EventArgs e)
        {
            DebounceSave();
        }

        private void DPSMeter_LocationChanged(object sender, EventArgs e)
        {
            DebounceSave();
        }

        private void DPSMeter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DebounceSave();
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

        private void openmobinfo(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenMobInfoWindow();
        }

        private void copytoclipboard(object sender, RoutedEventArgs e)
        {
            var name = ((sender as Button).DataContext as dynamic)?.Name as string;
            copytoclipboard(name);
        }

        private void copytoclipboard(string name)
        {
            var items = dPSWindowViewModel.EntityList.Where(a => a.TargetName == name);
            var fights = new List<string>();
            foreach (var item in items.OrderByDescending(a => a.TotalDamage))
            {
                fights.Add($"{item.SourceName} {item.PercentOfTotalDamage}% DPS:{item.TotalDPS} DMG:{item.TotalDamage}");
            }
            var fightdetails = "Fight Details: " + name + " Dmg: " + (items.FirstOrDefault()?.TargetTotalDamage ?? 0) + "    " + string.Join(" / ", fights);
            for (var i = 0; i < 2; i++)
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(fightdetails, System.Windows.Forms.TextDataFormat.Text);
                    (App.Current as App).ShowBalloonTip(4000, "DPS Copied", $"DPS for the fight with {name} has been copied to clipboard", System.Windows.Forms.ToolTipIcon.Info);
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    if (i == 1)
                    {
                        throw;
                    }
                }
            }
        }

        private void MoveCurrentToLastSession(object sender, RoutedEventArgs e)
        {
            dPSWindowViewModel.SessionPlayerDamage.LastSessionPlayerDamage = dPSWindowViewModel.SessionPlayerDamage.CurrentSessionPlayerDamage;
            dPSWindowViewModel.SessionPlayerDamage.CurrentSessionPlayerDamage = new PlayerDamage();
        }

        private void RemoveLastSession(object sender, RoutedEventArgs e)
        {
            dPSWindowViewModel.SessionPlayerDamage.LastSessionPlayerDamage = null;
        }

    }
}
