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
        private System.Timers.Timer UITimer;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly List<IEqLogParser> eqLogParsers;
        private readonly LineParser lineParser;
        private readonly FileReader fileReader;
        // 0 = idle, 1 = a batch is being read or parsed. Guarded with Interlocked because
        // Poll fires on thread-pool threads; a plain check-then-set race would let two
        // batches' slices interleave and parse log lines out of order.
        private int Processing = 0;
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
             LineParser lineParser
            )
        {
            this.eqLogParsers = eqLogParsers.ToList();
            //below I am forcing the order of parsers because the first one to handle the line wins.
            //So, the parsers should be ordered from most common to least common.
            var spellparsers = this.eqLogParsers
                .Where(a => a.GetType().Name.StartsWith("You") || a.GetType().Name.StartsWith("Spell"))
                .OrderBy(a => a.GetType().Name.StartsWith("You"))
                .ToList();
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
#if !(DEBUG || TEST)
            try
            {
#endif
            var date = line1.Substring(1, 24);
            var message = line1.Substring(27).Trim();
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
            if (message.StartsWith("Your body begins to rot.  You have taken "))
            {
                message = "Your body begins to rot.";
            }
            if (message.StartsWith("Your eardrums rupture.  You have taken "))
            {
                message = "Your eardrums rupture.";
            }

            foreach (var handler in eqLogParsers)
            {
                if (handler.Handle(message, timestamp, LineCounter))
                {
                    Debug.WriteLine($"--Handled by {handler.GetType().Name}: {line1}");
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
            if (System.Threading.Interlocked.CompareExchange(ref Processing, 1, 0) != 0)
            {
                return;
            }
            try
            {
                FindEq.LogFileInfo logfounddata = null;
                try
                {
                    logfounddata = FindEq.GetLogFileLocation(new FindEq.FindEQData { EqBaseLocation = settings.DefaultEqDirectory, EQlogLocation = settings.EqLogDirectory });
                }
                catch { }
                if (logfounddata == null || !logfounddata.Found)
                {
                    return;
                }

                settings.EqLogDirectory = logfounddata.Location;
                UpdatePlayer();
                var filepath = activePlayer.LogFileName;
                if (string.IsNullOrWhiteSpace(filepath))
                {
                    return;
                }
                var linelist = new List<string>();

                try
                {
                    linelist = fileReader.ReadNext(filepath);
                }
                catch (Exception ex)
                {
                    if (!(ex is System.IO.IOException) && !(ex is UnauthorizedAccessException))
                    {
                        App.LogUnhandledException(ex, "LogParser DispatchUI", activePlayer.Player?.Server);
                    }
                }

                var chunks = linelist
                  .Select((item, index) => new { item, index })
                  .GroupBy(x => x.index / 25)
                  .Select(g => g.Select(x => x.item).ToList())
                  .ToList();

                foreach (var chunk in chunks)
                {
                    appDispatcher.DispatchUI(() =>
                    {
                        foreach (var line in chunk)
                        {
                            MainRun(line);
                        }
                    });
                }
            }
            finally
            {
                Processing = 0;
            }
        }

        private void UpdatePlayer()
        {
            var playerchanged = activePlayer.Update(appDispatcher);
            if (playerchanged)
            {
                toolSettingsLoad.Save(settings);
            }
        }

        public void Dispose()
        {
            UITimer.Stop();
            UITimer.Dispose();
            UITimer = null;
        }
    }
}
