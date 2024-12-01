using Autofac;
using EQTool.Services.P99LoginMiddlemand;
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
        private static void _CopyFilesRecursively(string sourcePath, string targetPath)
        {
            foreach (var file in Directory.GetFiles(sourcePath))
            {
                File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)), true);
            }
        }
        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            for (var i = 0; i <= 6; i++)
            {
                try
                {
                    _CopyFilesRecursively(sourcePath, targetPath);
                    return;
                }
                catch
                {
                    if (i == 6)
                    {
                        throw;
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        private readonly AppDispatcher appDispatcher = new AppDispatcher();
        public enum UpdateStatus
        {
            UpdatesApplied,
            OldFilesDeleted,
            NoUpdateApplied
        }

        private const string programName = "EQTool.exe";

        public UpdateStatus ApplyUpdate(string parameter)
        {
            if (!string.IsNullOrWhiteSpace(parameter))
            {
                if (parameter.Contains("ping"))
                {
                    appDispatcher.DispatchUI(() =>
                    {
                        (App.Current as App).ShowBalloonTip(3000, "Applying PigParse Update", "Extracting Files . . .", System.Windows.Forms.ToolTipIcon.Info);
                    });
                    var sourcedirectory = System.IO.Directory.GetCurrentDirectory();
                    var parentidirectory = Directory.GetParent(sourcedirectory).FullName;
                    CopyFilesRecursively(sourcedirectory, parentidirectory);
                    var path = Path.Combine(parentidirectory, programName);
                    _ = System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = "pong",
                        UseShellExecute = true,
                        WorkingDirectory = parentidirectory
                    });
                    App.Current.Shutdown();
                    return UpdateStatus.UpdatesApplied;
                }
                else if (parameter.Contains("pong"))
                {
                    appDispatcher.DispatchUI(() =>
                    {
                        (App.Current as App).ShowBalloonTip(3000, "PigParse Update Complete", "Cleaning up files . . .", System.Windows.Forms.ToolTipIcon.Info);
                    });
                    try
                    {
                        System.IO.Directory.Delete("NewVersion", true);
                        System.IO.File.Delete("EqTool.zip");
                    }
                    catch { }
                    return UpdateStatus.OldFilesDeleted;
                }
            }

            return UpdateStatus.NoUpdateApplied;
        }

        public void CheckForUpdates(string currentversion1, string versiontype, Autofac.IContainer container, bool firstRun)
        {
            _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    var prerelease = false;
#if BETA
                    prerelease = true;
#endif
                    var currentversion = currentversion1;
                    if (!string.IsNullOrWhiteSpace(currentversion))
                    {
                        currentversion = currentversion1.Replace(versiontype, string.Empty);
                    }

                    var version = new string(currentversion.Where(a => char.IsDigit(a) || a == '.').ToArray());
                    version = version.Trim('.');
                    var json = httpclient.GetAsync(new Uri("https://api.github.com/repos/smasherprog/EqTool/releases")).Result.Content.ReadAsStringAsync().Result;
                    var githubdata = JsonConvert.DeserializeObject<List<GithubVersionInfo>>(json);
                    var releases = githubdata.OrderByDescending(a => a.published_at).Where(a => a.name != null && a.prerelease == prerelease && a.assets != null && a.assets.Any()).ToList();
                    var release = releases.FirstOrDefault();
                    var downloadurl = release.assets.Where(a => !string.IsNullOrWhiteSpace(a.browser_download_url) && a.browser_download_url.Contains(VersionType)).Select(a => a.browser_download_url).FirstOrDefault();
                    var newversion = release.tag_name;
                    if (version != newversion)
                    {
                        File.AppendAllText("Errors.txt", $"Updating: {currentversion1}-{versiontype}-{currentversion}-{version}-{newversion}");
                        appDispatcher.DispatchUI(() =>
                        {
                            (App.Current as App).ShowBalloonTip(3000, "Downloading PigParse Update", "This might take a few seconds . . .", System.Windows.Forms.ToolTipIcon.Info);
                        });
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
                        var logingmiddlemand = container?.Resolve<LoginMiddlemand>();
                        logingmiddlemand?.StopListening();
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
                        var path = System.IO.Directory.GetCurrentDirectory() + $"/NewVersion/{programName}";
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
                    if (ex.ToString().Contains("Access to the path 'NewVersion' is denied."))
                    {
                        appDispatcher.DispatchUI(() =>
                        {
                            (App.Current as App).ShowBalloonTip(3000, "PigParse Update Failed", "There was a permission issue with the currently install location. Please move Pigparse to a different directory!", System.Windows.Forms.ToolTipIcon.Warning);
                        });
                    }
                    else if (firstRun)
                    {
                        appDispatcher.DispatchUI(() =>
                        {
                            (App.Current as App).ShowBalloonTip(3000, "PigParse Update Failed", "There was a problem updating pigparse, please check github for the latest update!", System.Windows.Forms.ToolTipIcon.Warning);
                        });
                    }
                    container?.Resolve<LoggingService>().Log(ex.ToString(), EQToolShared.Enums.EventType.Update, null);
                    File.AppendAllText("Errors.txt", ex.ToString());
                }
            });
        }
    }
}
