using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace EQTool
{
    public partial class SpellWindow : Window
    {
        private readonly System.Timers.Timer UITimer;

        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly LogParser logParser;
        private readonly SpellLogParse spellLogParse;
        private readonly LogDeathParse logDeathParse;
        private readonly LogCustomTimer logCustomTimer;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly SpellWornOffLogParse spellWornOffLogParse;

        public SpellWindow(SpellWornOffLogParse spellWornOffLogParse, EQToolSettings settings, SpellWindowViewModel spellWindowViewModel, LogParser logParser, SpellLogParse spellLogParse, LogDeathParse logDeathParse, LogCustomTimer logCustomTimer, EQToolSettingsLoad toolSettingsLoad)
        {
            this.spellWornOffLogParse = spellWornOffLogParse;
            this.settings = settings;
            this.logCustomTimer = logCustomTimer;
            this.logDeathParse = logDeathParse;
            this.logParser = logParser;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            this.logParser.PlayerChangeEvent += LogParser_PlayerChangeEvent;
            this.spellLogParse = spellLogParse;
            spellWindowViewModel.SpellList = new System.Collections.ObjectModel.ObservableCollection<UISpell>();
            DataContext = this.spellWindowViewModel = spellWindowViewModel;
            Topmost = settings.TriggerWindowTopMost;
            if (settings.SpellWindowState != null && WindowBounds.isPointVisibleOnAScreen(settings.SpellWindowState.WindowRect))
            {
                Left = settings.SpellWindowState.WindowRect.Left;
                Top = settings.SpellWindowState.WindowRect.Top;
                Height = settings.SpellWindowState.WindowRect.Height;
                Width = settings.SpellWindowState.WindowRect.Width;
                WindowState = settings.SpellWindowState.State;
            }
            InitializeComponent();

            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += PollUI;
            UITimer.Enabled = true;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(spelllistview.ItemsSource);
            Properties.Settings.Default.GlobalTriggerWindowOpacity = settings.GlobalTriggerWindowOpacity;
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(UISpell.TargetName)));
            view.LiveGroupingProperties.Add(nameof(UISpell.TargetName));
            view.IsLiveGrouping = true;
            view.SortDescriptions.Add(new SortDescription(nameof(UISpell.TargetName), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(UISpell.SecondsLeftOnSpell), ListSortDirection.Descending));
            view.IsLiveSorting = true;
            view.LiveSortingProperties.Add(nameof(UISpell.SecondsLeftOnSpell));
            this.toolSettingsLoad = toolSettingsLoad;
            if (settings.SpellWindowState != null)
            {
                settings.SpellWindowState.Closed = false;
            }
            SaveState();
            SizeChanged += DPSMeter_SizeChanged;
            StateChanged += SpellWindow_StateChanged;
            LocationChanged += DPSMeter_LocationChanged;
        }

        private void SpellWindow_StateChanged(object sender, EventArgs e)
        {
            SaveState();
        }

        private void DPSMeter_LocationChanged(object sender, EventArgs e)
        {
            SaveState();
        }

        private void DPSMeter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveState();
        }

        private void LogParser_PlayerChangeEvent(object sender, LogParser.PlayerChangeEventArgs e)
        {
            spellWindowViewModel.ClearAllSpells();
        }

        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = spellLogParse.MatchSpell(e.Line);
            spellWindowViewModel.TryAdd(matched);

            var targettoremove = logDeathParse.GetDeadTarget(e.Line);
            spellWindowViewModel.TryRemoveTarget(targettoremove);

            var customtimer = logCustomTimer.GetStartTimer(e.Line);
            spellWindowViewModel.TryAddCustom(customtimer);

            var canceltimer = logCustomTimer.GetCancelTimer(e.Line);
            spellWindowViewModel.TryRemoveCustom(canceltimer);

            var spelltoremove = spellWornOffLogParse.MatchSpell(e.Line);
            spellWindowViewModel.TryRemoveUnambiguousSpell(spelltoremove);
        }

        private void SaveState()
        {
            if (settings.SpellWindowState == null)
            {
                settings.SpellWindowState = new Models.WindowState();
            }
            settings.SpellWindowState.WindowRect = new Rect
            {
                X = Left,
                Y = Top,
                Height = Height,
                Width = Width
            };
            settings.SpellWindowState.State = WindowState;
            toolSettingsLoad.Save(settings);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer.Stop();
            UITimer.Dispose();
            SizeChanged -= DPSMeter_SizeChanged;
            StateChanged -= SpellWindow_StateChanged;
            LocationChanged -= DPSMeter_LocationChanged;
            logParser.LineReadEvent -= LogParser_LineReadEvent;
            logParser.PlayerChangeEvent -= LogParser_PlayerChangeEvent;
            base.OnClosing(e);
        }

        private void PollUI(object sender, EventArgs e)
        {
            spellWindowViewModel.UpdateSpells();
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
            if (settings.SpellWindowState == null)
            {
                settings.SpellWindowState = new Models.WindowState();
            }
            settings.SpellWindowState.Closed = true;
            SaveState();
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
        private void openmobinfo(object sender, RoutedEventArgs e)
        {
            (App.Current as App).OpenMobInfoWindow();
        }

    }
}
