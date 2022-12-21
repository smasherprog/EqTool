using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Map;
using EQTool.Services.Spells.Log;
using EQTool.Services.WLD;
using EQTool.ViewModels;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static EQTool.Services.MapLoad;
using static System.Windows.Forms.LinkLabel;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        private readonly ZoneParser zoneParser;
        private readonly LogParser logParser;
        private readonly MapLoad mapLoad;
        private readonly ActivePlayer activePlayer;

        public MapWindow(ZoneParser zone, LogParser logParser, MapLoad mapLoad, ActivePlayer activePlayer)
        {
            this.zoneParser = zone;
            this.logParser = logParser;
            this.mapLoad = mapLoad;
            this.activePlayer = activePlayer;
            InitializeComponent();
            viewport3d.ShowCameraInfo = true; 
            var map = mapLoad.Load(zoneParser.TranslateToMapName(this.activePlayer.Player?.Zone));
            addmap(map);
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
        }

        private void addmap(MapDetails map)
        {
            if (map.Labels.Any() || map.Lines.Any())
            {
                viewport3d.Children.Clear();
                viewport3d.Children.Add(new DefaultLights()); 
                foreach (var item in map.Lines)
                { 
                    viewport3d.Children.Add(item);
                }

                foreach (var item in map.Labels)
                {
                    viewport3d.Children.Add(item);
                }
                var center = map.AABB.Center;
                center.Z = map.AABB.MaxHeight;
                viewport3d.Camera.Position = center;
            }
        }
        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = zoneParser.Match(e.Line);
            var map = mapLoad.Load(matched);
            if (map.Labels.Any())
            {
                if (this.activePlayer?.Player?.Zone != null)
                {
                    this.activePlayer.Player.Zone = matched;
                }
            }
            addmap(map);
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
