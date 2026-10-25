using EQTool.Models;
using EQTool.Services.Handlers;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services
{
    public class TriggerTimerManager
    {
        private class ActiveTimer
        {
            public Trigger Trigger;
            public TimerViewModel ViewModel;
            public DateTime EndTimeUtc;
            public TimeSpan Duration;
            public bool EndingFired;

            // placeholder values from the match that (re)started this timer
            public Hashtable Values;

            // Set for a spell trigger's timer, which tracks the landed spell and has no ViewModel
            // of its own (the spell is already listed in the spell window).
            public string SpellName;
            public string SpellTarget;
            public bool IsSpellTimer => SpellName != null;

            // whether the trigger was saved and enabled when this timer (re)started, as opposed to
            // being fired from the editor's Test button
            public bool StartedLive;
        }

        private class ActiveCounter
        {
            public Trigger Trigger;
            public DateTime LastMatchUtc;
            public TimeSpan ResetAfter;
        }

        private readonly object sync = new object();
        private readonly List<ActiveTimer> activeTimers = new List<ActiveTimer>();
        private readonly List<ActiveCounter> activeCounters = new List<ActiveCounter>();

        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;
        private readonly TriggerActionExecutor executor;
        private readonly IAppDispatcher appDispatcher;
        private readonly LogEvents logEvents;
        private readonly EQToolSettings settings;
        private readonly System.Windows.Threading.DispatcherTimer ticker;

        public TriggerTimerManager(SpellWindowViewModel spellWindowViewModel, EQSpells spells, TriggerActionExecutor executor, IAppDispatcher appDispatcher, LogEvents logEvents, EQToolSettings settings)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;
            this.executor = executor;
            this.appDispatcher = appDispatcher;
            this.logEvents = logEvents;
            this.settings = settings;

            ticker = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            ticker.Tick += (s, e) => Tick();
            ticker.Start();

            logEvents.SpellWornOffSelfEvent += (s, e) => EndSpellTimers(e.SpellNames, self: true);
            logEvents.SpellWornOffOtherEvent += LogEvents_SpellWornOffOtherEvent;
        }

        private void LogEvents_SpellWornOffOtherEvent(object sender, SpellWornOffOtherEvent e)
        {
            // a charm break arrives as this generic message; like the spell window, read it as
            // whichever charm spell is running
            var spellNames = e.Line == "Your charm spell has worn off."
                ? SpellHandlerService.AllCharmSpells
                : new List<string> { e.SpellName };
            EndSpellTimers(spellNames, self: false);
        }

        public void HandleTimerMatch(Trigger trigger)
        {
            if (trigger?.Timer == null || !trigger.Timer.IsEnabled)
            {
                return;
            }
            var duration = trigger.Timer.Duration;
            if (duration.TotalMilliseconds <= 0)
            {
                return;
            }
            var name = trigger.Expand(string.IsNullOrWhiteSpace(trigger.Timer.TimerName) ? trigger.TriggerName : trigger.Timer.TimerName);

            lock (sync)
            {
                var existing = activeTimers.FirstOrDefault(a => a.ViewModel != null && string.Equals(a.ViewModel.Name, name, StringComparison.OrdinalIgnoreCase));
                switch (trigger.Timer.RestartBehavior)
                {
                    case TimerRestartBehavior.DoNothing:
                        if (existing != null)
                        {
                            return;
                        }
                        break;
                    case TimerRestartBehavior.RestartTimer:
                        if (existing != null)
                        {
                            existing.EndTimeUtc = DateTime.UtcNow.Add(duration);
                            existing.Duration = duration;
                            existing.EndingFired = false;
                            existing.Values = trigger.CaptureValues();
                            appDispatcher.DispatchUI(() =>
                            {
                                existing.ViewModel.TotalDuration = duration;
                                existing.ViewModel.TotalRemainingDuration = duration;
                            });
                            FireOverlayBar(trigger, name, duration);
                            return;
                        }
                        break;
                    case TimerRestartBehavior.StartNewTimer:
                    default:
                        break;
                }

                var vm = new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = CustomTimer.CustomerTime,
                    Name = name,
                    TotalDuration = duration,
                    TotalRemainingDuration = duration,
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = TriggerColors.ToBrush(trigger.Timer.BarColor, Brushes.MediumPurple)
                };
                var iconName = string.IsNullOrWhiteSpace(trigger.Timer.IconName) ? "Feign Death" : trigger.Timer.IconName;
                if (spells.AllSpells.TryGetValue(iconName, out var spell) || spells.AllSpells.TryGetValue("Feign Death", out spell))
                {
                    vm.Rect = spell.Rect;
                    vm.Icon = spell.SpellIcon;
                }

                activeTimers.Add(new ActiveTimer
                {
                    Trigger = trigger,
                    ViewModel = vm,
                    EndTimeUtc = DateTime.UtcNow.Add(duration),
                    Duration = duration,
                    EndingFired = false,
                    Values = trigger.CaptureValues()
                });
                spellWindowViewModel.TryAdd(vm, allowDuplicates: true);
                FireOverlayBar(trigger, name, duration);
            }
        }

        // Arms Timer Ending / Timer Ended for a spell trigger using the landed spell's duration.
        // One timer per trigger and target: the spell landing again on the same target refreshes it.
        public void HandleSpellMatch(Trigger trigger, string spellName, string targetName, TimeSpan duration)
        {
            if (trigger == null || duration.TotalMilliseconds <= 0)
            {
                return;
            }
            if (trigger.TimerEnding?.Enabled != true && trigger.TimerEnded?.Enabled != true)
            {
                return;
            }

            lock (sync)
            {
                var existing = activeTimers.FirstOrDefault(a => a.IsSpellTimer && a.Trigger == trigger && string.Equals(a.SpellTarget, targetName, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    existing = new ActiveTimer { Trigger = trigger, SpellName = spellName, SpellTarget = targetName };
                    activeTimers.Add(existing);
                }
                existing.EndTimeUtc = DateTime.UtcNow.Add(duration);
                existing.Duration = duration;
                // The duration depends on caster level, so it can come in at or under the Timer
                // Ending threshold; the warning is for counting down to it, not for landing below it.
                existing.EndingFired = trigger.TimerEnding != null && trigger.TimerEnding.Threshold >= duration;
                existing.Values = trigger.CaptureValues();
                existing.StartedLive = trigger.TriggerEnabled && settings.Triggers.Contains(trigger);
            }
        }

        // A spell timer can run for hours with no spell window row of its own to dismiss, so it is
        // dropped once its trigger no longer stands behind it: switched to another spell or type,
        // or (when it started live) disabled or deleted.
        private bool IsAbandoned(ActiveTimer t)
        {
            if (!t.IsSpellTimer)
            {
                return false;
            }
            if (!t.Trigger.IsSpellTrigger || !string.Equals(t.Trigger.SpellName?.Trim(), t.SpellName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return t.StartedLive && (!t.Trigger.TriggerEnabled || !settings.Triggers.Contains(t.Trigger));
        }

        // A spell can end before its computed duration (dispelled, root broke, clicked off), so its
        // "worn off" message ends the matching spell timer and fires Timer Ended then. Like the spell
        // window, this only acts when the message identifies a single timer.
        private void EndSpellTimers(List<string> spellNames, bool self)
        {
            if (spellNames == null || !spellNames.Any())
            {
                return;
            }

            ActiveTimer ended;
            lock (sync)
            {
                var matches = activeTimers.Where(a => a.IsSpellTimer && !IsAbandoned(a) &&
                    string.Equals(a.SpellTarget, EQSpells.SpaceYou, StringComparison.OrdinalIgnoreCase) == self &&
                    spellNames.Any(n => string.Equals(n, a.SpellName, StringComparison.OrdinalIgnoreCase))).ToList();
                if (matches.Count != 1)
                {
                    return;
                }
                ended = matches[0];
                _ = activeTimers.Remove(ended);
            }

            if (ended.Trigger.TimerEnded != null && ended.Trigger.TimerEnded.Enabled)
            {
                executor.Execute(ended.Trigger.TimerEnded.Output, text => ended.Trigger.Expand(text, ended.Values));
            }
        }

        // When "Show in overlay" is checked, mirrors the timer's countdown as an animated bar in
        // the overlay window. The bar is labeled with the timer/trigger name (not the Basic text).
        private void FireOverlayBar(Trigger trigger, string name, TimeSpan duration)
        {
            if (trigger?.Timer == null || !trigger.Timer.ShowInOverlay)
            {
                return;
            }
            logEvents.Handle(new TimerBarEvent
            {
                Name = name,
                TotalSeconds = (int)Math.Ceiling(duration.TotalSeconds),
                BarColor = TriggerColors.ToBrush(trigger.Timer.BarColor, Brushes.MediumPurple)
            });
        }

        // Called when a trigger with counter-reset enabled matches. This only arms the inactivity
        // reset for the trigger's {COUNTER} tally; it does not display anything in the spells window
        // (only a configured Timer shows there).
        public void HandleCounterMatch(Trigger trigger)
        {
            if (trigger?.Counter == null || !trigger.Counter.ResetEnabled || trigger.Counter.ResetAfter.TotalMilliseconds <= 0)
            {
                return;
            }

            lock (sync)
            {
                var existing = activeCounters.FirstOrDefault(a => a.Trigger == trigger);
                if (existing == null)
                {
                    activeCounters.Add(new ActiveCounter { Trigger = trigger, LastMatchUtc = DateTime.UtcNow, ResetAfter = trigger.Counter.ResetAfter });
                }
                else
                {
                    existing.LastMatchUtc = DateTime.UtcNow;
                    existing.ResetAfter = trigger.Counter.ResetAfter;
                }
            }
        }

        private void Tick()
        {
            Tick(DateTime.UtcNow);
        }

        // takes the clock as a parameter so tests can step past a timer without waiting on it
        internal void Tick(DateTime now)
        {
            var endingToFire = new List<ActiveTimer>();
            var endedToFire = new List<ActiveTimer>();

            lock (sync)
            {
                foreach (var t in activeTimers.ToList())
                {
                    if (IsAbandoned(t))
                    {
                        _ = activeTimers.Remove(t);
                        continue;
                    }

                    var remaining = t.EndTimeUtc - now;

                    if (!t.EndingFired && t.Trigger?.TimerEnding != null && t.Trigger.TimerEnding.Enabled)
                    {
                        var threshold = t.Trigger.TimerEnding.Threshold;
                        if (threshold.TotalMilliseconds > 0 && remaining.TotalMilliseconds <= threshold.TotalMilliseconds && remaining.TotalMilliseconds > 0)
                        {
                            t.EndingFired = true;
                            endingToFire.Add(t);
                        }
                    }

                    if (remaining.TotalMilliseconds <= 0)
                    {
                        if (t.Trigger?.TimerEnded != null && t.Trigger.TimerEnded.Enabled)
                        {
                            endedToFire.Add(t);
                        }

                        if (!t.IsSpellTimer && t.Trigger?.Timer != null && t.Trigger.Timer.TimerType == TimerType.RepeatingTimer)
                        {
                            // re-arm a repeating timer
                            t.EndTimeUtc = now.Add(t.Duration);
                            t.EndingFired = false;
                            appDispatcher.DispatchUI(() =>
                            {
                                t.ViewModel.TotalDuration = t.Duration;
                                t.ViewModel.TotalRemainingDuration = t.Duration;
                                spellWindowViewModel.TryAdd(t.ViewModel, allowDuplicates: true);
                            });
                            FireOverlayBar(t.Trigger, t.ViewModel.Name, t.Duration);
                        }
                        else
                        {
                            _ = activeTimers.Remove(t);
                        }
                    }
                }

                foreach (var c in activeCounters.ToList())
                {
                    if (c.ResetAfter.TotalMilliseconds > 0 && (now - c.LastMatchUtc) >= c.ResetAfter)
                    {
                        // zero the {COUNTER} macro tally so it restarts on the next match
                        c.Trigger.CurrentCounter = 0;
                        _ = activeCounters.Remove(c);
                    }
                }
            }

            foreach (var t in endingToFire)
            {
                executor.Execute(t.Trigger.TimerEnding.Output, text => t.Trigger.Expand(text, t.Values));
            }
            foreach (var t in endedToFire)
            {
                executor.Execute(t.Trigger.TimerEnded.Output, text => t.Trigger.Expand(text, t.Values));
            }
        }
    }
}
