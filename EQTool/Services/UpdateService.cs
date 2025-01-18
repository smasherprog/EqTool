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
using System.Windows;
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
        public class GithubAsset
        {
            public string browser_download_url { get; set; }
        }

        public class GithubVersionInfo
        {
            public List<GithubAsset> assets { get; set; }
            public string name { get; set; }
            public string tag_name { get; set; }
            public bool prerelease { get; set; }
            public DateTime created_at { get; set; }
            public DateTime published_at { get; set; }
        }

        public enum UpdateStatus
        {
            UpdatesApplied,
            OldFilesDeleted,
            NoUpdateApplied
        }

        private const string programName = "EQTool.exe";

        public static UpdateStatus ApplyUpdate(string parameter)
        {
            if (!string.IsNullOrWhiteSpace(parameter))
            {
                if (parameter.Contains("ping"))
                {
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

        public static void CheckForUpdates(string currentversion1, string versiontype, Autofac.IContainer container, bool firstRun)
        {
            _ = Task.Factory.StartNew(() =>
            {
                IAppDispatcher appDispatcher = null;
                LoggingService loggingService = null;
                LoginMiddlemand loginMiddlemand = null;
                try
                {
                    appDispatcher = container.Resolve<IAppDispatcher>();
                    loggingService = container.Resolve<LoggingService>();
                    loginMiddlemand = container.Resolve<LoginMiddlemand>();
                }
                catch { }

                CheckForUpdates(currentversion1, versiontype, appDispatcher, loggingService, loginMiddlemand, firstRun);
            });
        }

        public static void CheckForUpdates(string currentversion1, string versiontype, IAppDispatcher appDispatcher, LoggingService loggingService, LoginMiddlemand loginMiddlemand, bool firstRun)
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
                    appDispatcher?.DispatchUI(() =>
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
                    loginMiddlemand?.StopListening();
                    appDispatcher?.DispatchUI(() =>
                    {
                        System.Windows.Application.Current.Shutdown();
                    });
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
                    appDispatcher?.DispatchUI(() =>
                    {
                        MessageBox.Show("PigParse Update Failed", "There was a permission issue with the currently install location. Please move Pigparse to a different directory!", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                else if (firstRun)
                {
                    appDispatcher?.DispatchUI(() =>
                    {
                        MessageBox.Show("PigParse Update Failed", "There was a problem updating pigparse, please check github for the latest update!", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                loggingService?.Log(ex.ToString(), EQToolShared.Enums.EventType.Update, null);
                File.AppendAllText("Errors.txt", ex.ToString());
            }
        }
    }
}
