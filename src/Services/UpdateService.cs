using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static EQTool.App;

namespace EQTool.Services
{
    public class UpdateService
    {
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
        public enum UpdateStatus
        {
            UpdatesApplied,
            OldFilesDeleted,
            NoUpdateApplied
        }

        public UpdateStatus ApplyUpdate(string parameter)
        {
            if (!string.IsNullOrWhiteSpace(parameter))
            {
                if (parameter.Contains("ping"))
                {
                    DeleteOldFiles();
                    _ = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory());
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
                    return UpdateStatus.UpdatesApplied;
                }
                else if (parameter.Contains("pong"))
                {
                    System.IO.Directory.Delete("NewVersion", true);
                    try
                    {
                        System.IO.File.Delete("EqTool.zip");
                    }
                    catch { }
                    return UpdateStatus.OldFilesDeleted;
                }
            }

            return UpdateStatus.NoUpdateApplied;
        }

        public void CheckForUpdates(string currentversion)
        {
            _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    var prerelease = false;
#if BETA
                    prerelease = true;
#endif
                    var json = httpclient.GetAsync(new Uri("https://api.github.com/repos/smasherprog/EqTool/releases")).Result.Content.ReadAsStringAsync().Result;
                    var githubdata = JsonConvert.DeserializeObject<List<GithubVersionInfo>>(json);
                    var releases = githubdata.OrderByDescending(a => a.created_at).Where(a => a.name != null).ToList();
                    var release = releases.FirstOrDefault(a => a.prerelease == prerelease && !a.name.Contains("Quarm"));
#if QUARM
                    release = releases.FirstOrDefault(a => a.name.Contains("Quarm"));
#endif
                    var downloadurl = release.assets.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.browser_download_url))?.browser_download_url;

                    if (currentversion != release.tag_name)
                    {
                        if (System.IO.Directory.Exists("NewVersion"))
                        {
                            System.IO.Directory.Delete("NewVersion", true);
                        }
                        var fileBytes = App.httpclient.GetByteArrayAsync(downloadurl).Result;
                        var filename = Path.GetFileName(downloadurl);
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
                            System.Windows.Application.Current.Shutdown();
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                System.Windows.Application.Current.Shutdown();
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
                catch (Exception)
                {
                    //File.AppendAllText("Errors.txt", ex.ToString());
                }
            });
        }
    }
}
