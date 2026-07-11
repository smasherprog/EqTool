using EQTool.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace EQTool.Services
{
    // Produces a sample log line that will cause a trigger to match, so the trigger editor's
    // "Test" box can be pre-filled with something that actually fires the trigger.
    //
    // For plain-text triggers the search text is returned verbatim (matching is a case-insensitive
    // substring check, so the text always matches itself). For regex triggers a matching example is
    // synthesized by walking the pattern: alternations pick their first branch, groups/quantifiers
    // are expanded minimally, character classes yield one representative character, and anchors /
    // look-arounds contribute nothing. The result is verified with Trigger.Matches before use.
    public static class TriggerTestSampleGenerator
    {
        // Same simplified-placeholder pattern Trigger uses to turn "{name}" into a named group.
        private static readonly Regex placeholderRegex = new Regex(@"\{(?<xxx>\w+)\}", RegexOptions.Compiled);

        // A word that safely matches the [\w` ]+ expansion of a simplified placeholder.
        private const string SampleWord = "Soandso";

        // Candidate characters tried (in order) when picking a representative for a character class.
        private const string ClassCandidates = "aA01 x!.-_#'";

        public static string Generate(Trigger trigger, string playerName)
        {
            if (trigger == null || string.IsNullOrWhiteSpace(trigger.SearchText))
            {
                return string.Empty;
            }

            // Plain-text triggers match on substring, so the search text matches itself.
            if (!trigger.EffectiveUseRegex)
            {
                return trigger.SearchText;
            }

            // Mirror Trigger.TriggerRegex's conversion: {c}/{C} -> the player name, {name} -> a word.
            var name = string.IsNullOrWhiteSpace(playerName) ? SampleWord : playerName;
            var pattern = trigger.SearchText.Replace("{c}", name).Replace("{C}", name);
            pattern = placeholderRegex.Replace(pattern, SampleWord);

            try
            {
                var pos = 0;
                // Best effort: the pattern-walk covers the constructs used by every built-in and
                // typical user trigger. If an exotic pattern yields a non-matching line, the user
                // still sees a close starting point they can adjust (Test reports match/no-match).
                return ParseSequence(pattern, ref pos);
            }
            catch
            {
                return string.Empty;
            }
        }

        // Parses one alternation branch (a sequence of terms) until it hits '|', ')', or the end.
        private static string ParseSequence(string s, ref int pos)
        {
            var sb = new StringBuilder();
            while (pos < s.Length && s[pos] != '|' && s[pos] != ')')
            {
                sb.Append(ParseTerm(s, ref pos));
            }
            return sb.ToString();
        }

        // Parses a single atom plus an optional quantifier.
        private static string ParseTerm(string s, ref int pos)
        {
            var atom = ParseAtom(s, ref pos);
            if (pos < s.Length)
            {
                var q = s[pos];
                if (q == '?' || q == '*')
                {
                    // zero-or-optional: omit the atom
                    pos++;
                    ConsumeLazyOrPossessive(s, ref pos);
                    return string.Empty;
                }
                if (q == '+')
                {
                    // one-or-more: a single occurrence is enough
                    pos++;
                    ConsumeLazyOrPossessive(s, ref pos);
                    return atom;
                }
                if (q == '{')
                {
                    return ExpandBraceQuantifier(s, ref pos, atom);
                }
            }
            return atom;
        }

        // Skips a trailing lazy ('?') or possessive ('+') modifier after a quantifier.
        private static void ConsumeLazyOrPossessive(string s, ref int pos)
        {
            if (pos < s.Length && (s[pos] == '?' || s[pos] == '+'))
            {
                pos++;
            }
        }

        // Handles {n} / {n,} / {n,m} numeric quantifiers by repeating the atom the minimum times.
        private static string ExpandBraceQuantifier(string s, ref int pos, string atom)
        {
            var start = pos;
            var close = s.IndexOf('}', pos);
            if (close < 0)
            {
                // not a real quantifier; treat '{' as a literal
                pos++;
                return "{";
            }
            var inner = s.Substring(pos + 1, close - pos - 1);
            var comma = inner.IndexOf(',');
            var minText = comma < 0 ? inner : inner.Substring(0, comma);
            if (int.TryParse(minText.Trim(), out var min))
            {
                pos = close + 1;
                ConsumeLazyOrPossessive(s, ref pos);
                var sb = new StringBuilder();
                for (var i = 0; i < min; i++)
                {
                    sb.Append(atom);
                }
                return sb.ToString();
            }
            // not numeric; treat '{' as a literal
            pos = start + 1;
            return "{";
        }

        // Parses one atom: anchor, group, character class, escape, dot, or literal.
        private static string ParseAtom(string s, ref int pos)
        {
            var c = s[pos];
            if (c == '^' || c == '$')
            {
                pos++;
                return string.Empty;
            }
            if (c == '(')
            {
                return ParseGroup(s, ref pos);
            }
            if (c == '[')
            {
                return ParseCharClass(s, ref pos);
            }
            if (c == '\\')
            {
                pos++;
                if (pos >= s.Length)
                {
                    return "\\";
                }
                var esc = s[pos];
                pos++;
                return SampleFromEscape(esc);
            }
            if (c == '.')
            {
                pos++;
                return "x";
            }
            pos++;
            return c.ToString();
        }

        // Parses a "(...)" group, choosing the first alternative. Look-arounds contribute nothing.
        private static string ParseGroup(string s, ref int pos)
        {
            pos++; // consume '('
            var isLookaround = false;
            if (Peek(s, pos, "?:"))
            {
                pos += 2;
            }
            else if (Peek(s, pos, "?<=") || Peek(s, pos, "?<!"))
            {
                pos += 3;
                isLookaround = true;
            }
            else if (Peek(s, pos, "?=") || Peek(s, pos, "?!"))
            {
                pos += 2;
                isLookaround = true;
            }
            else if (Peek(s, pos, "?<") || Peek(s, pos, "?'"))
            {
                // named group: skip past the "?<name>" (or "?'name'") header
                pos += 2;
                var closer = s[pos - 1] == '<' ? '>' : '\'';
                while (pos < s.Length && s[pos] != closer)
                {
                    pos++;
                }
                if (pos < s.Length)
                {
                    pos++; // consume closer
                }
            }

            var first = ParseSequence(s, ref pos);
            // discard any remaining alternatives in this group
            while (pos < s.Length && s[pos] == '|')
            {
                pos++;
                ParseSequence(s, ref pos);
            }
            if (pos < s.Length && s[pos] == ')')
            {
                pos++; // consume ')'
            }
            return isLookaround ? string.Empty : first;
        }

        // Parses a "[...]" character class and returns one representative character.
        private static string ParseCharClass(string s, ref int pos)
        {
            pos++; // consume '['
            var negated = false;
            if (pos < s.Length && s[pos] == '^')
            {
                negated = true;
                pos++;
            }
            var body = new StringBuilder();
            while (pos < s.Length && s[pos] != ']')
            {
                if (s[pos] == '\\' && pos + 1 < s.Length)
                {
                    body.Append(s[pos]).Append(s[pos + 1]);
                    pos += 2;
                }
                else
                {
                    body.Append(s[pos]);
                    pos++;
                }
            }
            if (pos < s.Length)
            {
                pos++; // consume ']'
            }
            return SampleFromCharClass(body.ToString(), negated);
        }

        // Picks a character that is inside (or, when negated, outside) the given class body.
        private static string SampleFromCharClass(string body, bool negated)
        {
            Regex cls;
            try
            {
                cls = new Regex("[" + body + "]");
            }
            catch
            {
                return "x";
            }
            foreach (var ch in ClassCandidates)
            {
                var inClass = cls.IsMatch(ch.ToString());
                if (inClass != negated)
                {
                    return ch.ToString();
                }
            }
            return "x";
        }

        private static string SampleFromEscape(char esc)
        {
            switch (esc)
            {
                case 'w': return "a";
                case 'd': return "1";
                case 's': return " ";
                case 'W': return "!";
                case 'D': return "a";
                case 'S': return "a";
                case 't': return "\t";
                // zero-width assertions contribute nothing
                case 'b':
                case 'B':
                case 'A':
                case 'Z':
                case 'z':
                case 'G':
                    return string.Empty;
                // anything else (\., \!, \(, etc.) is the escaped literal character
                default: return esc.ToString();
            }
        }

        private static bool Peek(string s, int pos, string token)
        {
            return pos + token.Length <= s.Length && s.Substring(pos, token.Length) == token;
        }
    }
}
