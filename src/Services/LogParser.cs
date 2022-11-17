using EQTool.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace EQTool.Services
{
    public class LogParser
    {
        private readonly Timer UITimer;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private long? LastReadOffset = null;

        public LogParser(ActivePlayer activePlayer, IAppDispatcher appDispatcher)
        {
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            UITimer = new System.Timers.Timer(100);
            UITimer.Elapsed += Poll;
            UITimer.Enabled = true;
        }

        public class LogParserEventArgs : EventArgs
        {
            public string Line { get; set; }
        }

        public event EventHandler<LogParserEventArgs> LineReadEvent;

        private void Poll(object sender, EventArgs e)
        {
            appDispatcher.DispatchUI(() =>
            {
                var playerchanged = activePlayer.Update();
                var lastreadoffset = LastReadOffset;
                if (playerchanged)
                {
                    LastReadOffset = null;
                    lastreadoffset = null;
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
                    if (!lastreadoffset.HasValue || lastreadoffset > fileinfo.Length)
                    {
                        Debug.WriteLine($"Player Switched or new Player detected");
                        lastreadoffset = fileinfo.Length;
                        LastReadOffset = lastreadoffset;
                    }
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                    using (var reader = new StreamReader(stream))
                    {
                        _ = stream.Seek(lastreadoffset.Value, SeekOrigin.Begin);
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            lastreadoffset = stream.Position;
                            LastReadOffset = lastreadoffset;
                            if (line.Length > 27)
                            {
                                LineReadEvent(this, new LogParserEventArgs { Line = line });
                            }
                        }
                    }
                }
                catch { }
            });
        }
    }
}
