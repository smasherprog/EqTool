using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EQTool.UI
{
    public partial class DPSMeter : BaseSaveStateWindow
    {
        private readonly System.Timers.Timer UITimer;
        private readonly LogEvents logEvents;
        private readonly DPSWindowViewModel dPSWindowViewModel;
        private readonly ActivePlayer activePlayer;

        public DPSMeter(LogEvents logEvents, DPSWindowViewModel dPSWindowViewModel, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad, LoggingService loggingService, ActivePlayer activePlayer)
             : base(settings.DpsWindowState, toolSettingsLoad, settings)
        {
            this.activePlayer = activePlayer;
            loggingService.Log(string.Empty, EventType.OpenDPS, activePlayer?.Player?.Server);
            this.logEvents = logEvents;
            this.dPSWindowViewModel = dPSWindowViewModel;
            this.dPSWindowViewModel.EntityList = new System.Collections.ObjectModel.ObservableCollection<EntittyDPS>();
            DataContext = dPSWindowViewModel;
            InitializeComponent();
            base.Init();
            this.logEvents.DamageEvent += LogParser_DamageEvent;
            this.logEvents.SlainEvent += LogParser_DeathEvent;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            DpsList.ItemsSource = dPSWindowViewModel.EntityList;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(dPSWindowViewModel.EntityList);
            view.IsLiveGrouping = true;
            view.IsLiveSorting = true;
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(EntittyDPS.TargetName)));
            view.LiveGroupingProperties.Add(nameof(EntittyDPS.TargetName));
            view.SortDescriptions.Add(new SortDescription(nameof(EntittyDPS.TotalDamage), ListSortDirection.Descending));
            view.LiveSortingProperties.Add(nameof(EntittyDPS.TotalDamage));
        }

        private void LogParser_DamageEvent(object sender, DamageEvent e)
        {
            dPSWindowViewModel.TryAdd(e);
        }

        private void LogParser_DeathEvent(object sender, SlainEvent e)
        {
            var zone = activePlayer?.Player?.Zone;
            if (!string.IsNullOrWhiteSpace(zone) && Zones.ZoneInfoMap.TryGetValue(zone, out var fzone))
            {
                if (fzone.NotableNPCs.Any(a => a == e.Victim) && !Zones.KaelFactionMobs.Any(a => a == e.Victim))
                {
                    copytoclipboard(e.Victim);
                }
            }

            dPSWindowViewModel.TargetDied(e.Victim);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer?.Stop();
            UITimer?.Dispose();
            if (logEvents != null)
            {
                logEvents.SlainEvent -= LogParser_DeathEvent;
                logEvents.DamageEvent -= LogParser_DamageEvent;
            }
            base.OnClosing(e);
        }

        private void PollUI(object sender, EventArgs e)
        {
            dPSWindowViewModel.UpdateDPS();
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

            try
            {
                System.Windows.Forms.Clipboard.SetText(fightdetails, System.Windows.Forms.TextDataFormat.Text);
                (App.Current as App).ShowBalloonTip(4000, "DPS Copied", $"DPS for the fight with {name} has been copied to clipboard", System.Windows.Forms.ToolTipIcon.Info);
                Thread.Sleep(10);
            }
            catch (Exception)
            {

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
