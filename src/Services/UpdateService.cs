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
            foreach (var file in Directory.GetFiles(sourcePath))
            {
                File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)), true);
            }
        }

        public enum UpdateStatus
        {
            UpdatesApplied,
            OldFilesDeleted,
            NoUpdateApplied
        }
#if QUARM
        private const string programName = "PQTool.exe";
#else
        private const string programName = "EQTool.exe";
#endif
        public UpdateStatus ApplyUpdate(string parameter)
        {
            if (!string.IsNullOrWhiteSpace(parameter))
            {
                Thread.Sleep(3000);
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
                    var releases = githubdata.OrderByDescending(a => a.published_at).Where(a => a.name != null).ToList();
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
                catch (Exception)
                {
                    //File.AppendAllText("Errors.txt", ex.ToString());
                }
            });
        }
    }
}
