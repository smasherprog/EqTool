﻿using ControlzEx.Theming;
using MahApps.Metro.Theming;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EQTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly HttpClient httpclient = new HttpClient();
        private MainWindow mainWindow;  

        public static double GlobalFontSize
        {
            get => (double)Current.Resources["GlobalFontSize"];
            set => Current.Resources["GlobalFontSize"] = value;
        }

        public static double GlobalTriggerWindowOpacity
        {
            get => (double)Current.Resources["GlobalTriggerWindowOpacity"];
            set => Current.Resources["GlobalTriggerWindowOpacity"] = value;
        }

        public static double GlobalDPSWindowOpacity
        {
            get => (double)Current.Resources["GlobalDPSWindowOpacity"];
            set => Current.Resources["GlobalDPSWindowOpacity"] = value;
        }
         
        private void App_Startup(object sender, StartupEventArgs e)
        {
            httpclient.DefaultRequestHeaders.Add("User-Agent", "request");
            if (e.Args.Length == 1)
            {
                try
                {
                    if (e.Args[0].Contains("ping"))
                    {
                        Thread.Sleep(1000 * 2);
                        var files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory());

                        // Copy the files and overwrite destination files if they already exist. 
                        foreach (var s in files)
                        {
                            // Use static Path methods to extract only the file name from the path.
                            var fileName = System.IO.Path.GetFileName(s);
                            var destFile = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory() + "/../", fileName);
                            System.IO.File.Copy(s, destFile, true);
                        }
                        var path = System.IO.Directory.GetCurrentDirectory() + "/../EQTool.exe";
                        _ = System.Diagnostics.Process.Start(new ProcessStartInfo
                        {
                            FileName = path,
                            Arguments = "pong",
                            UseShellExecute = true,
                            WorkingDirectory = System.IO.Directory.GetCurrentDirectory() + "/../"
                        });
                        App.Current.Shutdown();
                    }
                    else if (e.Args[0].Contains("pong"))
                    {
                        var theme = ThemeManager.Current.AddLibraryTheme(
                            new LibraryTheme(
                                new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Themes/Dark.Blue.xaml"),
                                MahAppsLibraryThemeProvider.DefaultInstance
                                )
                            );
                    
                        Thread.Sleep(1000 * 2);
                        System.IO.Directory.Delete("NewVersion", true);
                        System.IO.File.Delete("EqToool.zip");
                        mainWindow = new MainWindow();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    App.Current.Shutdown();
                }
            }
            else
            {
#if !DEBUG
                CheckForUpdates();
#endif
                var theme = ThemeManager.Current.AddLibraryTheme(
    new LibraryTheme(
        new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Themes/Dark.Blue.xaml"),
        MahAppsLibraryThemeProvider.DefaultInstance
        )
    );
                mainWindow = new MainWindow();
            }
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

        private void CheckForUpdates()
        {
            _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    var json = httpclient.GetAsync(new Uri("https://api.github.com/repos/smasherprog/EqTool/releases/latest")).Result.Content.ReadAsStringAsync().Result;
                    var githubdata = JsonConvert.DeserializeObject<GithubVersionInfo>(json);
                    var url = githubdata.assets.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.browser_download_url))?.browser_download_url;
                    var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    if (version != githubdata.tag_name)
                    {
                        var fileBytes = httpclient.GetByteArrayAsync(url).Result;
                        File.WriteAllBytes("EqToool.zip", fileBytes);
                        if (System.IO.Directory.Exists("NewVersion"))
                        {
                            System.IO.Directory.Delete("NewVersion", true);
                        }

                        ZipFile.ExtractToDirectory("EqToool.zip", "NewVersion");

                        if (Thread.CurrentThread == App.Current.Dispatcher.Thread)
                        {
                            mainWindow.Close();
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                mainWindow.Close();
                            });
                        }
                        var path = System.IO.Directory.GetCurrentDirectory() + "/NewVersion/EQTool.exe";
                        _ = System.Diagnostics.Process.Start(new ProcessStartInfo
                        {
                            FileName = path,
                            Arguments = "ping",
                            UseShellExecute = true,
                            WorkingDirectory = System.IO.Directory.GetCurrentDirectory() + "/NewVersion/"
                        });
                    }
                }
                catch
                {

                }

            });
        }
    }
}
