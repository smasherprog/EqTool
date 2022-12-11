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
        public class PlayerChangeEventArgs : EventArgs
        {
        }

        public event EventHandler<LogParserEventArgs> LineReadEvent;

        public event EventHandler<PlayerChangeEventArgs> PlayerChangeEvent;

        public void Push(LogParserEventArgs log)
        {
            appDispatcher.DispatchUI(() =>
            {
                LineReadEvent(this, log);
            });
        }

        private void Poll(object sender, EventArgs e)
        {
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
                    if (!LastReadOffset.HasValue || LastReadOffset > fileinfo.Length)
                    {
                        Debug.WriteLine($"Player Switched or new Player detected");
                        PlayerChangeEvent(this, new PlayerChangeEventArgs());
                        LastReadOffset = fileinfo.Length;
                    }
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                    using (var reader = new StreamReader(stream))
                    {
                        _ = stream.Seek(LastReadOffset.Value, SeekOrigin.Begin);
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            LastReadOffset = stream.Position;
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

        public void Dispose()
        {
            UITimer.Stop();
            UITimer.Dispose();
            UITimer = null;
        }
    }
}
