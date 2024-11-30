using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class SlainHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly PlayerTrackerService playerTrackerService;
        private readonly IAppDispatcher appDispatcher;
        private readonly EQSpells spells;
        private readonly FightHistory fightHistory;

        private string Victim = string.Empty;
        private string Killer = string.Empty;
        private readonly List<string> FactionMessages = new List<string>();
        private bool ExpMessage = false;
        private int LineNumber = -100;//just some value that can never exist
        private bool AlreadyEmitted = false;

        public SlainHandler(
            PlayerTrackerService playerTrackerService,
            IAppDispatcher appDispatcher,
            LogEvents logEvents,
            EQSpells spells,
            FightHistory fightHistory,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            SpellWindowViewModel spellWindowViewModel,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.fightHistory = fightHistory;
            this.spells = spells;
            this.appDispatcher = appDispatcher;
            this.playerTrackerService = playerTrackerService;
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.SlainEvent += LogEvents_SlainEvent;
            this.logEvents.FactionEvent += LogEvents_FactionEvent;
            this.logEvents.ExpGainedEvent += LogEvents_ExperienceGainedEvent;
            this.logEvents.PayerChangedEvent += LogEvents_PayerChangedEvent;
            this.logEvents.LineEvent += LogEvents_LineEvent;
            this.logEvents.CommsEvent += LogEvents_CommsEvent;
        }

        private void LogEvents_CommsEvent(object sender, CommsEvent e)
        {
            if (e.TheChannel == CommsEvent.Channel.SAY && MasterNPCList.NPCs.Contains(e.Sender))
            {
                //Reset();
            }
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            if (e.LineCounter == LineNumber)
            {
                return;
            }
            else if (e.LineCounter - 1 == LineNumber)
            {
                if (AlreadyEmitted)
                {
                    Reset();
                }
                else if (ExpMessage || FactionMessages.Any())
                {
                    DoEvent(new ConfirmedDeathEvent
                    {
                        Killer = Killer,
                        Victim = Victim,
                        Line = e.Line,
                        LineCounter = e.LineCounter,
                        TimeStamp = e.TimeStamp,
                    }, true);
                    Reset();
                }
            }
        }

        private void LogEvents_PayerChangedEvent(object sender, PayerChangedEvent e)
        {
            Reset();
        }

        private void Reset()
        {
            LineNumber = -100;
            Killer = Victim = string.Empty;
            FactionMessages.Clear();
            ExpMessage = AlreadyEmitted = false;
        }

        private void LogEvents_ExperienceGainedEvent(object sender, ExpGainedEvent e)
        {
            Victim = "Experience Slain Guess";
            Killer = "You";
            if (ExpMessage)
            {
                DoEvent(new ConfirmedDeathEvent
                {
                    Killer = Killer,
                    Victim = Victim,
                    Line = e.Line,
                    LineCounter = e.LineCounter,
                    TimeStamp = e.TimeStamp,
                }, true);
                Reset();
                AlreadyEmitted = true;
            }
            Victim = "Experience Slain Guess";
            Killer = "You";
            LineNumber = e.LineCounter;
            ExpMessage = true;
        }

        private void LogEvents_FactionEvent(object sender, FactionEvent e)
        {
            LineNumber = e.LineCounter;
            Victim = "Faction Slain Guess";
            Killer = "You";

            if ((FactionMessages.Any() && FactionMessages[0] == e.Line) || FactionMessages.Count == 5)
            {
                DoEvent(new ConfirmedDeathEvent
                {
                    Killer = Killer,
                    Victim = Victim,
                    Line = e.Line,
                    LineCounter = e.LineCounter,
                    TimeStamp = e.TimeStamp,
                }, true);
                Reset();
            }
            LineNumber = e.LineCounter;
            Victim = "Faction Slain Guess";
            Killer = "You";
            FactionMessages.Add(e.Line);

        }

        private void LogEvents_SlainEvent(object sender, SlainEvent e)
        {
            Victim = e.Victim;
            Killer = e.Killer;
            LineNumber = e.LineCounter;
            DoEvent(new ConfirmedDeathEvent
            {
                Killer = Killer,
                Victim = Victim,
                Line = e.Line,
                LineCounter = e.LineCounter,
                TimeStamp = e.TimeStamp,
            }, false);
            AlreadyEmitted = true;
        }

        private int deathcounter = 1;
        private void DoEvent(ConfirmedDeathEvent e, bool guess)
        {
            fightHistory.Remove(Victim, e.TimeStamp);
            logEvents.Handle(new ConfirmedDeathEvent
            {
                Killer = Killer,
                Victim = Victim,
                Line = e.Line,
                LineCounter = e.LineCounter,
                TimeStamp = e.TimeStamp,
            });

            if (playerTrackerService.IsPlayer(e.Victim) || (!MasterNPCList.NPCs.Contains(e.Victim) && !guess))
            {
                return;
            }
            var zonetimer = ZoneSpawnTimes.GetSpawnTime(e.Victim, activePlayer?.Player?.Zone);
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == "Disease Cloud");
            var add = new TimerViewModel
            {
                Name = "--Dead-- " + e.Victim,
                TotalDuration = TimeSpan.FromSeconds((int)zonetimer.TotalSeconds),
                TotalRemainingDuration = TimeSpan.FromSeconds((int)zonetimer.TotalSeconds),
                Icon = spell.SpellIcon,
                Rect = spell.Rect,
                PercentLeft = 100,
                GroupName = CustomTimer.CustomerTime,
                ProgressBarColor = Brushes.LightSalmon
            };
            appDispatcher.DispatchUI(() =>
            {
                var grpname = e.Victim;
                if (MasterNPCList.NPCs.Contains(grpname))
                {
                    grpname = " " + grpname;
                }
                var grp = spellWindowViewModel.SpellList.Where(a => string.Equals(a.GroupName, grpname, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var item in grp)
                {
                    _ = spellWindowViewModel.SpellList.Remove(item);
                }
                var exisitngdeathentry = spellWindowViewModel.SpellList.FirstOrDefault(a => string.Equals(a.Name == add.Name, StringComparison.OrdinalIgnoreCase) && CustomTimer.CustomerTime == a.GroupName);
                if (exisitngdeathentry != null)
                {
                    deathcounter = ++deathcounter > 999 ? 1 : deathcounter;
                    add.Name += "_" + deathcounter;
                }

                spellWindowViewModel.TryAdd(add);
            });
        }
    }
}
