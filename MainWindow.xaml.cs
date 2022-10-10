using EQTool.Models;
using EQTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly List<Spell> AllSpells = new List<Spell>();
        private readonly System.Windows.Forms.NotifyIcon SystemTrayIcon;
        public MainWindow()
        {
            InitializeComponent();
            if (string.IsNullOrWhiteSpace(FindEq.BestGuessRootEqPath))
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
                    new System.Windows.Forms.MenuItem("Spells", Spells),
                    new System.Windows.Forms.MenuItem("Exit", Exit)
                }),
            };
            var spells = ParseSpells.GetSpells();
            var spellicons = SpellIcons.GetSpellIcons();
            var spelllist = new ObservableCollection<Spell>();

            foreach (var item in spells)
            {
                var mappedspell = item.Map(spellicons);
                AllSpells.Add(mappedspell);
            }

            spelllistview.ItemsSource = spelllist;
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

        private void Spells(object sender, EventArgs e)
        {
            var s = (System.Windows.Forms.MenuItem)sender;
            s.Checked = !s.Checked;
            if (s.Checked)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
