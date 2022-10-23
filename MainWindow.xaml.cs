using EQTool.Services;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly System.Windows.Forms.NotifyIcon SystemTrayIcon;
        private SpellWindow spellWindow = null;
        private MapWindow mapwindow = null;
        private Settings settingswindow = null;
        private readonly System.Windows.Forms.MenuItem MapMenuItem;
        private readonly System.Windows.Forms.MenuItem SpellsMenuItem;

        public MainWindow()
        {
            InitializeComponent();
            App.GlobalTriggerWindowOpacity = Properties.Settings.Default.GlobalTriggerWindowOpacity = Properties.Settings.Default.GlobalTriggerWindowOpacity;
            if (!FindEq.IsValid(Properties.Settings.Default.DefaultEqDirectory))
            {
                Properties.Settings.Default.DefaultEqDirectory = FindEq.LoadEQPath();
            }

            Properties.Settings.Default.FontSize = Properties.Settings.Default.FontSize;
            Properties.Settings.Default.Save();

            var settingsbutton = new System.Windows.Forms.MenuItem("Settings", Settings);
            SpellsMenuItem = new System.Windows.Forms.MenuItem("Spells", Spells);
            MapMenuItem = new System.Windows.Forms.MenuItem("Map", Map);

            if (!FindEq.IsValid(Properties.Settings.Default.DefaultEqDirectory))
            {
                SpellsMenuItem.Enabled = false;
                MapMenuItem.Enabled = false;
                _ = MessageBox.Show("Project 1999 game files were not able to be found.\nYou must set the path before this program will work!", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                settingswindow = new Settings();
                settingswindow.Show();
                settingsbutton.Checked = true;
                settingswindow.Closed += (se, ee) =>
                {
                    if (FindEq.IsValid(Properties.Settings.Default.DefaultEqDirectory))
                    {
                        SpellsMenuItem.Enabled = true;
                        MapMenuItem.Enabled = true;
                    }
                    settingsbutton.Checked = false;
                };
            }


            SystemTrayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/eqicon.ico")).Stream),
                Visible = true,
                ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
                 {
                    MapMenuItem,
                    SpellsMenuItem,
                    settingsbutton                 ,
                    new System.Windows.Forms.MenuItem("Exit", Exit)
                 }),
            };
            Hide();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (SystemTrayIcon != null)
            {
                SystemTrayIcon.Visible = false;
            }
            base.OnClosing(e);
        }

        private void Map(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            s.Checked = !s.Checked;
            if (s.Checked)
            {
                mapwindow?.Close();
                mapwindow = new MapWindow();
                mapwindow.Closed += (se, ee) => s.Checked = false;
                mapwindow.Show();
            }
            else
            {
                mapwindow?.Close();
                mapwindow = null;
            }
        }

        private void Settings(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            s.Checked = !s.Checked;
            if (s.Checked)
            {
                settingswindow?.Close();
                settingswindow = new Settings();
                settingswindow.Show();
                settingswindow.Closed += (se, ee) =>
                {
                    if (FindEq.IsValid(Properties.Settings.Default.DefaultEqDirectory))
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
                spellWindow = new SpellWindow();
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
