using EQTool.Models;
using EQTool.Services.Handlers;
using EQTool.Services.IO;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EQTool.Services
{
    public class LogParser : IDisposable
    {
        private const string TimestampFormat = "ddd MMM dd HH:mm:ss yyyy";
        
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly List<IEqLogParser> eqLogParsers;
        private readonly LineParser lineParser;
        private readonly FileReader fileReader;
        
        private System.Timers.Timer UITimer;
        private bool Processing = false;
        public DateTime LastYouActivity { get; private set; } = DateTime.Now.AddMonths(-1);
        public DateTime LastEntryDateTime { get; private set; } = DateTime.Now;
        private int LineCounter = 0;

        public LogParser(
            IEnumerable<BaseHandler> eqLogParseHandlers, //,_ this forces the creation of all handlers
            IEnumerable<IEqLogParser> eqLogParsers,
            EQToolSettingsLoad toolSettingsLoad,
            ActivePlayer activePlayer,
            IAppDispatcher appDispatcher,
            EQToolSettings settings,
            FileReader fileReader,
            LineParser lineParser)
        {
            this.eqLogParsers = eqLogParsers.ToList();
            //below I am forcing the order of parsers because the first one to handle the line wins.
            //So, the parsers should be ordered from most common to least common.
            var spellparsers = this.eqLogParsers.Where(a => a.GetType().Name.StartsWith("You") || a.GetType().Name.StartsWith("Spell")).ToList();
            foreach (var parser in spellparsers)
            {
                _ = this.eqLogParsers.Remove(parser);
                this.eqLogParsers.Insert(0, parser);
            }

            var commsparser = this.eqLogParsers.OfType<CommsParser>().FirstOrDefault();
            _ = this.eqLogParsers.Remove(commsparser);
            this.eqLogParsers.Insert(0, commsparser);

            var perparser = this.eqLogParsers.OfType<PetParser>().FirstOrDefault();
            _ = this.eqLogParsers.Remove(perparser);
            this.eqLogParsers.Insert(0, perparser);

            var factionparser = this.eqLogParsers.OfType<FactionParser>().FirstOrDefault();
            _ = this.eqLogParsers.Remove(factionparser);
            this.eqLogParsers.Insert(0, factionparser);

            var damageparser = this.eqLogParsers.OfType<DamageParser>().FirstOrDefault();
            _ = this.eqLogParsers.Remove(damageparser);
            this.eqLogParsers.Insert(0, damageparser);

            this.lineParser = lineParser;
            this.toolSettingsLoad = toolSettingsLoad;
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.settings = settings;
            this.fileReader = fileReader;
            UITimer = new System.Timers.Timer(100);
            UITimer.Elapsed += Poll;
            UITimer.Enabled = true;
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
                return;

            if (!HasTimestampPrefix(logtext))
                logtext = $"[{datetime.ToString(TimestampFormat)}] {logtext}";

            Push(logtext);
        }

        private void MainRun(string line)
        {
            if (line == null || line.Length < 27)
            {
                return;
            }
#if !(DEBUG || TEST)
            try
            {
#endif
            var date = line.Substring(1, 24);
            var message = line.Substring(27).Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (message.StartsWith("You"))
            {
                LastYouActivity = DateTime.Now;
            }
            LineCounter += 1;
            var timestamp = LogFileDateTimeParse.ParseDateTime(date);
            LastEntryDateTime = timestamp;
            foreach (var handler in eqLogParsers)
            {
                if (handler.Handle(message, timestamp, LineCounter))
                {
                    Debug.WriteLine($"--Handled by {handler.GetType().Name}: {line}");
                    lineParser.Handle(message, timestamp, LineCounter);
                    return;
                }
            }

            lineParser.Handle(message, timestamp, LineCounter);
#if !(DEBUG || TEST)
            }
            catch (Exception e)
            { 
                App.LogUnhandledException(e, $"LogParser Filename: '{activePlayer.LogFileName}' '{line1}'", activePlayer?.Player?.Server);
            }
#endif
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
                    if (playerchanged)
                    {
                        toolSettingsLoad.Save(settings);
                    }
                    var filepath = activePlayer.LogFileName;
                    if (string.IsNullOrWhiteSpace(filepath))
                    {
                        Debug.WriteLine($"No playerfile found!");
                        return;
                    }

                    var linelist = fileReader.ReadNext(filepath);
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

        private bool HasTimestampPrefix(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            if (!line.StartsWith("["))
                return false;

            var closing = line.IndexOf(']');
            if (closing <= 1)
                return false;

            var inner = line.Substring(1, closing - 1);
            return DateTime.TryParseExact(
                inner,
                TimestampFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out _);
        }
        
        public void Dispose()
        {
            UITimer.Stop();
            UITimer.Dispose();
            UITimer = null;
        }
    }
}
