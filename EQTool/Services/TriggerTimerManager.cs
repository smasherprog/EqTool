using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
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
        private readonly System.Windows.Threading.DispatcherTimer ticker;

        public TriggerTimerManager(SpellWindowViewModel spellWindowViewModel, EQSpells spells, TriggerActionExecutor executor, IAppDispatcher appDispatcher, LogEvents logEvents)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;
            this.executor = executor;
            this.appDispatcher = appDispatcher;
            this.logEvents = logEvents;

            ticker = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            ticker.Tick += (s, e) => Tick();
            ticker.Start();

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
                var existing = activeTimers.FirstOrDefault(a => string.Equals(a.ViewModel.Name, name, StringComparison.OrdinalIgnoreCase));
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
                    EndingFired = false
                });
                spellWindowViewModel.TryAdd(vm, allowDuplicates: true);
                FireOverlayBar(trigger, name, duration);
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
            var now = DateTime.UtcNow;
            var endingToFire = new List<ActiveTimer>();
            var endedToFire = new List<ActiveTimer>();

            lock (sync)
            {
                foreach (var t in activeTimers.ToList())
                {
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

                        if (t.Trigger?.Timer != null && t.Trigger.Timer.TimerType == TimerType.RepeatingTimer)
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
                executor.Execute(t.Trigger.TimerEnding.Output, t.Trigger.Expand);
            }
            foreach (var t in endedToFire)
            {
                executor.Execute(t.Trigger.TimerEnded.Output, t.Trigger.Expand);
            }
        }
    }
}
