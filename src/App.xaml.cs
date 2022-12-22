using EQTool.Models; 
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
using System.Windows.Media;

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
            set
            {
                Current.Resources["GlobalFontSize"] = value;
                SetThemeValuesDPS();
                SetThemeValuesTriggers();
            }
        }

        private static Themes _Theme = Themes.Light;

        public static Themes Theme
        {
            get => _Theme;
            set
            {
                _Theme = value;
                SetThemeValuesDPS();
                SetThemeValuesTriggers();
            }
        }

        public static double GlobalTriggerWindowOpacity
        {
            get => (double)Current.Resources["GlobalTriggerWindowOpacity"];
            set
            {
                Current.Resources["GlobalTriggerWindowOpacity"] = value;
                SetThemeValuesTriggers();
            }
        }

        private static void SetThemeValuesTriggers()
        {
            var style = new Style
            {
                TargetType = typeof(Window)
            };

            style.Setters.Add(new Setter(Window.FontSizeProperty, GlobalFontSize));
            style.Setters.Add(new Setter(Window.BackgroundProperty, BackGroundBrushTrigger));
            Application.Current.Resources["MyWindowStyleTrigger"] = style;
        }

        private static SolidColorBrush BackGroundBrushTrigger
        {
            get
            {
                var t = _Theme == Themes.Light ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
                t.Opacity = GlobalTriggerWindowOpacity;
                return t;
            }
        }

        public static double GlobalDPSWindowOpacity
        {
            get => (double)Current.Resources["GlobalDPSWindowOpacity"];
            set
            {
                Current.Resources["GlobalDPSWindowOpacity"] = value;
                SetThemeValuesDPS();
            }
        }

        private static void SetThemeValuesDPS()
        {
            var style = new Style
            {
                TargetType = typeof(Window)
            };

            style.Setters.Add(new Setter(Window.FontSizeProperty, GlobalFontSize));
            style.Setters.Add(new Setter(Window.BackgroundProperty, BackGroundBrushDPS));
            Application.Current.Resources["MyWindowStyleDPS"] = style;
        }


        private static SolidColorBrush BackGroundBrushDPS
        {
            get
            {
                var t = _Theme == Themes.Light ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
                t.Opacity = GlobalDPSWindowOpacity;
                return t;
            }
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

        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public void OpenSpellsWindow()
        {
            mainWindow.OpenSpellsWindow();
        }

        public void OpenDPSWindow()
        {
            mainWindow.OpenDPSWindow();
        }
        public void OpenMapWindow()
        {
            mainWindow.OpenMapWindow();
        }

        public void OpenSettingsWindow()
        {
            mainWindow.OpenSettingsWindow();
        }

        public (string version, string urltodownload) LatestVersionAvailable
        {
            get
            {
                var json = httpclient.GetAsync(new Uri("https://api.github.com/repos/smasherprog/EqTool/releases/latest")).Result.Content.ReadAsStringAsync().Result;
                var githubdata = JsonConvert.DeserializeObject<GithubVersionInfo>(json);
                var url = githubdata.assets.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.browser_download_url))?.browser_download_url;
                return (githubdata.tag_name, url);
            }
        }

        public void CheckForUpdates()
        {
            _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    var (version, urltodownload) = LatestVersionAvailable;

                    if (Version != version)
                    {
                        var fileBytes = httpclient.GetByteArrayAsync(urltodownload).Result;
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
