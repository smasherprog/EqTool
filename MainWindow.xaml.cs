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

        public MainWindow()
        {
            InitializeComponent();
            Models.EqToolSettings.BestGuessRootEqPath = FindEq.LoadEQPath();
            if (string.IsNullOrWhiteSpace(Models.EqToolSettings.BestGuessRootEqPath))
            {
                _ = MessageBox.Show("Project 1999 game files were not able to be found.\nProject 1999 files must be installed in no deeper than 3 levels. \n\nGOOD c:/program files/everquest/eqgame.exe will be found\nBAD c:/program files/everquest/level/eqgame.exe", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                System.Windows.Application.Current.Shutdown();
                return;
            }

            SystemTrayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/eqicon.ico")).Stream),
                Visible = true,
                ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
                {
                    new System.Windows.Forms.MenuItem("Map", Map),
                    new System.Windows.Forms.MenuItem("Spells", Spells),
                    new System.Windows.Forms.MenuItem("Settings", Setings),
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
                mapwindow.Show();
            }
            else
            {
                mapwindow?.Close();
                mapwindow = null;
            }
        }

        private void Setings(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            s.Checked = !s.Checked;
            if (s.Checked)
            {
                settingswindow?.Close();
                settingswindow = new Settings();
                settingswindow.Show();
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
