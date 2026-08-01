using EQTool.Models;
using EQToolShared;
using EQToolShared.APIModels.UIFileControllerModels;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EQTool.Services
{
    // Two-way sync + backup for the EverQuest per-character UI config file pair
    // ("UI_<name>_<server>.ini" and "<name>_<server>.ini"). Modeled on
    // InventoryWatcherService. Everything is gated on the opt-in SyncUIFiles
    // setting AND a Discord login (the server also enforces the login). Nothing
    // runs otherwise.
    //
    // - FileSystemWatcher uploads a file whenever it changes on disk.
    // - SyncNow() (startup + the "Sync Now" button) reconciles every server file
    //   for every character: newer-wins in both directions, and any file missing
    //   locally is restored (the backup use-case).
    // - Downloads set the written file's mtime to the server value and record it,
    //   so the watcher does not echo our own writes straight back up.
    // - Successful uploads and downloads raise a tray balloon: filenames when
    //   1-2 files, otherwise UI vs character file counts.
    public class UIFileSyncService : IDisposable
    {
        private const string BaseUrl = "https://pigparse.azurewebsites.net";
        // mtime comparisons tolerate small clock differences between machines.
        private static readonly TimeSpan Epsilon = TimeSpan.FromSeconds(2);

        private readonly EQToolSettings _settings;
        private readonly LoggingService _loggingService;
        private readonly HttpClient _httpClient = new HttpClient();
        // file name (lower-cased) -> last mtime we uploaded or downloaded; used to
        // suppress duplicate FileSystemWatcher events and download echoes.
        private readonly ConcurrentDictionary<string, DateTime> _lastSyncedMtime = new ConcurrentDictionary<string, DateTime>();
        private FileSystemWatcher _watcher;

        public UIFileSyncService(EQToolSettings settings, LoggingService loggingService)
        {
            _settings = settings;
            _loggingService = loggingService;
        }

        // Logged in with Discord - required for any server call (upload/list/download/delete).
        private bool IsLoggedIn =>
            !string.IsNullOrEmpty(_settings.DiscordId) &&
            !string.IsNullOrEmpty(_settings.DiscordApiToken);

        // Automatic sync is enabled: the opt-in toggle gates only the background behavior
        // (watcher uploads + startup pull). Manual actions (Sync Now / Refresh / per-character
        // right-click) work whenever logged in, regardless of the toggle.
        private bool IsEnabled => _settings.SyncUIFiles && IsLoggedIn;

        public void Start()
        {
            var dir = GetEffectiveDirectory();
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                _watcher = new FileSystemWatcher(dir, "*.ini")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };
                _watcher.Created += OnFileChanged;
                _watcher.Changed += OnFileChanged;
            }

            // Automatic startup pull only when the opt-in toggle is on (and logged in).
            // Runs off-thread so InitStuff is not blocked.
            if (IsEnabled)
            {
                _ = Task.Factory.StartNew(() =>
                {
                    try { SyncNow(); }
                    catch { }
                });
            }
        }

        public void UpdateDirectory()
        {
            Dispose();
            Start();
        }

        private string GetEffectiveDirectory()
        {
            var root = _settings.DefaultEqDirectory;
            if (string.IsNullOrEmpty(root))
            {
                return null;
            }
            return FindEq.GetEffectiveUiDirectory(root);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }
            var fileName = Path.GetFileName(e.FullPath);
            if (!UIFileName.TryParse(fileName, out var info))
            {
                return;
            }
            _ = Task.Factory.StartNew(() =>
            {
                if (UploadFile(e.FullPath, fileName, info))
                {
                    ShowSyncNotification("Uploaded", new List<string> { fileName });
                }
            });
        }

        // ------- reconcile / backup -------

        public void SyncNow()
        {
            if (!IsLoggedIn)
            {
                return;
            }
            var dir = GetEffectiveDirectory();
            if (string.IsNullOrEmpty(dir))
            {
                return;
            }

            var serverFiles = GetServerFiles();

            // PULL / RESTORE: bring down anything newer on the server, or anything
            // missing locally (backup restore to a fresh machine).
            var downloaded = new List<string>();
            foreach (var meta in serverFiles)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(meta.FileName) || !UIFileName.IsUiPairFile(meta.FileName))
                    {
                        continue;
                    }
                    var path = Path.Combine(dir, meta.FileName);
                    var localExists = File.Exists(path);
                    var localfiletime = File.GetLastWriteTimeUtc(path).Add(Epsilon);
                    var shouldPull = !localExists ||
                        meta.LastModifiedUtc > localfiletime;
                    if (!shouldPull)
                    {
                        continue;
                    }
                    var download = DownloadFile(meta.FileName);
                    if (download != null && download.Contents != null && WriteDownloadedFile(path, download))
                    {
                        downloaded.Add(meta.FileName);
                    }
                }
                catch { }
            }
            ShowSyncNotification("Downloaded", downloaded);

            // PUSH / SEED: upload local files newer than (or absent from) the server.
            var uploaded = new List<string>();
            foreach (var localPath in EnumerateLocalPairFiles(dir))
            {
                try
                {
                    var fileName = Path.GetFileName(localPath);
                    if (!UIFileName.TryParse(fileName, out var info))
                    {
                        continue;
                    }
                    var mtime = File.GetLastWriteTimeUtc(localPath);
                    var meta = serverFiles.FirstOrDefault(m => string.Equals(m.FileName, fileName, StringComparison.OrdinalIgnoreCase));
                    var shouldPush = meta == null || mtime > meta.LastModifiedUtc.Add(Epsilon);
                    if (shouldPush && UploadFile(localPath, fileName, info))
                    {
                        uploaded.Add(fileName);
                    }
                }
                catch { }
            }
            ShowSyncNotification("Uploaded", uploaded);
        }

        private List<string> EnumerateLocalPairFiles(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    return new List<string>();
                }
                return Directory.GetFiles(dir, "*.ini", SearchOption.TopDirectoryOnly)
                    .Where(f => UIFileName.IsUiPairFile(Path.GetFileName(f)))
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        // Returns true when the file was actually written to disk.
        private bool WriteDownloadedFile(string path, UIFileDownloadResponse download)
        {
            var key = Path.GetFileName(path).ToLowerInvariant();
            var watcherWasOn = _watcher != null && _watcher.EnableRaisingEvents;
            try
            {
                if (watcherWasOn)
                {
                    _watcher.EnableRaisingEvents = false;
                }
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }
                File.WriteAllText(path, download.Contents);
                // Match the server mtime so future comparisons are correct and the
                // watcher does not treat our own write as a new local change.
                File.SetLastWriteTimeUtc(path, download.LastModifiedUtc);
                _lastSyncedMtime[key] = download.LastModifiedUtc;
                return true;
            }
            catch { }
            finally
            {
                if (watcherWasOn && _watcher != null)
                {
                    _watcher.EnableRaisingEvents = true;
                }
            }
            return false;
        }

        // ------- upload -------

        // Returns true when the file was actually uploaded to the server.
        private bool UploadFile(string path, string fileName, UIFileNameInfo info)
        {
            if (!IsEnabled)
            {
                return false;
            }
            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }
                var mtime = File.GetLastWriteTimeUtc(path);
                var key = fileName.ToLowerInvariant();
                if (_lastSyncedMtime.TryGetValue(key, out var known) && known == mtime)
                {
                    return false; // unchanged since our last sync (duplicate event / echo)
                }
                var text = ReadAllTextWithRetry(path);
                if (text == null)
                {
                    return false;
                }

                var request = new UIFileUploadRequest
                {
                    FileName = fileName,
                    PlayerName = info.PlayerName,
                    Server = info.Server,
                    LastModifiedUtc = mtime,
                    Contents = text
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = Send(HttpMethod.Post, BaseUrl + "/api/uifile/upload", content);
                if (response != null && response.IsSuccessStatusCode)
                {
                    _lastSyncedMtime[key] = mtime;
                    return true;
                }
            }
            catch { }
            return false;
        }

        // Tray balloon summarizing synced files ("Uploaded"/"Downloaded"): filenames
        // when 1-2 files, otherwise UI vs character file counts.
        private static void ShowSyncNotification(string verb, List<string> files)
        {
            if (files.Count == 0)
            {
                return;
            }

            string message;
            if (files.Count <= 2)
            {
                message = verb + " " + string.Join(", ", files);
            }
            else
            {
                var uiCount = files.Count(f => UIFileName.TryParse(f, out var info) && info.IsUiLayoutFile);
                var nonUiCount = files.Count - uiCount;
                message = $"{verb} {uiCount} UI file{(uiCount == 1 ? "" : "s")} and {nonUiCount} character file{(nonUiCount == 1 ? "" : "s")}";
            }

            try
            {
                var app = App.Current as App;
                _ = (app?.Dispatcher.BeginInvoke((Action)(() =>
                {
                    app.ShowBalloonTip(4000, "UI Files Synced", message, System.Windows.Forms.ToolTipIcon.Info);
                })));
            }
            catch { }
        }

        private static string ReadAllTextWithRetry(string path)
        {
            // EQ may still hold the file open briefly while saving it.
            for (var attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    return File.ReadAllText(path);
                }
                catch (IOException)
                {
                    Thread.Sleep(250);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        // ------- management API surface (used by the UI Sync settings tab) -------

        public List<UIFileMetadata> GetServerFiles()
        {
            if (!IsLoggedIn)
            {
                return new List<UIFileMetadata>();
            }
            try
            {
                var response = Send(HttpMethod.Get, BaseUrl + "/api/uifile/list");
                if (response != null && response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<List<UIFileMetadata>>(json) ?? new List<UIFileMetadata>();
                }
            }
            catch { }
            return new List<UIFileMetadata>();
        }

        // Local UI pair files present on disk (parsed to player/server). Does not
        // require a Discord login - it just reads the EQ folder.
        public List<UIFileNameInfo> GetLocalUiFiles()
        {
            var result = new List<UIFileNameInfo>();
            foreach (var path in EnumerateLocalPairFiles(GetEffectiveDirectory()))
            {
                if (UIFileName.TryParse(Path.GetFileName(path), out var info))
                {
                    result.Add(info);
                }
            }
            return result;
        }

        public bool DeleteServerFile(string fileName)
        {
            if (!IsLoggedIn || string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }
            try
            {
                var url = BaseUrl + "/api/uifile/delete?fileName=" + Uri.EscapeDataString(fileName);
                var response = Send(HttpMethod.Delete, url);
                return response != null && response.IsSuccessStatusCode;
            }
            catch { }
            return false;
        }

        private UIFileDownloadResponse DownloadFile(string fileName)
        {
            try
            {
                var url = BaseUrl + "/api/uifile/download?fileName=" + Uri.EscapeDataString(fileName);
                var response = Send(HttpMethod.Get, url);
                if (response != null && response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<UIFileDownloadResponse>(json);
                }
            }
            catch { }
            return null;
        }

        private HttpResponseMessage Send(HttpMethod method, string url, HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.DiscordApiToken);
            if (content != null)
            {
                request.Content = content;
            }
            return _httpClient.SendAsync(request).Result;
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _watcher = null;
        }
    }
}
