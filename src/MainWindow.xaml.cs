using Autofac;
using EQTool.Models;
using EQTool.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Autofac.IContainer container;
        private readonly System.Windows.Forms.NotifyIcon SystemTrayIcon;
        private SpellWindow spellWindow = null;
        private MapWindow mapwindow = null;
        private DPSMeter dpsmeter = null;
        private Settings settingswindow = null;
        private readonly System.Windows.Forms.MenuItem MapMenuItem;
        private readonly System.Windows.Forms.MenuItem SpellsMenuItem;
        private readonly System.Windows.Forms.MenuItem DpsMeterMenuItem;
        private readonly System.Windows.Forms.MenuItem SettingsMenuItem;

        public MainWindow()
        {
            InitializeComponent();
            container = DI.Init();

            SettingsMenuItem = new System.Windows.Forms.MenuItem("Settings", Settings);
            SpellsMenuItem = new System.Windows.Forms.MenuItem("Spells", Spells);
            MapMenuItem = new System.Windows.Forms.MenuItem("Map", Map);
            DpsMeterMenuItem = new System.Windows.Forms.MenuItem("Dps", DPS);
            var gitHubMenuItem = new System.Windows.Forms.MenuItem("Suggestions", Suggestions);
            SpellsMenuItem.Enabled = false;
            MapMenuItem.Enabled = false;
            DpsMeterMenuItem.Enabled = false;
            if (FindEq.IsValid(EQToolSettings.DefaultEqDirectory))
            {
                var d = new EQToolMessageBox("Configuration", "Project 1999 game files were not able to be found.\nYou must set the path before this program will work!");
                _ = d.ShowDialog();
                FindEQ();
            }
            else if (FindEq.TryCheckLoggingEnabled(EQToolSettings.DefaultEqDirectory) == false)
            {
                var d = new EQToolMessageBox("Configuration", "You must enable Logging before any features will work. This can be done in the settings window!");
                _ = d.ShowDialog();
                Settings(SettingsMenuItem, null);
            }
            else
            {
                SpellsMenuItem.Enabled = MapMenuItem.Enabled = DpsMeterMenuItem.Enabled = true;
            }
#if !DEBUG
            MapMenuItem.Enabled = false;  
#endif 
            SystemTrayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = Properties.Resources.logo,
                Visible = true,
                ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
                 {
                    DpsMeterMenuItem,
                    MapMenuItem,
                    SpellsMenuItem,
                    SettingsMenuItem,
                    gitHubMenuItem,
                    new System.Windows.Forms.MenuItem("Exit", Exit)
                 }),
            };
            Hide();

            Spells(SpellsMenuItem, null);
        }

        private EQToolSettings EQToolSettings => container.Resolve<EQToolSettings>();
        protected override void OnClosing(CancelEventArgs e)
        {
            if (SystemTrayIcon != null)
            {
                SystemTrayIcon.Visible = false;
            }
            container.Resolve<EQToolSettingsLoad>().Save(EQToolSettings);
            base.OnClosing(e);
        }

        private void Suggestions(object sender, EventArgs e)
        {
            _ = System.Windows.MessageBox.Show("Please, post an issue in github if you have any suggetsions or you find any bugs!", "Suggestions", MessageBoxButton.OK, MessageBoxImage.Information);
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/smasherprog/EqTool/issues",
                UseShellExecute = true
            });
        }

        private void Map(object sender, EventArgs e)
        {
#if !DEBUG
            _ = MessageBox.Show("Map is not yet enabled!", "Map", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
#endif
            var s = (System.Windows.Forms.MenuItem)sender;
            s.Checked = !s.Checked;
            if (s.Checked)
            {
                mapwindow?.Close();
                mapwindow = container.Resolve<MapWindow>();
                mapwindow.Closed += (se, ee) => s.Checked = false;
                mapwindow.Show();
            }
            else
            {
                mapwindow?.Close();
                mapwindow = null;
            }
        }

        private void DPS(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            s.Checked = !s.Checked;
            if (s.Checked)
            {
                dpsmeter?.Close();
                dpsmeter = container.Resolve<DPSMeter>();
                dpsmeter.Closed += (se, ee) => s.Checked = false;
                dpsmeter.Show();
            }
            else
            {
                dpsmeter?.Close();
                dpsmeter = null;
            }
        }

        private void Settings(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            s.Checked = !s.Checked;
            if (s.Checked)
            {
                settingswindow?.Close();
                settingswindow = container.Resolve<Settings>();
                settingswindow.Show();
                settingswindow.Closed += (se, ee) =>
                {
                    if (FindEq.IsValid(EQToolSettings.DefaultEqDirectory) && FindEq.TryCheckLoggingEnabled(EQToolSettings.DefaultEqDirectory) == true)
                    {
                        SpellsMenuItem.Enabled = true;
                        MapMenuItem.Enabled = true;
                    }
                    s.Checked = false;
                };
            }
            else
            {
                settingswindow?.Close();
                settingswindow = null;
            }
        }

        private void FindEQ()
        {
            using (var fbd = new FolderBrowserDialog { ShowNewFolderButton = false, Description = "Find eqgame.exe", RootFolder = Environment.SpecialFolder.MyComputer })
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (FindEq.IsValid(fbd.SelectedPath))
                    {
                        EQToolSettings.DefaultEqDirectory = fbd.SelectedPath;
                    }
                    else
                    {
                        var d = new EQToolMessageBox("Find eqgame.exe", "eqgame.exe was not found in this folder! Try again");
                        _ = d.ShowDialog();
                        FindEQ();
                    }
                }
                else
                {
                    var d = new EQToolMessageBox("Find eqgame.exe", "EQGame.exe was not able to be located, closing EQTool!");
                    _ = d.ShowDialog();
                    Close();
                }
            }
        }

        private void Spells(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            s.Checked = !s.Checked;
            if (s.Checked)
            {
                spellWindow?.Close();
                var players = EQToolSettings.Players ?? new System.Collections.Generic.List<PlayerInfo>();
                var directory = new DirectoryInfo(EQToolSettings.DefaultEqDirectory + "/Logs/");
                var loggedincharlogfile = directory.GetFiles()
                    .Where(a => a.Name.StartsWith("eqlog") && a.Name.EndsWith(".txt"))
                    .OrderByDescending(a => a.LastWriteTime)
                    .FirstOrDefault();
                if (loggedincharlogfile != null)
                {
                    var charname = loggedincharlogfile.Name.Replace("eqlog_", string.Empty);
                    var indexpart = charname.IndexOf("_");
                    var charName = charname.Substring(0, indexpart);
                    if (!players.Any(a => a.Name == charName))
                    {
                        var d = new EQToolMessageBox("Configuration", "Please set your characters level in settings otherwise spell timers wont work correctly.");
                        _ = d.ShowDialog();
                        settingswindow?.Close();
                        settingswindow = container.Resolve<Settings>();
                        settingswindow.Show();
                        settingswindow.Closed += (se, ee) =>
                        {
                            if (FindEq.IsValid(EQToolSettings.DefaultEqDirectory))
                            {
                                SpellsMenuItem.Enabled = true;
                                MapMenuItem.Enabled = true;
                            }
                            SettingsMenuItem.Checked = false;
                        };
                    }
                }
                spellWindow = container.Resolve<SpellWindow>();
                spellWindow.Closed += (se, ee) => s.Checked = false;
                spellWindow.Show();
            }
            else
            {
                spellWindow?.Close();
                spellWindow = null;
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
