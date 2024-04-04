using EQTool.Models;
using EQTool.Services.Map;
using EQTool.Services.Parsing;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using EQToolShared.Map;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static EQTool.Services.EnrageParser;
using static EQTool.Services.EventsList;
using static EQTool.Services.FindEq;

namespace EQTool.Services
{
    public class LogParser : IDisposable
    {
        private System.Timers.Timer UITimer;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private string LastLogFilename = string.Empty;
        private readonly EQToolSettings settings;
        private readonly LevelLogParse levelLogParse;
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly LocationParser locationParser;
        private readonly DPSLogParse dPSLogParse;
        private readonly LogDeathParse logDeathParse;
        private readonly ConLogParse conLogParse;
        private readonly LogCustomTimer logCustomTimer;
        private readonly SpellLogParse spellLogParse;
        private readonly SpellWornOffLogParse spellWornOffLogParse;
        private readonly PlayerWhoLogParse playerWhoLogParse;
        private readonly EnterWorldParser enterWorldParser;
        private readonly QuakeParser quakeParser;
        private readonly POFDTParser pOFDTParser;
        private readonly EnrageParser enrageParser;
        private readonly ChParser chParser;
        private readonly InvisParser invisParser;
        private readonly LevParser levParser;
        private readonly FTEParser fTEParser;
        private readonly CharmBreakParser charmBreakParser;
        private readonly FailedFeignParser failedFeignParser;
        private readonly GroupInviteParser groupInviteParser;
        private readonly ResistSpellParser resistSpellParser;
        private readonly RandomParser randomParser;
        private readonly EventsList eventsList;
        private readonly CampCheckParser campCheckParser;


        private bool Processing = false;
        private readonly IEnumerable<ILogParser> logParsers;

        public LogParser(
            IEnumerable<ILogParser> logParsers,
            CampCheckParser campCheckParser,
            GroupInviteParser groupInviteParser,
            CharmBreakParser charmBreakParser,
            FTEParser fTEParser,
            ChParser chParser,
            QuakeParser quakeParser,
            EnterWorldParser enterWorldParser,
            SpellWornOffLogParse spellWornOffLogParse,
            SpellLogParse spellLogParse,
            LogCustomTimer logCustomTimer,
            ConLogParse conLogParse,
            LogDeathParse logDeathParse,
            DPSLogParse dPSLogParse,
            LocationParser locationParser,
            EQToolSettingsLoad toolSettingsLoad,
            ActivePlayer activePlayer,
            IAppDispatcher appDispatcher,
            EQToolSettings settings,
            POFDTParser pOFDTParser,
            EnrageParser enrageParser,
            LevelLogParse levelLogParse,
            PlayerWhoLogParse playerWhoLogParse,
            InvisParser invisParser,
            LevParser levParser,
            FailedFeignParser failedFeignParser,
            EventsList eventsList
            )
        {
            this.logParsers = logParsers;
            this.campCheckParser = campCheckParser;
            this.groupInviteParser = groupInviteParser;
            this.failedFeignParser = failedFeignParser;
            this.charmBreakParser = charmBreakParser;
            this.fTEParser = fTEParser;
            this.invisParser = invisParser;
            this.levParser = levParser;
            this.chParser = chParser;
            this.enrageParser = enrageParser;
            this.pOFDTParser = pOFDTParser;
            this.quakeParser = quakeParser;
            this.enterWorldParser = enterWorldParser;
            this.spellWornOffLogParse = spellWornOffLogParse;
            this.spellLogParse = spellLogParse;
            this.logCustomTimer = logCustomTimer;
            this.conLogParse = conLogParse;
            this.logDeathParse = logDeathParse;
            this.dPSLogParse = dPSLogParse;
            this.locationParser = locationParser;
            this.toolSettingsLoad = toolSettingsLoad;
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.levelLogParse = levelLogParse;
            this.settings = settings;
            this.playerWhoLogParse = playerWhoLogParse;
            UITimer = new System.Timers.Timer(100);
            UITimer.Elapsed += Poll;
            UITimer.Enabled = true;
            LastYouActivity = DateTime.UtcNow.AddMonths(-1);
        }

        public DateTime LastYouActivity { get; private set; }

        private long? LastLogReadOffset { get; set; } = null;
        private string previousLine = string.Empty;

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
                    this.eventsList.Handle(new FightHitEventArgs { HitInformation = matched });
                    return;
                }

                foreach (var item in logParsers)
                {
                    if (item.Evaluate(message, previousLine))
                    {
                        break;
                    }
                }
                previousLine = message;

