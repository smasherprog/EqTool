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
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
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
        private readonly HelixViewport3D helixViewport3D;
        private readonly ActivePlayer activePlayer;

        public MapWindow(ZoneParser zone, LogParser logParser, MapLoad mapLoad, ActivePlayer activePlayer)
        {
            this.zoneParser = zone;
            this.logParser = logParser;
            this.mapLoad = mapLoad;
            this.activePlayer = activePlayer;
            this.logParser.LineReadEvent += LogParser_LineReadEvent;
            helixViewport3D = new HelixViewport3D();
    
            helixViewport3D.ShowCameraInfo = true;
            helixViewport3D.Children.Add(new DefaultLights());
            this.AddChild(helixViewport3D);
            InitializeComponent();
            var points = mapLoad.Load(zoneParser.TranslateToMapName(this.activePlayer.Player?.Zone));
            foreach (var item in points.Lines)
            {
                helixViewport3D.Children.Add(item);
            }
            foreach (var item in points.Labels)
            {
                helixViewport3D.Children.Add(item);
            }
        }

        private void LogParser_LineReadEvent(object sender, LogParser.LogParserEventArgs e)
        {
            var matched = zoneParser.Match(e.Line);
            var points = mapLoad.Load(matched);
            if (points.Labels.Any())
            {
                helixViewport3D.Children.Clear();
                helixViewport3D.Children.Add(new DefaultLights());
                if (this.activePlayer?.Player?.Zone != null)
                {
                    this.activePlayer.Player.Zone = matched;
                }

                foreach (var item in points.Lines)
                {
                    helixViewport3D.Children.Add(item);
                }
                
                foreach (var item in points.Labels)
                {
                    helixViewport3D.Children.Add(item);
                }
                helixViewport3D.Camera.Position = new Point3D { X = 0, Y = 4000, Z = 0 };
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            logParser.LineReadEvent += LogParser_LineReadEvent;
            base.OnClosing(e);
        }
    }
}
