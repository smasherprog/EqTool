using EQTool.Models;
using EQTool.Services;
using EQTool.Services.P99LoginMiddlemand;
using EQTool.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace EQTool.UI.SettingsComponents
{
    public partial class SettingsGeneral : UserControl
    {
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly SettingsWindowViewModel SettingsWindowData;
        private readonly LoginMiddlemand loginMiddlemand;
        private readonly IAppDispatcher appDispatcher;

        public SettingsGeneral(EQToolSettings settings,
        EQToolSettingsLoad toolSettingsLoad,
        SettingsWindowViewModel SettingsWindowData,
          IAppDispatcher appDispatcher,
        LoginMiddlemand loginMiddlemand)
        {
            this.appDispatcher = appDispatcher;
            this.settings = settings;
            this.toolSettingsLoad = toolSettingsLoad;
            this.SettingsWindowData = SettingsWindowData;
            this.loginMiddlemand = loginMiddlemand;
            InitializeComponent();
        }
        private void TryUpdateSettings()
        {
            var logfounddata = FindEq.GetLogFileLocation(new FindEq.FindEQData { EqBaseLocation = settings.DefaultEqDirectory, EQlogLocation = string.Empty });
            if (logfounddata?.Found == true)
            {
                settings.EqLogDirectory = logfounddata.Location;
                SettingsWindowData.EqLogPath = logfounddata.Location;
            }
            SettingsWindowData.Update();

            LoginMiddleMandCheckBox.IsChecked = loginMiddlemand.Running;

            var player = SettingsWindowData.ActivePlayer.Player;

            if (player?.ShowSpellsForClasses != null)
            {
                foreach (var item in SettingsWindowData.SelectedPlayerClasses)
                {
                    item.IsChecked = player.ShowSpellsForClasses.Contains(item.TheValue);
                }
            }
            else
            {
                foreach (var item in SettingsWindowData.SelectedPlayerClasses)
                {
                    item.IsChecked = false;
                }
            }
            var hasvalideqdir = FindEq.IsValidEqFolder(settings.DefaultEqDirectory);
            if (hasvalideqdir && FindEq.TryCheckLoggingEnabled(settings.DefaultEqDirectory) == true)
            {
                ((App)System.Windows.Application.Current).ToggleMenuButtons(true);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        private bool IsEqRunning()
        {
            return Process.GetProcessesByName("eqgame").Length > 0;
        }

        private void TryCheckLoggingEnabled()
        {
            SettingsWindowData.IsLoggingEnabled = FindEq.TryCheckLoggingEnabled(settings.DefaultEqDirectory) ?? false;
        }
        private void SaveConfig()
        {
            toolSettingsLoad.Save(settings);
        }

        private void fontsizescombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SaveConfig();
        }

        private void EqFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            var descriptiontext = "Select Project 1999 EQ Directory";
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog() { Description = descriptiontext, ShowNewFolderButton = false })
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (FindEq.IsValidEqFolder(fbd.SelectedPath))
                    {
                        SettingsWindowData.EqPath = settings.DefaultEqDirectory = fbd.SelectedPath;
                        appDispatcher.DispatchUI(() =>
                        {
                            TryUpdateSettings();
                            TryCheckLoggingEnabled();
                        });
                    }
                    else
                    {
                        _ = System.Windows.Forms.MessageBox.Show("eqgame.exe was not found in this folder. Make sure this is a valid Folder!", "Message");
                    }
                }
            }
        }

        private void enableLogging_Click(object sender, RoutedEventArgs e)
        {
            if (IsEqRunning())
            {
                _ = System.Windows.MessageBox.Show("You must exit EQ before you can enable Logging!", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                if (settings.DefaultEqDirectory.ToLower().Contains("program files"))
                {
                    _ = System.Windows.MessageBox.Show("Everquest is installed in program files. YOU MUST ADD LOG=TRUE to the eqclient.ini yourself.", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    var data = File.ReadAllLines(settings.DefaultEqDirectory + "/eqclient.ini");
                    var newlist = new List<string>();
                    foreach (var item in data)
                    {
                        var line = item.ToLower().Trim().Replace(" ", string.Empty);
                        if (line.StartsWith("log="))
                        {
                            newlist.Add("Log=TRUE");
                        }
                        else
                        {
                            newlist.Add(item);
                        }
                    }
                    File.WriteAllLines(settings.DefaultEqDirectory + "/eqclient.ini", newlist);
                }
            }
            catch { }
            TryUpdateSettings();
            TryCheckLoggingEnabled();
        }

        private void LoginMiddleMandToggle(object sender, RoutedEventArgs e)
        {
            var s = sender as System.Windows.Controls.CheckBox;
            if (s.IsChecked == true)
            {
                if (loginMiddlemand.IsConfiguredCorrectly())
                {
                    loginMiddlemand.StartListening();
                    settings.LoginMiddleMand = true;
                    SaveConfig();
                }
                else
                {
                    var result = System.Windows.MessageBox.Show("Your eqhost.txt file is not setup correctly to use LoginMiddlemand. Would you like to use default settings?", "LoginMiddlemand Configuration", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        loginMiddlemand.ApplyDefaultConfiguration();
                        settings.LoginMiddleMand = true;
                        SaveConfig();

                        if (IsEqRunning())
                        {
                            _ = System.Windows.MessageBox.Show("You must exit EQ before you can start using LoginMiddlemand! Please close Everquest and restart it!", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        loginMiddlemand.StartListening();
                    }
                    else
                    {
                        s.IsChecked = false;
                    }
                }
            }
            else
            {
                settings.LoginMiddleMand = false;
                SaveConfig();
                var result = System.Windows.MessageBox.Show("Would you like your eqhosts.txt file to be reverted to use eqemulator default login server?", "Revert LoginMiddlemand Configuration", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    loginMiddlemand.RevertDefaultConfiguration();
                    if (IsEqRunning())
                    {
                        _ = System.Windows.MessageBox.Show("You must exit EQ before Everquest will use these settings! Please close Everquest and restart it!", "Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                loginMiddlemand.StopListening();
            }
        }

        private void Savesettings(object sender, RoutedEventArgs e)
        {
            SaveConfig();
        }

    }
}
