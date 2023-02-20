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

namespace EQTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static HttpClient httpclient = new HttpClient();
        private MainWindow mainWindow;

        private static Themes _Theme = Themes.Light;

        public static Themes Theme
        {
            get => _Theme;
            set
            {
                _Theme = value;
                EQTool.Properties.Settings.Default.ColorMode = value.ToString();
                ThemeChangedEvent?.Invoke(null, new ThemeChangeEventArgs { Theme = value });
            }
        }
        public class ThemeChangeEventArgs : EventArgs
        {
            public Themes Theme { get; set; }
        }

        public static event EventHandler<ThemeChangeEventArgs> ThemeChangedEvent;


        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                _ = Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private void DeleteOldFiles()
        {
            var oldfiles = new List<string>()
            {
                "Autofac.dll",
                "Autofac.xml",
                "Copy.png",
                "DotNetProjects.DataVisualization.Toolkit.dll",
                "dps.png",
                "EQTool.pdb",
                "FightLog.csv",
                "HarfBuzzSharp.dll",
                "HarfBuzzSharp.pdb",
                "HarfBuzzSharp.xml",
                "HelixToolkit.dll",
                "HelixToolkit.Wpf.dll",
                "HelixToolkit.Wpf.xml",
                "HelixToolkit.xml",
                "libHarfBuzzSharp.dylib",
                "libSkiaSharp.dylib",
                "LiveChartsCore.dll",
                "LiveChartsCore.SkiaSharpView.dll",
                "LiveChartsCore.SkiaSharpView.WPF.dll",
                "LiveChartsCore.SkiaSharpView.WPF.xml",
                "LiveChartsCore.SkiaSharpView.xml",
                "LiveChartsCore.xml",
                "map.png",
                "Microsoft.Bcl.AsyncInterfaces.dll",
                "Microsoft.Bcl.AsyncInterfaces.xml",
                "Microsoft.Extensions.Logging.Abstractions.dll",
                "Microsoft.Extensions.Logging.Abstractions.xml",
                "Newtonsoft.Json.dll",
                "Newtonsoft.Json.xml",
                "open-folder.png",
                "SharpDX.dll",
                "SharpDX.pdb",
                "SkiaSharp.dll",
                "SkiaSharp.HarfBuzz.dll",
                "SkiaSharp.HarfBuzz.pdb",
                "SkiaSharp.HarfBuzz.xml",
                "SkiaSharp.pdb",
                "SkiaSharp.Views.Desktop.Common.dll",
                "SkiaSharp.Views.Desktop.Common.pdb",
                "SkiaSharp.Views.Desktop.Common.xml",
                "SkiaSharp.Views.WPF.dll",
                "SkiaSharp.Views.WPF.pdb",
                "SkiaSharp.Views.WPF.xml",
                "SkiaSharp.xml",
                "System.Buffers.dll",
                "System.Buffers.xml",
                "System.Diagnostics.DiagnosticSource.dll",
                "System.Diagnostics.DiagnosticSource.xml",
                "System.Drawing.Common.dll",
                "System.Drawing.Common.xml",
                "System.Memory.dll",
                "System.Memory.xml",
                "System.Numerics.Vectors.dll",
                "System.Numerics.Vectors.xml",
                "System.Runtime.CompilerServices.Unsafe.dll",
                "System.Runtime.CompilerServices.Unsafe.xml",
                "System.Threading.Tasks.Extensions.dll",
                "System.Threading.Tasks.Extensions.xml",
                "TestFight.txt",
                "TestFight2.txt",
                "Trash.png",
                "wizard.png"
              };
            try
            {
                System.IO.Directory.Delete(System.IO.Directory.GetCurrentDirectory() + "/../map_files", true);
                foreach (var item in oldfiles)
                {
                    System.IO.File.Delete(System.IO.Directory.GetCurrentDirectory() + $"/../{item}");
                }
            }
            catch { }
        }

        private bool WaitForEQToolToStop()
        {
            var counter = 0;
            int count;
            do
            {
                count = Process.GetProcessesByName("eqtool").Count();
                if (counter++ > 6)
                {
                    return false;
                }
                Debug.WriteLine($"Waiting for eqtool {count} on counter {counter}");
                Thread.Sleep(1000);
            }
            while (count != 1);
            return true;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (!WaitForEQToolToStop())
            {
                MessageBox.Show("Another EQTool is currently running. You must shut that one down first!", "Multiple EQTools running!", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
                return;
            }
            httpclient.DefaultRequestHeaders.Add("User-Agent", "request");
            if (e.Args.Length == 1)
            {
                try
                {
                    if (e.Args[0].Contains("ping"))
                    {
                        DeleteOldFiles();
                        var files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory());
                        CopyFilesRecursively(System.IO.Directory.GetCurrentDirectory(), System.IO.Directory.GetCurrentDirectory() + "/../");

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
                        System.IO.Directory.Delete("NewVersion", true);
                        try
                        {
                            System.IO.File.Delete("EqTool.zip");
                        }
                        catch { }
                        mainWindow = new MainWindow(true);
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText("Errors.txt", ex.ToString());
                    MessageBox.Show(ex.Message);
                    App.Current.Shutdown();
                }
            }
            else
            {
#if !DEBUG
                //CheckForUpdates();
#endif
                mainWindow = new MainWindow(false);
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
            public bool prerelease { get; set; }
            public DateTime created_at { get; set; }
        }

        public string Version
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version.ToString();
#if Beta
                v = "Beta-" + v;
#endif
                return v;
            }
        }

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

        public void OpenMobInfoWindow()
        {
            mainWindow.OpenMobInfoWindow();
        }

        public void OpenSettingsWindow()
        {
            mainWindow.OpenSettingsWindow();
        }

        public (string version, string urltodownload) LatestVersionAvailable
        {
            get
            {
                var prerelease = false;
#if Beta
                prerelease = true;
#endif
                var json = httpclient.GetAsync(new Uri("https://api.github.com/repos/smasherprog/EqTool/releases")).Result.Content.ReadAsStringAsync().Result;
                var githubdata = JsonConvert.DeserializeObject<List<GithubVersionInfo>>(json);
                var release = githubdata.OrderByDescending(a => a.created_at).FirstOrDefault(a => a.prerelease == prerelease);
                var downloadurl = release.assets.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.browser_download_url))?.browser_download_url;
                return (release.tag_name, downloadurl);
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
                        if (System.IO.Directory.Exists("NewVersion"))
                        {
                            System.IO.Directory.Delete("NewVersion", true);
                        }
                        var fileBytes = httpclient.GetByteArrayAsync(urltodownload).Result;
                        var filename = Path.GetFileName(urltodownload);
                        if (filename.EndsWith(".zip"))
                        {
                            File.WriteAllBytes("EqTool.zip", fileBytes);
                            ZipFile.ExtractToDirectory("EqTool.zip", System.IO.Directory.GetCurrentDirectory() + "/NewVersion");
                        }
                        else
                        {
                            _ = System.IO.Directory.CreateDirectory(System.IO.Directory.GetCurrentDirectory() + "/NewVersion");
                            File.WriteAllBytes(System.IO.Directory.GetCurrentDirectory() + "/NewVersion/" + filename, fileBytes);
                        }

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
                catch (Exception ex)
                {
                    File.AppendAllText("Errors.txt", ex.ToString());
                }
            });
        }
    }
}
