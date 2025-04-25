using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class TriggerHandler : BaseHandler
    {
        public TriggerHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.LineEvent += LogEvents_LineEvent;
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            foreach (var trigger in eQToolSettings.Triggers.Where(a => a.TriggerEnabled))
            {
                // check for a match
                var matched = Match(trigger, e.Line);
                if (matched)
                {
                    // text to speech?
                    if (trigger.AudioTextEnabled)
                    {
                        textToSpeach.Say(trigger.AudioText);
                    }

                    // displayed text?
                    if (trigger.DisplayTextEnabled)
                    {
                        _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            logEvents.Handle(new OverlayEvent { Text = trigger.DisplayText, ForeGround = Brushes.Red, Reset = false });
                            System.Threading.Thread.Sleep(5000);
                            logEvents.Handle(new OverlayEvent { Text = trigger.DisplayText, ForeGround = Brushes.Red, Reset = true });
                        });
                    }
                }
            }
        }

        private const string ginaRegexPattern = @"\{(?<xxx>\w+)\}";
        private static Regex ginaRegex = new Regex(ginaRegexPattern, RegexOptions.Compiled);
        public static bool Match(Models.Trigger trigger, string line)
        {
            if (!trigger.TriggerEnabled)
            {
                return false;
            }

            Regex regex;
            try
            {
                regex = trigger.TriggerRegex;
            }
            catch (Exception ex)
            {
                // this is a problem with the regex, so disable the trigger
                trigger.TriggerEnabled = false;
                return false;
            }

            var namevaluePairs = new List<(string Name, string Value)>();
            var match = regex.Match(line);
            if (match.Success)
            {
                foreach (Group g in match.Groups)
                {
                    if (!namevaluePairs.Any(a => string.Equals(a.Name, g.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        namevaluePairs.Add((g.Name, g.Value));
                    }
                }
            }

            if (trigger.DisplayTextEnabled)
            {
                trigger.DisplayText = ProcessOutputText(trigger.DisplayText, namevaluePairs);
            }

            if (trigger.AudioTextEnabled)
            {
                trigger.AudioText = ProcessOutputText(trigger.AudioText, namevaluePairs);
            }

            return match.Success;
        }

        // utility function to merge in the parsed values into the output simplified regex fields
        // example:  Change the output text for a "Sense Heading" trigger from:
        //      "Direction: {direction}"
        // to
        //      "Direction: East"
        private static string ProcessOutputText(string inputText, List<(string Name, string Value)> namevaluePairs)
        {
            string rv = inputText;

            // walk the list of matches, replacing the user match with the real match
            Match match = ginaRegex.Match(rv);
            while (match.Success)
            {
                // Handle match here...
                string group_name = match.Groups["xxx"].Value;

                // this key should be present, but confirm in case user made a typo
                var nameValue = namevaluePairs.FirstOrDefault(a => string.Equals(a.Name, group_name, StringComparison.OrdinalIgnoreCase));
                if (nameValue != default)
                {
                    // use regex to replace the gina named group with value from the hashtable
                    // do them one group at a time
                    string replace_text = $"{nameValue.Value}";
                    rv = ginaRegex.Replace(rv, replace_text, 1);
                }

                match = match.NextMatch();
            }
            return rv;
        }
    }
}
