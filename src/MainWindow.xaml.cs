using Autofac;
using EQTool.Models;
using EQTool.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
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
        private readonly LogParser logParser;

        public MainWindow(bool updated)
        {
            InitializeComponent();

            container = DI.Init();
            SettingsMenuItem = new System.Windows.Forms.MenuItem("Settings", Settings);
            SpellsMenuItem = new System.Windows.Forms.MenuItem("Spells", Spells);
            MapMenuItem = new System.Windows.Forms.MenuItem("Map (ALPHA)", Map);
            DpsMeterMenuItem = new System.Windows.Forms.MenuItem("Dps", DPS);
            var gitHubMenuItem = new System.Windows.Forms.MenuItem("Suggestions", Suggestions);
            var whythepig = new System.Windows.Forms.MenuItem("Why the Pig?", WhyThePig);
            var updates = new System.Windows.Forms.MenuItem("Check for Update", UpdateClicked);
            var versionstring = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var beta = false;

#if Beta || DEBUG
            beta = true;
#endif 
            var logo = Properties.Resources.logo;
            if (beta)
            {
                versionstring = "Beta-" + versionstring;
                logo = Properties.Resources.sickpic;
            }

            var version = new System.Windows.Forms.MenuItem(versionstring, UpdateClicked)
            {
                Enabled = false
            };
            ToggleMenuButtons(false);
            SystemTrayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = logo,
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
            SystemTrayIcon.BalloonTipClicked += UpdateClicked;
            if (!FindEq.IsValid(EQToolSettings.DefaultEqDirectory) || FindEq.TryCheckLoggingEnabled(EQToolSettings.DefaultEqDirectory) == false)
            {
                Settings(SettingsMenuItem, null);
            }
            else
            {
                var eqsettings = container.Resolve<EQToolSettings>();
                App.GlobalFontSize = eqsettings.FontSize;
                App.GlobalTriggerWindowOpacity = eqsettings.GlobalTriggerWindowOpacity;
                App.GlobalDPSWindowOpacity = eqsettings.GlobalDPSWindowOpacity;
                App.Theme = eqsettings.Theme;
                ToggleMenuButtons(true);
                if (EQToolSettings.SpellWindowState == null || !EQToolSettings.SpellWindowState.Closed)
                {
                    Spells(SpellsMenuItem, null);
                }
                if (EQToolSettings.DpsWindowState == null || !EQToolSettings.DpsWindowState.Closed)
                {
                    DPS(DpsMeterMenuItem, null);
                }
            }

            Hide();
            logParser = container.Resolve<LogParser>();
            if (updated)
            {
                SystemTrayIcon.BalloonTipClicked += UpdateNotes;
                SystemTrayIcon.ShowBalloonTip(5000, "PigParse Updated!", "Click here for details!", System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private EQToolSettings EQToolSettings => container.Resolve<EQToolSettings>();
        protected override void OnClosing(CancelEventArgs e)
        {
            SystemTrayIcon.Visible = false;
            SystemTrayIcon.Dispose();
            spellWindow?.Close();
            mapwindow?.Close();
            dpsmeter?.Close();
            settingswindow?.Close();
            container.Resolve<EQToolSettingsLoad>().Save(EQToolSettings);
            base.OnClosing(e);
        }

        private void UpdateNotes(object sender, EventArgs e)
        {
            SystemTrayIcon.BalloonTipClicked -= UpdateNotes;
            var versionstring = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/smasherprog/EqTool/releases/tag/" + versionstring,
                UseShellExecute = true
            });
        }

        private void UpdateClicked(object sender, EventArgs e)
        {
            (App.Current as App).CheckForUpdates();
        }

        private void ToggleMenuButtons(bool value)
        {
            SpellsMenuItem.Enabled = value;
            MapMenuItem.Enabled = value;
            DpsMeterMenuItem.Enabled = value;
        }

        private void WhyThePig(object sender, EventArgs e)
        {
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/nSrz8hAwxM",
                UseShellExecute = true
            });
        }

        private void Suggestions(object sender, EventArgs e)
        {
            _ = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/smasherprog/EqTool/issues",
                UseShellExecute = true
            });
        }

        public void OpenMapWindow()
        {
            if (mapwindow != null)
            {
                _ = mapwindow.Focus();
            }
            else
            {
                Map(MapMenuItem, null);
            }
        }

        private void Map(object sender, EventArgs e)
        {
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

        public void OpenDPSWindow()
        {
            if (dpsmeter != null)
            {
                _ = dpsmeter.Focus();
            }
            else
            {
                DPS(DpsMeterMenuItem, null);
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

        public void OpenSettingsWindow()
        {
            if (settingswindow != null)
            {
                _ = settingswindow.Focus();
            }
            else
            {
                Settings(SettingsMenuItem, null);
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
                        ToggleMenuButtons(true);
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

        public void OpenSpellsWindow()
        {
            if (spellWindow != null)
            {
                _ = spellWindow.Focus();
            }
            else
            {
                Spells(SpellsMenuItem, null);
            }
        }

        private void Spells(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            s.Checked = !s.Checked;
            if (s.Checked)
            {
                spellWindow?.Close();
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
