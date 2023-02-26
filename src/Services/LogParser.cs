using EQTool.Models;
using EQTool.Services.Map;
using EQTool.Services.Spells.Log;
using EQTool.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows.Media.Media3D;
using static EQTool.Services.Spells.Log.LogCustomTimer;

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
        private readonly LocationParser locationParser;
        private readonly ZoneViewModel zoneViewModel;
        private readonly DPSLogParse dPSLogParse;
        private readonly LogDeathParse logDeathParse;
        private readonly ConLogParse conLogParse;
        private readonly LogCustomTimer logCustomTimer;
        private readonly SpellLogParse spellLogParse;
        private readonly SpellWornOffLogParse spellWornOffLogParse;

        public LogParser(
            SpellWornOffLogParse spellWornOffLogParse,
            SpellLogParse spellLogParse,
            LogCustomTimer logCustomTimer,
            ConLogParse conLogParse,
            LogDeathParse logDeathParse,
            DPSLogParse dPSLogParse,
            ZoneViewModel zoneViewModel,
            LocationParser locationParser,
            EQToolSettingsLoad toolSettingsLoad,
            ActivePlayer activePlayer,
            IAppDispatcher appDispatcher,
            EQToolSettings settings,
            LevelLogParse levelLogParse)
        {
            this.spellWornOffLogParse = spellWornOffLogParse;
            this.spellLogParse = spellLogParse;
            this.logCustomTimer = logCustomTimer;
            this.conLogParse = conLogParse;
            this.logDeathParse = logDeathParse;
            this.dPSLogParse = dPSLogParse;
            this.zoneViewModel = zoneViewModel;
            this.locationParser = locationParser;
            this.toolSettingsLoad = toolSettingsLoad;
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.levelLogParse = levelLogParse;
            this.settings = settings;
            UITimer = new System.Timers.Timer(100);
            UITimer.Elapsed += Poll;
            UITimer.Enabled = true;
        }

        public class PlayerChangeEventArgs : EventArgs
        {
        }

        public class PlayerZonedEventArgs : EventArgs
        {
            public string Zone { get; set; }
        }
        public class PlayerLocationEventArgs : EventArgs
        {
            public Point3D Location { get; set; }
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
            public CustomerTimer CustomerTimer { get; set; }
        }

        public class CancelTimerEventArgs : EventArgs
        {
            public string Name { get; set; }
        }

        public class StartCastingEventArgs : EventArgs
        {
            public SpellParsingMatch Spell { get; set; }
        }

        public class StartWornsOffEventArgs : EventArgs
        {
            public string SpellName { get; set; }
        }

        public event EventHandler<StartWornsOffEventArgs> SpellWornsOffEvent;

        public event EventHandler<StartCastingEventArgs> StartCastingEvent;

        public event EventHandler<CancelTimerEventArgs> CancelTimerEvent;

        public event EventHandler<StartTimerEventArgs> StartTimerEvent;

        public event EventHandler<ConEventArgs> ConEvent;

        public event EventHandler<DeadEventArgs> DeadEvent;

        public event EventHandler<FightHitEventArgs> FightHitEvent;

        public event EventHandler<PlayerChangeEventArgs> PlayerChangeEvent;

        public event EventHandler<PlayerZonedEventArgs> PlayerZonedEvent;

        public event EventHandler<PlayerLocationEventArgs> PlayerLocationEvent;


        public void Push(string log)
        {
            appDispatcher.DispatchUI(() =>
            {
                MainRun(log);
            });
        }

        private void MainRun(string line)
        {
            var pos = locationParser.Match(line);
            if (pos.HasValue)
            {
                PlayerLocationEvent?.Invoke(this, new PlayerLocationEventArgs { Location = pos.Value });
            }

            var matched = dPSLogParse.Match(line);
            if (matched != null)
            {
                FightHitEvent?.Invoke(this, new FightHitEventArgs { HitInformation = matched });
            }

            var name = logDeathParse.GetDeadTarget(line);
            if (!string.IsNullOrWhiteSpace(name))
            {
                DeadEvent?.Invoke(this, new DeadEventArgs { Name = name });
            }

            name = conLogParse.ConMatch(line);
            if (!string.IsNullOrWhiteSpace(name))
            {
                ConEvent?.Invoke(this, new ConEventArgs { Name = name });
            }

            var customtimer = logCustomTimer.GetStartTimer(line);
            if (customtimer != null)
            {
                StartTimerEvent?.Invoke(this, new StartTimerEventArgs { CustomerTimer = customtimer });
            }

            name = logCustomTimer.GetCancelTimer(line);
            if (!string.IsNullOrWhiteSpace(name))
            {
                CancelTimerEvent?.Invoke(this, new CancelTimerEventArgs { Name = name });
            }

            var matchedspell = spellLogParse.MatchSpell(line);
            if (matchedspell != null)
            {
                StartCastingEvent?.Invoke(this, new StartCastingEventArgs { Spell = matchedspell });
            }

            name = spellWornOffLogParse.MatchSpell(line);
            if (!string.IsNullOrWhiteSpace(name))
            {
                SpellWornsOffEvent?.Invoke(this, new StartWornsOffEventArgs { SpellName = name });
            }

            levelLogParse.MatchLevel(line);
            var matchedzone = ZoneParser.Match(line);
            if (!string.IsNullOrWhiteSpace(matchedzone))
            {
                var p = activePlayer.Player;
                if (p != null && p.Zone != zoneViewModel.Name)
                {
                    zoneViewModel.Name = matchedzone;
                    p.Zone = matchedzone;
                    toolSettingsLoad.Save(settings);
                }
                PlayerZonedEvent?.Invoke(this, new PlayerZonedEventArgs { Zone = matchedzone });
            }
            if (line.Length > 27)
            {
                // LineReadEvent?.Invoke(this, new LogParserEventArgs { Line = line });
            }
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
                            MainRun(line);
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
