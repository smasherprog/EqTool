using EQTool.Models;
using System;
using System.IO;
using System.Timers;

namespace EQTool.Services
{
    public class LogArchiveService : IDisposable
    {
        private readonly EQToolSettings settings;
        private Timer timer;

        public LogArchiveService(EQToolSettings settings)
        {
            this.settings = settings;
            timer = new Timer(1000 * 60 * 60); // check every hour
            timer.Elapsed += (s, e) => TryArchiveLogs();
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public void TryArchiveLogs()
        {
            if (!settings.LogArchiveEnabled)
            {
                return;
            }

            var logDir = settings.EqLogDirectory;
            if (string.IsNullOrWhiteSpace(logDir) || !Directory.Exists(logDir))
            {
                return;
            }

            var thresholdBytes = (long)settings.LogArchiveSizeMB * 1024 * 1024;
            var archiveDir = Path.Combine(logDir, "archive");

            try
            {
                var files = Directory.GetFiles(logDir, "*.txt");
                foreach (var file in files)
                {
                    try
                    {
                        var info = new FileInfo(file);
                        if (info.Length < thresholdBytes)
                        {
                            continue;
                        }

                        if (!Directory.Exists(archiveDir))
                        {
                            Directory.CreateDirectory(archiveDir);
                        }

                        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                        var nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                        var dest = Path.Combine(archiveDir, $"{nameWithoutExt}_{timestamp}.txt");
                        File.Move(file, dest);
                    }
                    catch { }
                }
            }
            catch { }
        }

        public void Dispose()
        {
            timer?.Stop();
            timer?.Dispose();
            timer = null;
        }
    }
}
