using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.Services.Spells.Log;
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
        private readonly EnterWorldParser enterWorldParser;
        private readonly EventsList eventsList;
        private readonly CampCheckParser campCheckParser;
        private readonly DPSLogParse dPSLogParse;

        private bool Processing = false;
        private readonly List<ILogParser> logParsers;

        public LogParser(
            IEnumerable<ILogParser> logParsers,
            CampCheckParser campCheckParser,
            EnterWorldParser enterWorldParser,
            ActivePlayer activePlayer,
            IAppDispatcher appDispatcher,
            EQToolSettings settings,
            DPSLogParse dPSLogParse,
             EventsList eventsList
            )
        {
            this.logParsers = logParsers.ToList();
            this.campCheckParser = campCheckParser;
            this.enterWorldParser = enterWorldParser;
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.eventsList = eventsList;
            this.dPSLogParse = dPSLogParse;
            this.settings = settings;
            UITimer = new System.Timers.Timer(100);
            UITimer.Elapsed += Poll;
            UITimer.Enabled = true;
            LastYouActivity = DateTime.UtcNow.AddMonths(-1);
        }

        public DateTime LastYouActivity { get; private set; }
        private long? LastLogReadOffset { get; set; } = null;

        public void Push(string log)
        {
            appDispatcher.DispatchUI(() =>
            {
                MainRun(log);
            });
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
                if (message.StartsWith("["))
                {
                    return;
                }

                var timestamp = LogFileDateTimeParse.ParseDateTime(date);
                var matched = dPSLogParse.Match(message, timestamp);
                if (matched != null)
                {
                    this.eventsList.Handle(new EventsList.FightHitEventArgs { HitInformation = matched });
                    return;
                }

                foreach (var item in logParsers)
                {
                    if (item.Evaluate(message))
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                App.LogUnhandledException(e, $"LogParser Filename: '{activePlayer.LogFileName}' '{line1}'", this.activePlayer?.Player?.Server);
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
                    var newplayerdetected = false;
                    if (!LastLogReadOffset.HasValue || (LastLogReadOffset > fileinfo.Length && fileinfo.Length > 0))
                    {
                        Debug.WriteLine($"Player Switched or new Player detected {filepath} {fileinfo.Length}");
                        LastLogReadOffset = fileinfo.Length;
                        this.campCheckParser.StillCamping = false;
                        newplayerdetected = true;
                    }
                    var linelist = new List<string>();
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    {
                        var lookbacksize = 4000;
                        if (newplayerdetected && LastLogReadOffset.Value > lookbacksize)
                        {
                            _ = stream.Seek(LastLogReadOffset.Value - lookbacksize, SeekOrigin.Begin);
                            var buffer = new byte[lookbacksize];
                            _ = stream.Read(buffer, 0, lookbacksize);
                            using (var ms = new MemoryStream(buffer))
                            using (var innerstream = new StreamReader(ms))
                            {
                                while (!innerstream.EndOfStream)
                                {
                                    if (enterWorldParser.HasEnteredWorld(innerstream.ReadLine()))
                                    {
                                        this.campCheckParser.HasUsedStartupEnterWorld = true;
                                        Debug.WriteLine("EnteredWorldEvent Player Changed");
                                        this.eventsList.Handle(new EventsList.EnteredWorldArgs());
                                        break;
                                    }
                                }
                            }
                        }
                        _ = stream.Seek(LastLogReadOffset.Value, SeekOrigin.Begin);
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            linelist.Add(line);
                            LastLogReadOffset = stream.Position;
                        }
                    }

                    if (!this.campCheckParser.HasUsedStartupEnterWorld && linelist.Any())
                    {
                        this.campCheckParser.HasUsedStartupEnterWorld = true;
                        Debug.WriteLine("EnteredWorldEvent First Time");
                        this.eventsList.Handle(new EventsList.EnteredWorldArgs());
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
