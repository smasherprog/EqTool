using EQTool.Services;
using EQTool.Services.Map;
using EQTool.ViewModels;
using HelixToolkit.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        private readonly Timer UITimer;
        private readonly LogParser logParser;
        private readonly MapViewModel mapViewModel; 
        private readonly LocationParser locationParser;
        private readonly ZoneParser zoneParser;

        public MapWindow(ZoneParser zoneParser, MapViewModel mapViewModel, LocationParser locationParser,  LogParser logParser)
        {
            this.zoneParser = zoneParser;
            this.locationParser = locationParser;   
            this.logParser = logParser; 
            DataContext = this.mapViewModel = mapViewModel;
            Topmost = true;
            InitializeComponent();
            this.mapViewModel.LoadDefaultMap();
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            UITimer = new System.Timers.Timer(1000);
            UITimer.Elapsed += UITimer_Elapsed;
            UITimer.Enabled = true;   
        }

        private void UITimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mapViewModel.Update();
        }
          
        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var pos = locationParser.Match(e.Line);
            if (pos.HasValue)
            {
                mapViewModel.UpdateLocation(pos.Value); 
            }
            else
            { 
                var matched = zoneParser.Match(e.Line);
                mapViewModel.LoadMap(matched); 
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            logParser.LineReadEvent += LogParser_LineReadEvent;
            base.OnClosing(e);
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
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
