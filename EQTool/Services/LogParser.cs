using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EQTool.Services
{
    public class LogParser : IDisposable
    {
        private System.Timers.Timer UITimer;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private string LastLogFilename = string.Empty;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly List<IEqLogParseHandler> eqLogParseHandlers;
        private readonly LogEvents logEvents;
        private bool Processing = false;
        public DateTime LastYouActivity { get; private set; }
        private long? LastLogReadOffset { get; set; } = null;

        public LogParser(
            IEnumerable<IEqLogParseHandler> eqLogParseHandlers,
            EQToolSettingsLoad toolSettingsLoad,
            ActivePlayer activePlayer,
            IAppDispatcher appDispatcher,
            EQToolSettings settings,
             LogEvents logEvents
            )
        {
            this.eqLogParseHandlers = eqLogParseHandlers.ToList();
            this.logEvents = logEvents;
            this.toolSettingsLoad = toolSettingsLoad;
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.settings = settings;
            UITimer = new System.Timers.Timer(100);
            UITimer.Elapsed += Poll;
            UITimer.Enabled = true;
            LastYouActivity = DateTime.UtcNow.AddMonths(-1);
            this.logEvents.LevelEvent += LogEvents_LevelEvent;
            this.logEvents.YouZonedEvent += LogEvents_YouZonedEvent;
        }

        private void LogEvents_YouZonedEvent(object sender, YouZonedEvent e)
        {
            var p = activePlayer.Player;
            if (p != null)
            {
                p.Zone = e.ShortName;
                toolSettingsLoad.Save(settings);
            }
        }

        private void LogEvents_LevelEvent(object sender, LevelEvent e)
        {
            var player = activePlayer.Player;
            if (player != null)
            {
                player.Level = e.NewLevel;
            }
        }

        public void Push(string line)
        {
            appDispatcher.DispatchUI(() =>
            {
                MainRun(line);
            });
        }

        public void Push(string message, DateTime datetime)
        {
            var logtext = message?.Trim();
            if (string.IsNullOrWhiteSpace(logtext))
            {
                return;
            }
            if (!logtext.StartsWith("["))
            {
                var format = "ddd MMM dd HH:mm:ss yyyy";
                var d = datetime;
                logtext = "[" + d.ToString(format) + "] " + logtext;
            }
            Push(logtext);
        }

        private void MainRun(string line1)
        {
            if (line1 == null || line1.Length < 27)
            {
                return;
            }
            try
            {
                var date = line1.Substring(1, 24);
                var message = line1.Substring(27).Trim();
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }

                if (message.StartsWith("You"))
                {
                    LastYouActivity = DateTime.UtcNow;
                }

                var timestamp = LogFileDateTimeParse.ParseDateTime(date);
                foreach (var handler in eqLogParseHandlers)
                {
                    if (handler.Handle(message, timestamp))
                    {
                        Debug.WriteLine($"--Handled by {handler.GetType().Name}: {message}");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                App.LogUnhandledException(e, $"LogParser Filename: '{activePlayer.LogFileName}' '{line1}'", activePlayer?.Player?.Server);
            }
        }

        private void Poll(object sender, EventArgs e)
        {
            if (Processing)
            {
                return;
            }
            Processing = true;
            FindEq.LogFileInfo logfounddata = null;
            try
            {
                logfounddata = FindEq.GetLogFileLocation(new FindEq.FindEQData { EqBaseLocation = settings.DefaultEqDirectory, EQlogLocation = settings.EqLogDirectory });
            }
            catch { }
            if (logfounddata == null || !logfounddata.Found)
            {
                Processing = false;
                return;
            }
            settings.EqLogDirectory = logfounddata.Location;
            appDispatcher.DispatchUI(() =>
            {
                try
                {
                    var playerchanged = activePlayer.Update();
                    var filepath = activePlayer.LogFileName;
                    if (playerchanged || filepath != LastLogFilename)
                    {
                        LastLogReadOffset = null;
                        LastLogFilename = filepath;
                    }

                    if (string.IsNullOrWhiteSpace(filepath))
                    {
                        Debug.WriteLine($"No playerfile found!");
                        return;
                    }

                    var fileinfo = new FileInfo(filepath);
                    if (!LastLogReadOffset.HasValue || (LastLogReadOffset > fileinfo.Length && fileinfo.Length > 0))
                    {
                        Debug.WriteLine($"Player Switched or new Player detected {filepath} {fileinfo.Length}");
                        LastLogReadOffset = fileinfo.Length;
                        logEvents.Handle(new PayerChangedEvent { TimeStamp = DateTime.Now });
                    }
                    var linelist = new List<string>();
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    {
                        _ = stream.Seek(LastLogReadOffset.Value, SeekOrigin.Begin);
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            linelist.Add(line);
                            LastLogReadOffset = stream.Position;
                        }
                    }
                    foreach (var line in linelist)
                    {
                        MainRun(line);
                    }
                }
                catch (Exception ex) when (!(ex is System.IO.IOException) && !(ex is UnauthorizedAccessException))
                {
                    App.LogUnhandledException(ex, "LogParser DispatchUI", activePlayer?.Player?.Server);
                }
                finally
                {
                    Processing = false;
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
