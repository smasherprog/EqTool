using EQTool.Models;
using EQTool.Services.Map;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace EQTool.Services
{
    public class LogParser : IDisposable
    {
        private Timer UITimer;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private long? LastReadOffset = null;
        private readonly EQToolSettings settings;
        private readonly LevelLogParse levelLogParse;
        private readonly EQToolSettingsLoad toolSettingsLoad;

        public LogParser(EQToolSettingsLoad toolSettingsLoad, ActivePlayer activePlayer, IAppDispatcher appDispatcher, EQToolSettings settings, LevelLogParse levelLogParse)
        {
            this.toolSettingsLoad = toolSettingsLoad;
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.levelLogParse = levelLogParse;
            this.settings = settings;
            UITimer = new System.Timers.Timer(100);
            UITimer.Elapsed += Poll;
            UITimer.Enabled = true;
        }

        public class LogParserEventArgs : EventArgs
        {
            public string Line { get; set; }
        }
        public class PlayerChangeEventArgs : EventArgs
        {
        }

        public event EventHandler<LogParserEventArgs> LineReadEvent;

        public event EventHandler<PlayerChangeEventArgs> PlayerChangeEvent;

        public void Push(LogParserEventArgs log)
        {
            appDispatcher.DispatchUI(() =>
            {
                LineReadEvent?.Invoke(this, log);
            });
        }

        private void Poll(object sender, EventArgs e)
        {
            if (!FindEq.HasLogFiles(settings.DefaultEqDirectory))
            {
                return;
            }
            appDispatcher.DispatchUI(() =>
            {
                var playerchanged = activePlayer.Update();
                if (playerchanged)
                {
                    LastReadOffset = null;
                }
                var filepath = activePlayer.LogFileName;
                if (string.IsNullOrWhiteSpace(filepath))
                {
                    Debug.WriteLine($"No playerfile found!");
                    return;
                }

                try
                {
                    var fileinfo = new FileInfo(filepath);
                    if (!LastReadOffset.HasValue || (LastReadOffset > fileinfo.Length && fileinfo.Length > 0))
                    {
                        Debug.WriteLine($"Player Switched or new Player detected {filepath} {fileinfo.Length}");
                        LastReadOffset = fileinfo.Length;
                        PlayerChangeEvent?.Invoke(this, new PlayerChangeEventArgs());
                    }
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                    using (var reader = new StreamReader(stream))
                    {
                        _ = stream.Seek(LastReadOffset.Value, SeekOrigin.Begin);
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            LastReadOffset = stream.Position;
                            levelLogParse.MatchLevel(line);
                            var matchedzone = ZoneParser.Match(line);
                            if (!string.IsNullOrWhiteSpace(matchedzone))
                            {
                                var p = activePlayer.Player;
                                if (p != null)
                                {
                                    p.Zone = matchedzone;
                                    toolSettingsLoad.Save(settings);
                                }
                            }
                            if (line.Length > 27)
                            {
                                LineReadEvent?.Invoke(this, new LogParserEventArgs { Line = line });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            });
        }

        public void Dispose()
        {
            UITimer.Stop();
            UITimer.Dispose();
            UITimer = null;
        }
    }
}
