using EQTool.Models;
using EQTool.Services;
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
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;


        public SpellWindow(EQToolSettings settings, SpellWindowViewModel spellWindowViewModel, LogParser logParser, EQToolSettingsLoad toolSettingsLoad)
        {
            this.settings = settings;
            this.logParser = logParser;
            this.logParser.SpellWornsOffEvent += LogParser_SpellWornsOffEvent;
            this.logParser.StartCastingEvent += LogParser_StartCastingEvent;
            this.logParser.DeadEvent += LogParser_DeadEvent;
            this.logParser.StartTimerEvent += LogParser_StartTimerEvent;
            this.logParser.CancelTimerEvent += LogParser_CancelTimerEvent;
            this.logParser.PlayerChangeEvent += LogParser_PlayerChangeEvent;
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

        private void LogParser_SpellWornsOffEvent(object sender, LogParser.StartWornsOffEventArgs e)
        {
            spellWindowViewModel.TryRemoveUnambiguousSpell(e.SpellName);
        }

        private void LogParser_StartCastingEvent(object sender, LogParser.StartCastingEventArgs e)
        {
            spellWindowViewModel.TryAdd(e.Spell);
        }

        private void LogParser_DeadEvent(object sender, LogParser.DeadEventArgs e)
        {
            spellWindowViewModel.TryRemoveTarget(e.Name);
        }

        private void LogParser_CancelTimerEvent(object sender, LogParser.CancelTimerEventArgs e)
        {
            spellWindowViewModel.TryRemoveCustom(e.Name);
        }

        private void LogParser_StartTimerEvent(object sender, LogParser.StartTimerEventArgs e)
        {
            spellWindowViewModel.TryAddCustom(e.CustomerTimer);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UITimer.Stop();
            UITimer.Dispose();
            SizeChanged -= DPSMeter_SizeChanged;
            StateChanged -= SpellWindow_StateChanged;
            LocationChanged -= DPSMeter_LocationChanged;
            logParser.SpellWornsOffEvent -= LogParser_SpellWornsOffEvent;
            logParser.StartCastingEvent -= LogParser_StartCastingEvent;
            logParser.DeadEvent -= LogParser_DeadEvent;
            logParser.StartTimerEvent -= LogParser_StartTimerEvent;
            logParser.CancelTimerEvent -= LogParser_CancelTimerEvent;
            logParser.PlayerChangeEvent -= LogParser_PlayerChangeEvent;
            base.OnClosing(e);
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