                var pos = locationParser.Match(message);
                if (pos.HasValue)
                {
                    this.eventsList.Handle(new PlayerLocationEventArgs { Location = pos.Value, PlayerInfo = activePlayer.Player });
                    return;
                }

                var name = logDeathParse.GetDeadTarget(message);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    this.eventsList.Handle(new DeadEventArgs { Name = name });
                    return;
                }

                name = conLogParse.ConMatch(message);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    this.eventsList.Handle(new ConEventArgs { Name = name });
                    return;
                }

                var customtimer = logCustomTimer.GetStartTimer(message);
                if (customtimer != null)
                {
                    this.eventsList.Handle(new StartTimerEventArgs { CustomTimer = customtimer });
                    return;
                }

                name = logCustomTimer.GetCancelTimer(message);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    this.eventsList.Handle(new CancelTimerEventArgs { Name = name });
                    return;
                }

                var didcharmbreak = this.charmBreakParser.DidCharmBreak(message);
                if (didcharmbreak)
                {
                    this.eventsList.Handle(new CharmBreakArgs());
                    return;
                }

                if (message == "The screams fade away.")
                {
                    this.eventsList.Handle(new SpellWornOffOtherEventArgs { SpellName = "Soul Consumption" });
                    return;
                }

                var matchedspell = spellLogParse.MatchSpell(message);
                if (matchedspell != null)
                {
                    this.eventsList.Handle(new SpellEventArgs { Spell = matchedspell });
                    return;
                }

                var resistspell = this.resistSpellParser.ParseNPCSpell(message);
                if (resistspell != null)
                {
                    ResistSpellEvent?.Invoke(this, resistspell);
                    return;
                }

                name = spellWornOffLogParse.MatchWornOffOtherSpell(message);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    this.eventsList.Handle(new SpellWornOffOtherEventArgs { SpellName = name });
                    return;
                }

                var spells = spellWornOffLogParse.MatchWornOffSelfSpell(message);
                if (spells.Any())
                {
                    this.eventsList.Handle(new SpellWornOffSelfEventArgs { SpellNames = spells });
                    return;
                }

                var quaked = quakeParser.IsQuake(message);
                if (quaked)
                {
                    this.eventsList.Handle(new QuakeArgs());
                    return;
                }

                var randomdata = randomParser.Parse(message);
                if (randomdata != null)
                {
                    this.eventsList.Handle(new RandomRollEventArgs { RandomRollData = randomdata });
                    return;
                }

                var dt = this.pOFDTParser.DtCheck(message);
                if (dt != null)
                {
                    POFDTEvent?.Invoke(this, dt);
                    return;
                }

                var enragecheck = this.enrageParser.EnrageCheck(message);
                if (enragecheck != null)
                {
                    EnrageEvent?.Invoke(this, enragecheck);
                    return;
                }

                var chdata = this.chParser.ChCheck(message);
                if (chdata != null)
                {
                    CHEvent?.Invoke(this, chdata);
                    return;
                }

                var lev = this.levParser.Parse(message);
                if (lev.HasValue)
                {
                    LevEvent?.Invoke(this, lev.Value);
                    return;
                }

                var invi = this.invisParser.Parse(message);
                if (invi.HasValue)
                {
                    InvisEvent?.Invoke(this, invi.Value);
                    return;
                }

                var fte = this.fTEParser.Parse(message);
                if (fte != null)
                {
                    FTEEvent?.Invoke(this, fte);
                    return;
                }

                stringmsg = this.groupInviteParser.Parse(message);
                if (!string.IsNullOrWhiteSpace(stringmsg))
                {
                    GroupInviteEvent?.Invoke(this, stringmsg);
                    return;
                }

                levelLogParse.MatchLevel(message);
                var matchedzone = ZoneParser.Match(message);
                if (!string.IsNullOrWhiteSpace(matchedzone))
                {
                    var b4matchedzone = matchedzone;

                    matchedzone = ZoneParser.TranslateToMapName(matchedzone);
                    Debug.WriteLine($"Zone Change Detected {matchedzone}--{b4matchedzone}");
                    var p = activePlayer.Player;
                    if (p != null)
                    {
                        p.Zone = matchedzone;
                        toolSettingsLoad.Save(settings);
                    }
                    this.eventsList.Handle(new PlayerZonedEventArgs { Zone = matchedzone });
                    return;
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
            LogFileInfo logfounddata = null;
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
                                        this.eventsList.Handle(new EnteredWorldArgs());
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
                        this.eventsList.Handle(new EnteredWorldArgs());
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
