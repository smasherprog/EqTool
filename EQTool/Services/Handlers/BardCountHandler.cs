using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    // Aggregate multi-target short bursts (winces / "chains of music" / resists) into a single summary message.
    public class BardCountHandler : BaseHandler
    {
        private readonly object _lock = new object();
        private readonly List<Session> _sessions = new List<Session>();
        private readonly Regex _winceRegex = new Regex(@"\bwinces\.$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _resistTargetRegex = new Regex(@"^Your target resisted the (?<spell>.+?) spell\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _resistYouRegex = new Regex(@"^You resist the (?<spell>.+?) spell!?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private const int TrackWindowMillis = 500; // adjust to your latency

        public BardCountHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.LineEvent += LogEvents_LineEvent;
            logEvents.ResistSpellEvent += LogEvents_ResistSpellEvent;
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            if (e?.Line == null) return;

            // parse resist lines directly (covers cases where ResistParser didn't produce an event)
            var m = _resistTargetRegex.Match(e.Line);
            if (m.Success)
            {
                var spellName = m.Groups["spell"].Value.Trim();
                if (EQSpells.BardSpellsThatNeedResists.Any(a => string.Equals(a, spellName, StringComparison.OrdinalIgnoreCase)))
                {
                    CreateOrAttachSession(e.TimeStamp, spellName, isResist: true, forceCreate: true);
                    return;
                }
            }

            m = _resistYouRegex.Match(e.Line);
            if (m.Success)
            {
                var spellName = m.Groups["spell"].Value.Trim();
                if (EQSpells.BardSpellsThatNeedResists.Any(a => string.Equals(a, spellName, StringComparison.OrdinalIgnoreCase)))
                {
                    CreateOrAttachSession(e.TimeStamp, spellName, isResist: true, forceCreate: true);
                    return;
                }
            }

            // "is bound by silver strands of music" is a per-target message for some bard spells.
            // Treat this as a hit.
            if (e.Line.IndexOf("is bound by silver strands of music", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreateOrAttachSession(e.TimeStamp, GetActiveSpellName(), hitOnly: true);
                return;
            }

            if (e.Line.IndexOf("is bound in chords of music", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreateOrAttachSession(e.TimeStamp, GetActiveSpellName(), hitOnly: true);
                return;
            }

            if (_winceRegex.IsMatch(e.Line))
            {
                // wince counts as a hit occurrence
                CreateOrAttachSession(e.TimeStamp, GetActiveSpellName(), hitOnly: true);
            }
        }

        private void LogEvents_ResistSpellEvent(object sender, ResistSpellEvent e)
        {
            if (e == null || e.Spell?.name == null) return;

            var spellName = e.Spell.name;
            if (EQSpells.BardSpellsThatNeedResists.Any(a => string.Equals(a, spellName, StringComparison.OrdinalIgnoreCase)))
            {
                CreateOrAttachSession(e.TimeStamp, spellName, isResist: true, forceCreate: true);
            }
        }

        // start or attach to an existing session
        private void CreateOrAttachSession(DateTime timestamp, string possibleSpell, bool hitOnly = false, bool isResist = false, bool forceCreate = false)
        {
            // If we know the spell name and it's either in the configured list or forced, try to attach/create a named session
            if (!string.IsNullOrWhiteSpace(possibleSpell) &&
                (forceCreate
                 || EQSpells.SpellsThatNeedCounts.Any(a => a.Equals(possibleSpell, StringComparison.OrdinalIgnoreCase))
                 || EQSpells.BardSpellsThatNeedResists.Any(a => a.Equals(possibleSpell, StringComparison.OrdinalIgnoreCase))))
            {
                lock (_lock)
                {
                    // try to find an existing named session for this spell
                    var s = _sessions.Where(a => !string.IsNullOrWhiteSpace(a.SpellName) && string.Equals(a.SpellName, possibleSpell, StringComparison.OrdinalIgnoreCase)
                                                && Math.Abs((timestamp - a.StartTime).TotalMilliseconds) <= TrackWindowMillis)
                                     .OrderByDescending(a => a.StartTime)
                                     .FirstOrDefault();
                    if (s != null)
                    {
                        if (hitOnly) s.Hits++;
                        if (isResist) s.Resists++;
                        return;
                    }

                    // no named session found � try to find a recent anonymous session (created by winces/chains)
                    var anon = _sessions.Where(a => string.IsNullOrWhiteSpace(a.SpellName)
                                                    && Math.Abs((timestamp - a.StartTime).TotalMilliseconds) <= TrackWindowMillis)
                                        .OrderByDescending(a => a.StartTime)
                                        .FirstOrDefault();
                    if (anon != null)
                    {
                        // attach anonymous session to this spell name so resist/hit are reported together
                        anon.SpellName = possibleSpell;
                        if (hitOnly) anon.Hits++;
                        if (isResist) anon.Resists++;
                        return;
                    }
                }
                    
                // no existing session at all � create a new named session
                var ns = CreateSession(possibleSpell, timestamp);
                if (hitOnly) ns.Hits = 1;
                if (isResist) ns.Resists = 1;
                ScheduleFinalize(ns);
                return;
            }

            // anonymous session when spell name unknown or not configured - attach to the most recent session within window
            lock (_lock)
            {
                // Prefer any recent session (named or anonymous) so lines with identical timestamps are grouped together.
                var recent = _sessions
                    .Where(a => Math.Abs((timestamp - a.StartTime).TotalMilliseconds) <= TrackWindowMillis)
                    .OrderByDescending(a => a.StartTime)
                    .FirstOrDefault();

                if (recent != null)
                {
                    if (hitOnly) recent.Hits++;
                    if (isResist) recent.Resists++;
                    return;
                }

                // no recent session � create a new anonymous session
                var anon = CreateSession(null, timestamp);
                if (hitOnly) anon.Hits = 1;
                if (isResist) anon.Resists = 1;
                ScheduleFinalize(anon);
            }
        }

        private Session CreateSession(string spellName, DateTime start)
        {
            var s = new Session { SpellName = spellName, StartTime = start, Hits = 0, Resists = 0 };
            lock (_lock) _sessions.Add(s);
            return s;
        }

        private void ScheduleFinalize(Session s)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(TrackWindowMillis).ConfigureAwait(false);
                FinalizeSession(s);
            });
        }

        private void FinalizeSession(Session s)
        {
            var removed = false;
            lock (_lock)
            {
                removed = _sessions.Remove(s);
            }
            if (!removed) return;

            var total = s.Hits + s.Resists;
            if (total == 0) return;

            var parts = new List<string> { $"{total} Total" };
            if (s.Hits > 0) parts.Add($"{s.Hits} Hit{(s.Hits == 1 ? "" : "s")}");
            if (s.Resists > 0) parts.Add($"{s.Resists} Resist{(s.Resists == 1 ? "" : "s")}");

            var text = string.Join(" | ", parts);

            // Always emit chat/console summary so there's a persistent record
            var ev = new CommsEvent
            {
                Sender = "System",
                Content = text,
                TheChannel = CommsEvent.Channel.SAY,
                TimeStamp = DateTime.Now,
                Line = text
            };
            logEvents.Handle(ev);

            // Overlay: respect player setting BardCountTextAlert
            var doOverlay = activePlayer?.Player?.BardCountTextAlert ?? false;
            if (doOverlay)
            {
                _ = Task.Factory.StartNew(() =>
                {
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Yellow, Reset = false });
                    Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Yellow, Reset = true });
                });
            }

            // Audio: respect player setting BardCountAudio
            var doAudio = activePlayer?.Player?.BardCountAudio ?? false;
            if (doAudio)
            {
                textToSpeach.Say(text);
            }
        }

        private string GetActiveSpellName()
        {
            try
            {
                return activePlayer?.UserCastingSpell?.name;
            }
            catch
            {
                return null;
            }
        }

        private class Session
        {
            public string SpellName;
            public DateTime StartTime;
            public int Hits;
            public int Resists;
        }
    }
}