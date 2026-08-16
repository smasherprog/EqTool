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
    public class BardCountHandler : BaseHandler
    {
        private readonly object _lock = new object();
        private readonly List<Session> _sessions = new List<Session>();

        // Accept variants: with/without "the" and "spell"
        private readonly Regex _resistTargetRegex = new Regex(@"^Your target resisted(?: the)? (?<spell>.+?)(?: spell)?\.$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _winceRegex = new Regex(@"\bwinces\.$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // LogParser raises LineEvent for every line, including lines a parser already handled, so a
        // resist arrives here twice: once as ResistSpellEvent and once as LineEvent. Remember the
        // line we already counted so the regex fallback below only fires when ResistParser didn't.
        // Both events are raised on the UI thread (LogParser dispatches MainRun), so no lock needed.
        private int _lastResistLineCounter = -1;

        // Must exceed the log's 1-second timestamp resolution (LogFileDateTimeParse uses
        // "ddd MMM dd HH:mm:ss yyyy"), otherwise two lines on adjacent seconds are 1000ms apart and
        // a burst straddling a whole-second boundary can never merge into one session.
        private const int TrackWindowMillis = 1500;

        // A burst of anonymous winces needs at least this many hits before we alert. One " winces."
        // on its own is far more likely to be someone else's nuke than a bard AE.
        private const int MinimumAnonymousBurst = 2;

        // Landing messages unique to one song each in the spell data, so a match identifies the
        // song outright. Names are spelled to match EQSpells.BardSpellsThatNeedResists.
        private const string StrandsOfMusicLanded = "is bound by silver strands of music";
        private const string ChordsOfMusicLanded = "is bound in chords of music";
        private const string SelosAssonantStrane = "Selo's Assonant Strane";
        private const string SelosChordsOfCessation = "Selo's Chords of Cessation";

        public BardCountHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.LineEvent += LogEvents_LineEvent;
            logEvents.ResistSpellEvent += LogEvents_ResistSpellEvent;
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            if (e?.Line == null)
            {
                return;
            }

            // parse resist lines directly (covers cases where ResistParser didn't produce an event)
            if (e.LineCounter != _lastResistLineCounter)
            {
                var m = _resistTargetRegex.Match(e.Line);
                if (m.Success)
                {
                    var spellName = NormalizeSpellName(m.Groups["spell"].Value);
                    if (IsBardSongThatNeedsResists(spellName))
                    {
                        CreateOrAttachSession(e.TimeStamp, spellName, isResist: true, forceCreate: true);
                        return;
                    }
                }
            }

            // These two landing messages are each unique to a single song in the spell data, so we
            // can name the session outright rather than guessing. Naming matters: an identified
            // session reports unconditionally below, and it puts the song in the summary text.
            if (e.Line.IndexOf(StrandsOfMusicLanded, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreateOrAttachSession(e.TimeStamp, SelosAssonantStrane, hitOnly: true);
                return;
            }

            if (e.Line.IndexOf(ChordsOfMusicLanded, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreateOrAttachSession(e.TimeStamp, SelosChordsOfCessation, hitOnly: true);
                return;
            }

            if (_winceRegex.IsMatch(e.Line))
            {
                // Unlike the two messages above, " winces." is shared by Chords of Dissonance,
                // Denon`s Disruptive Discord and a long tail of unrelated spells (Cannibalize, the
                // mana drains, Denon`s Dissension...), and nothing tells us a bard is singing:
                // YouBeginCastingParser only matches "You begin casting ", so UserCastingSpell is
                // never set for a song. We therefore cannot identify the source here. Count it and
                // let the burst threshold in FinalizeSession discard stray one-off winces.
                CreateOrAttachSession(e.TimeStamp, GetActiveSpellName(), hitOnly: true);
            }
        }

        private void LogEvents_ResistSpellEvent(object sender, ResistSpellEvent e)
        {
            if (e == null || e.Spell?.name == null)
            {
                return;
            }

            // LineEvent for this same line follows immediately; mark it so we don't count it twice.
            _lastResistLineCounter = e.LineCounter;

            // "You resist the X spell!" means someone landed X on us. That is not our cast, so it
            // does not belong in our hit/resist tally.
            if (e.isYou)
            {
                return;
            }

            var spellName = NormalizeSpellName(e.Spell.name);
            if (IsBardSongThatNeedsResists(spellName))
            {
                CreateOrAttachSession(e.TimeStamp, spellName, isResist: true, forceCreate: true);
            }
        }

        private static bool IsBardSongThatNeedsResists(string normalizedName)
        {
            return !string.IsNullOrWhiteSpace(normalizedName)
                && EQSpells.BardSpellsThatNeedResists.Any(a => string.Equals(NormalizeSpellName(a), normalizedName, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsTrackedSpell(string normalizedName)
        {
            return !string.IsNullOrWhiteSpace(normalizedName)
                && (EQSpells.SpellsThatNeedCounts.Any(a => string.Equals(NormalizeSpellName(a), normalizedName, StringComparison.OrdinalIgnoreCase))
                    || IsBardSongThatNeedsResists(normalizedName));
        }

        private void CreateOrAttachSession(DateTime timestamp, string possibleSpell, bool hitOnly = false, bool isResist = false, bool forceCreate = false)
        {
            var normalized = NormalizeSpellName(possibleSpell);

            // If we know the spell name and it's either in the configured list or forced, try to attach/create a named session
            if (!string.IsNullOrWhiteSpace(normalized) && (forceCreate || IsTrackedSpell(normalized)))
            {
                Session s;
                lock (_lock)
                {
                    s = _sessions.Where(a => !string.IsNullOrWhiteSpace(a.SpellName)
                                              && string.Equals(NormalizeSpellName(a.SpellName), normalized, StringComparison.OrdinalIgnoreCase)
                                              && a.LastEventTime.HasValue
                                              && Math.Abs((timestamp - a.LastEventTime.Value).TotalMilliseconds) <= TrackWindowMillis)
                                 .OrderByDescending(a => a.LastEventTime)
                                 .FirstOrDefault();
                    if (s != null)
                    {
                        if (hitOnly)
                        {
                            s.Hits++;
                        }

                        if (isResist)
                        {
                            s.Resists++;
                        }

                        s.LastEventTime = timestamp;
                        ScheduleFinalize(s);
                        return;
                    }

                    // no named session found - try to find a recent anonymous session (created by winces/chains)
                    var anon = _sessions.Where(a => string.IsNullOrWhiteSpace(a.SpellName)
                                                    && a.LastEventTime.HasValue
                                                    && Math.Abs((timestamp - a.LastEventTime.Value).TotalMilliseconds) <= TrackWindowMillis)
                                        .OrderByDescending(a => a.LastEventTime)
                                        .FirstOrDefault();
                    if (anon != null)
                    {
                        anon.SpellName = normalized;
                        if (hitOnly)
                        {
                            anon.Hits++;
                        }

                        if (isResist)
                        {
                            anon.Resists++;
                        }

                        anon.LastEventTime = timestamp;
                        ScheduleFinalize(anon);
                        return;
                    }

                    // no existing session at all - create a new named session (inside lock to avoid races)
                    s = CreateSession(normalized, timestamp);
                    if (hitOnly)
                    {
                        s.Hits = 1;
                    }

                    if (isResist)
                    {
                        s.Resists = 1;
                    }

                    s.LastEventTime = timestamp;
                }

                ScheduleFinalize(s);
                return;
            }

            // anonymous session when spell name unknown or not configured - attach to the most recent session within window
            Session recent;
            lock (_lock)
            {
                recent = _sessions
                    .Where(a => a.LastEventTime.HasValue
                                && Math.Abs((timestamp - a.LastEventTime.Value).TotalMilliseconds) <= TrackWindowMillis)
                    .OrderByDescending(a => a.LastEventTime)
                    .FirstOrDefault();

                if (recent != null)
                {
                    if (hitOnly)
                    {
                        recent.Hits++;
                    }

                    if (isResist)
                    {
                        recent.Resists++;
                    }

                    recent.LastEventTime = timestamp;
                }
                else
                {
                    recent = CreateSession(null, timestamp);
                    if (hitOnly)
                    {
                        recent.Hits = 1;
                    }

                    if (isResist)
                    {
                        recent.Resists = 1;
                    }

                    recent.LastEventTime = timestamp;
                }

                // This event could not be attributed to a song. The lookup above deliberately has no
                // name filter, so an unrelated " winces." can land in a session we named from a
                // landing message - remember that so we don't claim the whole burst was that song.
                recent.HasUnattributedEvents = true;
            }

            ScheduleFinalize(recent);
        }

        private static string NormalizeSpellName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            var n = name.Trim();
            n = n.Replace('`', '\'')
                 .Replace('\u2018', '\'')
                 .Replace('\u2019', '\'')
                 .Replace('\u201C', '"')
                 .Replace('\u201D', '"');
            while (n.Contains("  "))
            {
                n = n.Replace("  ", " ");
            }

            return n;
        }

        private Session CreateSession(string spellName, DateTime start)
        {
            var s = new Session { SpellName = spellName, StartTime = start, LastEventTime = start, Hits = 0, Resists = 0 };
            lock (_lock)
            {
                _sessions.Add(s);
            }

            return s;
        }

        private void ScheduleFinalize(Session s)
        {
            CancellationTokenSource cts;
            lock (_lock)
            {
                if (s.Cts != null)
                {
                    try { s.Cts.Cancel(); } catch { }
                }
                s.Cts = new CancellationTokenSource();
                cts = s.Cts;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TrackWindowMillis, cts.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    return; // rescheduled
                }
                FinalizeSession(s, cts);
            });
        }

        private void FinalizeSession(Session s, CancellationTokenSource expectedCts = null)
        {
            lock (_lock)
            {
                if (expectedCts != null && !ReferenceEquals(s.Cts, expectedCts))
                {
                    return; // a newer schedule exists
                }
                s.Cts = null;
            }

            var removed = false;
            lock (_lock)
            {
                removed = _sessions.Remove(s);
            }
            if (!removed)
            {
                return;
            }

            var total = s.Hits + s.Resists;
            if (total == 0)
            {
                return;
            }

            // An anonymous session means every event in it was an unattributable " winces." line, so
            // require an actual multi-target burst before alerting. Sessions we could identify - by
            // a resist naming the song, or by one of the two unique landing messages - always report.
            if (string.IsNullOrWhiteSpace(s.SpellName) && total < MinimumAnonymousBurst)
            {
                return;
            }

            var parts = new List<string> { $"{total} Total" };
            if (s.Hits > 0)
            {
                parts.Add($"{s.Hits} Hit{(s.Hits == 1 ? "" : "s")}");
            }

            if (s.Resists > 0)
            {
                parts.Add($"{s.Resists} Resist{(s.Resists == 1 ? "" : "s")}");
            }

            var text = string.Join(" | ", parts);
            // Only name the burst when every event in it was attributable to that song; otherwise we
            // would be asserting someone else's winces were ours.
            if (!string.IsNullOrWhiteSpace(s.SpellName) && !s.HasUnattributedEvents)
            {
                text = $"{s.SpellName}: {text}";
            }

            // Persistent record goes to the console window, which actually renders it. This used to
            // raise a synthetic CommsEvent, but nothing displays CommsEvent - the only subscribers
            // are other handlers - so the record was invisible, and raising it from this thread-pool
            // thread pushed those handlers off the UI thread for no benefit.
            debugOutput.WriteLine($"{(s.LastEventTime ?? s.StartTime):HH:mm:ss} {text}", OutputType.Spells);

            var doOverlay = activePlayer?.Player?.BardCountTextAlert ?? false;
            if (doOverlay)
            {
                logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Yellow, Duration = TimeSpan.FromSeconds(3) });
            }

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
                return NormalizeSpellName(activePlayer?.UserCastingSpell?.name);
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
            public DateTime? LastEventTime;
            public int Hits;
            public int Resists;
            public bool HasUnattributedEvents;
            public CancellationTokenSource Cts;
        }
    }
}