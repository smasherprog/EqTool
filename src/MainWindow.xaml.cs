using Autofac;
using EQTool.Models;
using EQTool.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Windows;

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
        private readonly HttpClient httpclient = new HttpClient();

        public MainWindow()
        {
            httpclient.DefaultRequestHeaders.Add("User-Agent", "request");
            InitializeComponent();
            container = DI.Init();

            SettingsMenuItem = new System.Windows.Forms.MenuItem("Settings", Settings);
            SpellsMenuItem = new System.Windows.Forms.MenuItem("Spells", Spells);
            MapMenuItem = new System.Windows.Forms.MenuItem("Map", Map);
            DpsMeterMenuItem = new System.Windows.Forms.MenuItem("Dps", DPS);
            var gitHubMenuItem = new System.Windows.Forms.MenuItem("Suggestions", Suggestions);
            var whythepig = new System.Windows.Forms.MenuItem("Why the Pig?", WhyThePig);
            var updates = new System.Windows.Forms.MenuItem("Updates", UpdateClicked);
            var versionstring = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var version = new System.Windows.Forms.MenuItem(versionstring, UpdateClicked)
            {
                Enabled = false
            };
            SpellsMenuItem.Enabled = false;
            MapMenuItem.Enabled = false;
            DpsMeterMenuItem.Enabled = false;
            SystemTrayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = Properties.Resources.logo,
                Visible = true,
                ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
                 {
                     whythepig,
                    DpsMeterMenuItem,
                    MapMenuItem,
                    SpellsMenuItem,
                    SettingsMenuItem,
                    gitHubMenuItem,
                    updates,
                    version,
                    new System.Windows.Forms.MenuItem("Exit", Exit)
                 }),
            };

            if (!FindEq.IsValid(EQToolSettings.DefaultEqDirectory) || FindEq.TryCheckLoggingEnabled(EQToolSettings.DefaultEqDirectory) == false)
            {
                Settings(SettingsMenuItem, null);
            }
            else
            {
                SpellsMenuItem.Enabled = MapMenuItem.Enabled = DpsMeterMenuItem.Enabled = true;
                Spells(SpellsMenuItem, null);
                DPS(DpsMeterMenuItem, null);
            }

            CheckForNewUpdates();
            Hide();
#if !DEBUG
            MapMenuItem.Enabled = false;  
#endif 
        }

        private EQToolSettings EQToolSettings => container.Resolve<EQToolSettings>();
        protected override void OnClosing(CancelEventArgs e)
        {
            SystemTrayIcon.Visible = false;
            SystemTrayIcon.Dispose();
            container.Resolve<EQToolSettingsLoad>().Save(EQToolSettings);
            base.OnClosing(e);
        }

        public class GithubAsset
        {
            public string browser_download_url { get; set; }
        }

        public class GithubVersionInfo
        {
            public List<GithubAsset> assets { get; set; }

            public string tag_name { get; set; }
        }

        private void CheckForNewUpdates()
        {
            var json = httpclient.GetAsync(new Uri("https://api.github.com/repos/smasherprog/EqTool/releases/latest")).Result.Content.ReadAsStringAsync().Result;
            var githubdata = JsonConvert.DeserializeObject<GithubVersionInfo>(json);
            var url = githubdata.assets.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.browser_download_url))?.browser_download_url;
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (version != githubdata.tag_name)
            {
                SystemTrayIcon.Text = $"A new version '{version}' of EQTool is available!";
                SystemTrayIcon.BalloonTipTitle = $"A new version of EQTool is available!";
                SystemTrayIcon.BalloonTipText = $"A new version '{version}' of EQTool is available!";
                SystemTrayIcon.BalloonTipClicked += UpdateClicked;
                SystemTrayIcon.ShowBalloonTip(2000);
            }
        }

        private void UpdateClicked(object sender, EventArgs e)
        {
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/smasherprog/EqTool/releases/latest",
                UseShellExecute = true
            });
        }

        private void WhyThePig(object sender, EventArgs e)
        {
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/xgMreRqK",
                UseShellExecute = true
            });
        }

        private void Suggestions(object sender, EventArgs e)
        {
            _ = System.Windows.MessageBox.Show("Please, post an issue in github if you have any suggestions or you find any bugs!", "Suggestions", MessageBoxButton.OK, MessageBoxImage.Information);
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/smasherprog/EqTool/issues",
                UseShellExecute = true
            });
        }

        private void Map(object sender, EventArgs e)
        {
#if !DEBUG
            _ = System.Windows.MessageBox.Show("Map is not yet enabled!", "Map", MessageBoxButton.OK, MessageBoxImage.Information);
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
                                DpsMeterMenuItem.Enabled = true;
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
