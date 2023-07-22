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
using System.Windows.Media.Media3D;

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
        private bool StartingWhoOfZone = false;
        private bool Processing = false;
        private bool StillCamping = false;
        private bool HasUsedStartupEnterWorld = false;

        public LogParser(
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
            LevelLogParse levelLogParse,
            PlayerWhoLogParse playerWhoLogParse)
        {
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
        }

        private long? LastLogReadOffset { get; set; } = null;

        public class PlayerZonedEventArgs : EventArgs
        {
            public string Zone { get; set; }
        }
        public class PlayerLocationEventArgs : EventArgs
        {
            public Point3D Location { get; set; }
            public PlayerInfo PlayerInfo { get; set; }
        }
        public class FightHitEventArgs : EventArgs
        {
            public DPSParseMatch HitInformation { get; set; }
        }
        public class DeadEventArgs : EventArgs
        {
            public string Name { get; set; }
        }

        public class ConEventArgs : EventArgs
        {
            public string Name { get; set; }
        }

        public class StartTimerEventArgs : EventArgs
        {
            public LogCustomTimer.CustomerTimer CustomerTimer { get; set; }
        }

        public class CancelTimerEventArgs : EventArgs
        {
            public string Name { get; set; }
        }

        public class SpellEventArgs : EventArgs
        {
            public SpellParsingMatch Spell { get; set; }
        }

        public class SpellWornOffOtherEventArgs : EventArgs
        {
            public string SpellName { get; set; }
        }

        public class SpellWornOffSelfEventArgs : EventArgs
        {
            public List<string> SpellNames { get; set; }
        }

        public class WhoPlayerEventArgs : EventArgs
        {
            public EQToolShared.APIModels.PlayerControllerModels.Player PlayerInfo { get; set; }
        }
        public class WhoEventArgs : EventArgs { }
        public class CampEventArgs : EventArgs { }
        public class EnteredWorldArgs : EventArgs { }

        public event EventHandler<WhoEventArgs> WhoEvent;

        public event EventHandler<WhoPlayerEventArgs> WhoPlayerEvent;

        public event EventHandler<SpellWornOffSelfEventArgs> SpellWornOffSelfEvent;

        public event EventHandler<SpellWornOffOtherEventArgs> SpellWornOtherOffEvent;

        public event EventHandler<SpellEventArgs> StartCastingEvent;

        public event EventHandler<CancelTimerEventArgs> CancelTimerEvent;

        public event EventHandler<StartTimerEventArgs> StartTimerEvent;

        public event EventHandler<ConEventArgs> ConEvent;

        public event EventHandler<DeadEventArgs> DeadEvent;

        public event EventHandler<FightHitEventArgs> FightHitEvent;

        public event EventHandler<PlayerZonedEventArgs> PlayerZonedEvent;

        public event EventHandler<PlayerLocationEventArgs> PlayerLocationEvent;

        public event EventHandler<CampEventArgs> CampEvent;

        public event EventHandler<EnteredWorldArgs> EnteredWorldEvent;

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

                var timestamp = LogFileDateTimeParse.ParseDateTime(date);
                var pos = locationParser.Match(message);
                if (pos.HasValue)
                {
                    PlayerLocationEvent?.Invoke(this, new PlayerLocationEventArgs { Location = pos.Value, PlayerInfo = activePlayer.Player });
                    return;
                }

                if (message == "It will take about 5 more seconds to prepare your camp.")
                {
                    StillCamping = true;
                    _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        System.Threading.Thread.Sleep(1000 * 6);
                        if (StillCamping)
                        {
                            appDispatcher.DispatchUI(() =>
                            {
                                Debug.WriteLine("CampEvent");
                                CampEvent?.Invoke(this, new CampEventArgs());
                            });
                        }
                    });
                    return;
                }
                else if (message == "You abandon your preparations to camp.")
                {
                    StillCamping = false;
                    return;
                }
                else if (message == "Welcome to EverQuest!")
                {
                    HasUsedStartupEnterWorld = true;
                    Debug.WriteLine("EnteredWorldEvent In Game");
                    EnteredWorldEvent?.Invoke(this, new EnteredWorldArgs());
                    return;
                }

                var playerwho = playerWhoLogParse.ParsePlayerInfo(message);
                if (playerwho != null && StartingWhoOfZone)
                {
                    WhoPlayerEvent?.Invoke(this, new WhoPlayerEventArgs { PlayerInfo = playerwho });
                    return;
                }

                if (playerWhoLogParse.IsZoneWhoLine(message))
                {
                    StartingWhoOfZone = true;
                    WhoEvent?.Invoke(this, new WhoEventArgs());
                    return;
                }
                else
                {
                    StartingWhoOfZone = message == "---------------------------" && StartingWhoOfZone;
                }

                var matched = dPSLogParse.Match(message, timestamp);
                if (matched != null)
                {
                    FightHitEvent?.Invoke(this, new FightHitEventArgs { HitInformation = matched });
                    return;
                }

                var name = logDeathParse.GetDeadTarget(message);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    DeadEvent?.Invoke(this, new DeadEventArgs { Name = name });
                    return;
                }

                name = conLogParse.ConMatch(message);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    ConEvent?.Invoke(this, new ConEventArgs { Name = name });
                    return;
                }

                var customtimer = logCustomTimer.GetStartTimer(message);
                if (customtimer != null)
                {
                    StartTimerEvent?.Invoke(this, new StartTimerEventArgs { CustomerTimer = customtimer });
                    return;
                }

                name = logCustomTimer.GetCancelTimer(message);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    CancelTimerEvent?.Invoke(this, new CancelTimerEventArgs { Name = name });
                    return;
                }

                var matchedspell = spellLogParse.MatchSpell(message);
                if (matchedspell != null)
                {
                    StartCastingEvent?.Invoke(this, new SpellEventArgs { Spell = matchedspell });
                    return;
                }

                name = spellWornOffLogParse.MatchWornOffOtherSpell(message);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    SpellWornOtherOffEvent?.Invoke(this, new SpellWornOffOtherEventArgs { SpellName = name });
                    return;
                }

                var spells = spellWornOffLogParse.MatchWornOffSelfSpell(message);
                if (spells.Any())
                {
                    SpellWornOffSelfEvent?.Invoke(this, new SpellWornOffSelfEventArgs { SpellNames = spells });
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
                    PlayerZonedEvent?.Invoke(this, new PlayerZonedEventArgs { Zone = matchedzone });
                    return;
                }

            }
            catch (Exception e)
            {
                App.LogUnhandledException(e, $"LogParser '{line1}'");
            }
        }

        private void Poll(object sender, EventArgs e)
        {
            if (Processing)
            {
                return;
            }
            var logfounddata = FindEq.GetLogFileLocation(new FindEq.FindEQData { EqBaseLocation = settings.DefaultEqDirectory, EQlogLocation = settings.EqLogDirectory });
            if (logfounddata == null || !logfounddata.Found)
            {
                return;
            }
            settings.EqLogDirectory = logfounddata.Location;
            appDispatcher.DispatchUI(() =>
            {
                if (Processing)
                {
                    return;
                }
                Processing = true;
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
                        StillCamping = false;
                        newplayerdetected = true;
                    }
                    var linelist = new List<string>();
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
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
                                        HasUsedStartupEnterWorld = true;
                                        Debug.WriteLine("EnteredWorldEvent Player Changed");
                                        EnteredWorldEvent?.Invoke(this, new EnteredWorldArgs());
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

                    if (!HasUsedStartupEnterWorld && linelist.Any())
                    {
                        HasUsedStartupEnterWorld = true;
                        Debug.WriteLine("EnteredWorldEvent First Time");
                        EnteredWorldEvent?.Invoke(this, new EnteredWorldArgs());
                    }
                    foreach (var line in linelist)
                    {
                        MainRun(line);
                    }
                }
                catch (Exception ex) when (!(ex is System.IO.IOException))
                {
                    App.LogUnhandledException(ex, "LogParser DispatchUI");
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
