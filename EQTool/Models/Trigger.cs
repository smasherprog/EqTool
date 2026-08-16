using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EQTool.Models
{
    [Serializable]
    public class Trigger
    {
        // Users may write patterns with simplified {name} placeholders instead of full regex -
        // "^{backstabber} backstabs {target} for {damage} points" is converted to
        // "^(?<backstabber>[\w` ]+) backstabs (?<target>[\w` ]+) for (?<damage>[\w` ]+) points".
        // Captured values land in valueHash and are substituted back into the output fields.
        private const string placeholderRegexPattern = @"\{(?<xxx>\w+)\}";
        private static readonly Regex placeholderRegex = new Regex(placeholderRegexPattern, RegexOptions.Compiled);

        // The {COUNTER} macro, replaced in output fields with the number of times this trigger
        // has matched. Matched case-insensitively, like {c}.
        private static readonly Regex counterTokenRegex = new Regex(@"\{COUNTER\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string _PlayerName { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonIgnore]
        public string PlayerName
        {
            get => _PlayerName;
            set
            {
                if (_PlayerName != value)
                {
                    _PlayerName = value;
                    if (UsesPlayerNameToken)
                    {
                        // regex needs to be recompiled if it contains the {c} macro, since that macro is replaced with the current PlayerName
                        _TriggerRegex = null;
                        _compileFailed = false;
                        _matchTimedOut = false;
                    }
                    CurrentCounter = 0;
                }
            }
        }

        public Guid TriggerId { get; set; } = Guid.NewGuid();
        public bool TriggerEnabled { get; set; }
        public string TriggerName { get; set; }

        public Guid? FolderId { get; set; }

        // Set when this USER trigger lives directly under a built-in library folder (e.g.
        // "Encounters/Kael"). Built-in folders are synthesized from code and have no stable ids,
        // so user triggers anchor to them by "/"-separated path instead of FolderId. Null when the
        // trigger has a user folder (FolderId) or sits at the root. Unrelated to BuiltInFolder,
        // which is the non-persisted folder a BUILT-IN trigger's code definition declares.
        public string BuiltInFolderPath { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public bool IsBuiltIn { get; set; }

        public string BuiltInId { get; set; }

        // True once the user has edited a built-in trigger's definition. Keeps SyncBuiltInTriggers
        // from refreshing (overwriting) their edits with the code definition on load. Only meaningful
        // for built-ins.
        public bool Customized { get; set; }

        // Optional "/"-separated folder path under the Built In category used to organize
        // built-in library entries in the tree (e.g. "Encounters/Kael"). Display-only and
        // never persisted - a copied-out user trigger keeps no folder path.
        [Newtonsoft.Json.JsonIgnore]
        public string BuiltInFolder { get; set; }

        public string Category { get; set; } = "Default";
        public string Comments { get; set; } = string.Empty;

        public string Zone { get; set; }
        public bool MatchesZone(string currentZone)
        {
            return string.IsNullOrEmpty(Zone) ||
                string.Equals(Zone, currentZone, StringComparison.OrdinalIgnoreCase);
        }

        // Optional server restriction: when non-empty, the trigger only fires for characters on
        // one of these servers (e.g. the Green-only FTE 96% rule). Null/empty = every server.
        // Not exposed in the trigger editor; only built-in definitions set it.
        public List<EQToolShared.Enums.Servers> Servers { get; set; }
        public bool MatchesServer(EQToolShared.Enums.Servers? currentServer)
        {
            return Servers == null || Servers.Count == 0 ||
                (currentServer.HasValue && Servers.Contains(currentServer.Value));
        }

        public bool? UseRegex { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public bool EffectiveUseRegex => UseRegex ?? true;

        public TriggerOutput Basic { get; set; }

        // Timer / counter configuration. Null == not configured.
        public TriggerTimer Timer { get; set; }
        public TriggerTimerEnding TimerEnding { get; set; }
        public TriggerTimerEnded TimerEnded { get; set; }
        public TriggerCounter Counter { get; set; }

        // Returns the Basic output, constructing one from the legacy fields when a
        // trigger predates the expanded editor (so old triggers keep working at runtime).
        public TriggerOutput GetEffectiveBasic()
        {
            if (Basic != null)
            {
                return Basic;
            }
            return new TriggerOutput
            {
                DisplayTextEnabled = DisplayTextEnabled,
                DisplayText = DisplayText ?? string.Empty,
                AudioType = AudioTextEnabled ? TriggerAudioType.TextToSpeech : TriggerAudioType.None,
                TtsText = AudioText ?? string.Empty
            };
        }

        private string _SearchText = string.Empty;

        // Whether the search pattern depends on the logged-in player's name. Callers use this to
        // avoid evaluating a {c} pattern before the name is known, which would otherwise expand
        // the macro to an empty string (see TriggerRegex).
        [Newtonsoft.Json.JsonIgnore]
        public bool UsesPlayerNameToken = false;

        public string SearchText
        {
            get => _SearchText;
            set
            {
                if (_SearchText != value)
                {
                    _SearchText = value;
                    _TriggerRegex = null;
                    _compileFailed = false;
                    _matchTimedOut = false;
                    UsesPlayerNameToken = !string.IsNullOrEmpty(value) && _SearchText.IndexOf("{c}", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
        }

        public bool DisplayTextEnabled { get; set; }
        public string DisplayText { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ExpandedDisplayText => ExpandOutputText(DisplayText);

        public bool AudioTextEnabled { get; set; }
        public string AudioText { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ExpandedAudioText => ExpandOutputText(AudioText);

        private Regex _TriggerRegex;

        // set when the pattern failed to compile, so the per-line hot path doesn't retry
        // (and throw) on every line; cleared when SearchText or PlayerName changes
        private bool _compileFailed;

        // set when a match exceeded RegexTimeouts.Trigger. A pattern that backtracks that badly
        // will do it again on the next line, and paying the full timeout per line would stall the
        // UI thread just as surely as the original hang, so the pattern is parked for the session.
        // Cleared when SearchText or PlayerName changes (i.e. when the user edits it).
        private bool _matchTimedOut;


        [Newtonsoft.Json.JsonIgnore]
        public Regex TriggerRegex
        {
            get
            {
                if (_TriggerRegex == null && !_compileFailed && !string.IsNullOrWhiteSpace(_SearchText))
                {
                    try
                    {
                        var escapedPlayerName = Regex.Escape(PlayerName ?? string.Empty);
                        var convertedSearchText = _SearchText.Replace("{c}", escapedPlayerName).Replace("{C}", escapedPlayerName);

                        // Convert every simplified {name} placeholder into a real named group in a
                        // single pass. Each placeholder is rewritten at its own position, so a token
                        // that is left alone (see below) can't shift the ones after it.
                        convertedSearchText = placeholderRegex.Replace(convertedSearchText, m =>
                        {
                            var group_name = m.Groups["xxx"].Value;

                            // {2}, {10} etc. are regex quantifiers, not placeholders - "\d{3}" means
                            // "three digits". A group name can't start with a digit either, so
                            // rewriting these into "(?<3>...)" both silently changed what the pattern
                            // matched and, for "{0}", threw ("capture number cannot be zero") and left
                            // the trigger permanently dead. Leave the quantifier as the user wrote it.
                            if (IsAllDigits(group_name))
                            {
                                return m.Value;
                            }

                            return $"(?<{group_name}>[\\w` ]+)";
                        });

                        // The match timeout is the process-wide default set in App's static ctor -
                        // user-authored patterns are the ones most likely to backtrack catastrophically.
                        _TriggerRegex = new Regex(convertedSearchText, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    }
                    catch
                    {
                        _compileFailed = true;
                    }
                }

                return _TriggerRegex;
            }
        }

        private static bool IsAllDigits(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsDigit(value[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private readonly Hashtable valueHash = new Hashtable();

        public void SaveNamedGroupValues(Match match)
        {
            foreach (Group g in match.Groups)
            {
                if (valueHash.ContainsKey(g.Name))
                {
                    valueHash[g.Name] = g.Value;
                }
                else
                {
                    valueHash.Add(g.Name, g.Value);
                }
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public long CurrentCounter { get; set; } = 0;

        public string Expand(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : ExpandOutputText(text);
        }

        public bool Matches(string line)
        {
            if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(SearchText))
            {
                return false;
            }

            if (EffectiveUseRegex)
            {
                if (_matchTimedOut)
                {
                    return false;
                }
                var regex = TriggerRegex;
                if (regex == null)
                {
                    return false;
                }
                try
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        SaveNamedGroupValues(match);
                        return true;
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    // a pattern that backtracks past the budget (e.g. "^(\w+ )+$" against a long
                    // non-matching line) would otherwise hang the UI thread outright
                    _matchTimedOut = true;
                }
                return false;
            }

            return line.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }


        private string ExpandOutputText(string unExpandedText)
        {
            var rv = unExpandedText.Replace("{c}", PlayerName ?? string.Empty).Replace("{C}", PlayerName ?? string.Empty);

            // replace the {COUNTER} macro before the generic placeholder loop, since {COUNTER}
            // also matches the \w+ placeholder pattern but is a macro, not a captured group
            if (counterTokenRegex.IsMatch(rv))
            {
                rv = counterTokenRegex.Replace(rv, CurrentCounter.ToString());
            }

            // Replace every placeholder with its captured value in a single pass. Each one is
            // rewritten at its own position, which matters when a name isn't in the hash (the user
            // made a typo, or the group didn't participate in this match): the old loop iterated
            // matches of the original text but always replaced the *first* placeholder remaining in
            // the rewritten text, so skipping one shifted every later value one placeholder left -
            // "{typo} {damage}" produced "1000 {damage}".
            // A MatchEvaluator also means the captured value is inserted literally, so a value
            // containing '$' isn't read as a substitution pattern ($1, $&, ...).
            rv = placeholderRegex.Replace(rv, m =>
            {
                var group_name = m.Groups["xxx"].Value;

                // this key should be present, but confirm in case user made a typo
                return valueHash.ContainsKey(group_name)
                    ? $"{valueHash[group_name]}"
                    : m.Value;
            });
            return rv;
        }
    }
}