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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EQTool.Services
{
    public class UIFileSyncService : IDisposable
    {
        private const string BaseUrl = "https://pigparse.azurewebsites.net";
        // mtime comparisons tolerate small clock differences between machines.
        private static readonly TimeSpan Epsilon = TimeSpan.FromSeconds(2);
        // How long to let a save settle before reading it: EQ rewrites an ini in
        // several flushes and the first watcher event arrives mid-write.
        private const int SettleMilliseconds = 1500;

        private readonly EQToolSettings _settings;
        private readonly HttpClient _httpClient = new HttpClient();
        // file name (lower-cased) -> hash of the contents we last synced.
        private readonly ConcurrentDictionary<string, string> _syncedHash = new ConcurrentDictionary<string, string>();
        // Uploads run one at a time so the hash guard is read and recorded without
        // another event for the same file slipping in between.
        private readonly object _uploadLock = new object();
        // 0/1: one reconcile at a time.
        private int _syncing;
        private FileSystemWatcher _watcher;

        public UIFileSyncService(EQToolSettings settings)
        {
            _settings = settings;
        }

        private bool IsLoggedIn =>
            !string.IsNullOrEmpty(_settings.DiscordId) &&
            !string.IsNullOrEmpty(_settings.DiscordApiToken);

        // The opt-in toggle gates only background behavior. Manual actions (Sync Now, Refresh,
        // per-character right-click) work whenever logged in, regardless of the toggle.
        private bool IsEnabled => _settings.SyncUIFiles && IsLoggedIn;

        public void Start()
        {
            Dispose();
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

            // off-thread so InitStuff is not blocked
            if (IsEnabled)
            {
                RunInBackground(SyncNow);
            }
        }

        public void UpdateDirectory()
        {
            Start(); // Start() tears down the previous watcher first
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _watcher = null;
        }

        private string GetEffectiveDirectory()
        {
            var root = _settings.DefaultEqDirectory;
            return string.IsNullOrEmpty(root) ? null : FindEq.GetEffectiveUiDirectory(root);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!IsEnabled || !UIFileName.TryParse(e.FullPath, out var info))
            {
                return;
            }
            var fileName = Path.GetFileName(e.FullPath);
            RunInBackground(() =>
            {
                // One save arrives as several events. They all wait out the settle
                // period, the first one through uploads, and the rest match the
                // recorded hash and do nothing - so one save, one balloon.
                Thread.Sleep(SettleMilliseconds);
                if (UploadFile(e.FullPath, fileName, info))
                {
                    ShowSyncNotification("Uploaded", new List<string> { fileName });
                }
            });
        }

        // A call arriving while one is already running (startup pull vs. the "Sync Now"
        // button) returns instead of doing the whole reconcile twice.
        public void SyncNow()
        {
            if (!IsLoggedIn || Interlocked.CompareExchange(ref _syncing, 1, 0) != 0)
            {
                return;
            }
            try
            {
                var dir = GetEffectiveDirectory();
                if (string.IsNullOrEmpty(dir))
                {
                    return;
                }
                var serverFiles = GetServerFiles();
                ShowSyncNotification("Downloaded", Pull(dir, serverFiles));
                ShowSyncNotification("Uploaded", Push(dir, serverFiles));
            }
            finally
            {
                _ = Interlocked.Exchange(ref _syncing, 0);
            }
        }

        // also pulls anything missing locally, which is the restore-to-a-fresh-machine case
        private List<string> Pull(string dir, List<UIFileMetadata> serverFiles)
        {
            var downloaded = new List<string>();
            foreach (var meta in serverFiles)
            {
                try
                {
                    if (!UIFileName.IsUiPairFile(meta.FileName))
                    {
                        continue;
                    }
                    var path = Path.Combine(dir, meta.FileName);
                    if (File.Exists(path) && meta.LastModifiedUtc <= File.GetLastWriteTimeUtc(path).Add(Epsilon))
                    {
                        continue; // local copy is current
                    }
                    var download = DownloadFile(meta.FileName);
                    if (download?.Contents != null && WriteDownloadedFile(path, download))
                    {
                        downloaded.Add(meta.FileName);
                    }
                }
                catch { }
            }
            return downloaded;
        }

        private List<string> Push(string dir, List<UIFileMetadata> serverFiles)
        {
            var uploaded = new List<string>();
            foreach (var path in EnumerateLocalPairFiles(dir))
            {
                try
                {
                    var fileName = Path.GetFileName(path);
                    if (!UIFileName.TryParse(fileName, out var info))
                    {
                        continue;
                    }
                    var meta = serverFiles.FirstOrDefault(m => string.Equals(m.FileName, fileName, StringComparison.OrdinalIgnoreCase));
                    if (meta != null && File.GetLastWriteTimeUtc(path) <= meta.LastModifiedUtc.Add(Epsilon))
                    {
                        continue; // server copy is current
                    }
                    if (UploadFile(path, fileName, info))
                    {
                        uploaded.Add(fileName);
                    }
                }
                catch { }
            }
            return uploaded;
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

        // only a login is checked here: the watcher tests the SyncUIFiles toggle before calling
        private bool UploadFile(string path, string fileName, UIFileNameInfo info)
        {
            if (!IsLoggedIn || !File.Exists(path))
            {
                return false;
            }
            try
            {
                lock (_uploadLock)
                {
                    var mtime = File.GetLastWriteTimeUtc(path);
                    var text = ReadAllTextWithRetry(path);
                    if (text == null)
                    {
                        return false;
                    }
                    var key = fileName.ToLowerInvariant();
                    var hash = ComputeHash(text);
                    if (_syncedHash.TryGetValue(key, out var synced) && synced == hash)
                    {
                        // Already on the server: a duplicate watcher event, a save
                        // that changed nothing, or our own downloaded write.
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
                    var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = Send(HttpMethod.Post, BaseUrl + "/api/uifile/upload", content);
                    if (response == null || !response.IsSuccessStatusCode)
                    {
                        return false;
                    }
                    _syncedHash[key] = hash;
                    return true;
                }
            }
            catch { }
            return false;
        }

        // Returns true when the file was written to disk. The watcher deliberately
        // stays on: the hash recorded here is what keeps our own write from
        // echoing straight back up.
        private bool WriteDownloadedFile(string path, UIFileDownloadResponse download)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }
                _syncedHash[Path.GetFileName(path).ToLowerInvariant()] = ComputeHash(download.Contents);
                File.WriteAllText(path, download.Contents);
                // Match the server mtime so newer-wins comparisons stay correct.
                File.SetLastWriteTimeUtc(path, download.LastModifiedUtc);
                return true;
            }
            catch { }
            return false;
        }

        private static string ComputeHash(string text)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? string.Empty)));
            }
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

        private static void RunInBackground(Action work)
        {
            _ = Task.Factory.StartNew(() =>
            {
                try { work(); }
                catch { }
            });
        }

        public List<UIFileMetadata> GetServerFiles()
        {
            if (!IsLoggedIn)
            {
                return new List<UIFileMetadata>();
            }
            return SendJson<List<UIFileMetadata>>(BaseUrl + "/api/uifile/list") ?? new List<UIFileMetadata>();
        }

        // no login needed - this only reads the EQ folder
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
            var response = Send(HttpMethod.Delete, BaseUrl + "/api/uifile/delete?fileName=" + Uri.EscapeDataString(fileName));
            return response != null && response.IsSuccessStatusCode;
        }

        private UIFileDownloadResponse DownloadFile(string fileName)
        {
            return SendJson<UIFileDownloadResponse>(BaseUrl + "/api/uifile/download?fileName=" + Uri.EscapeDataString(fileName));
        }

        // Returns null when the call failed; every caller treats that as failure,
        // so nothing above this line needs its own try/catch around HTTP.
        private HttpResponseMessage Send(HttpMethod method, string url, HttpContent content = null)
        {
            try
            {
                var request = new HttpRequestMessage(method, url)
                {
                    Content = content
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.DiscordApiToken);
                return _httpClient.SendAsync(request).Result;
            }
            catch { }
            return null;
        }

        private T SendJson<T>(string url) where T : class
        {
            var response = Send(HttpMethod.Get, url);
            if (response == null || !response.IsSuccessStatusCode)
            {
                return null;
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            catch { }
            return null;
        }
    }
}
