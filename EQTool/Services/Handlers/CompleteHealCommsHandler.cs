using EQTool.Models;
using EQTool.Services.Handlers;
using EQToolShared;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    public class CompleteHealCommsHandler : BaseHandler
    {
        public CompleteHealCommsHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.CommsEvent += LogEvents_CommsEvent;
        }

        private void LogEvents_CommsEvent(object sender, CommsEvent e)
        {
            if (e.TheChannel == CommsEvent.Channel.TELL || e.TheChannel == CommsEvent.Channel.SAY)
            {
                return;
            }

            var m = ChCheck(e.Sender, e.Content, e.TimeStamp);
            if (m != null)
            {
                logEvents.Handle(m);
            }
        }

        private readonly Regex rchregex = new Regex(@"\bCH\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex cleanSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

        public CompleteHealEvent ChCheck(string sender, string line, DateTime timestamp)
        {
            var chwordfound = " ch ";
            var chindex = line.IndexOf(chwordfound, System.StringComparison.OrdinalIgnoreCase);
            if (chindex == -1)
            {
                chwordfound = "ch ";
                chindex = line.IndexOf("ch ", System.StringComparison.OrdinalIgnoreCase);
                if (chindex != 0)
                {
                    chindex = -1;
                }
            }

            if (line.IndexOf(" rch ", System.StringComparison.OrdinalIgnoreCase) != -1)
            {
                line = rchregex.Replace(line, "");
                line = cleanSpacesRegex.Replace(line, " ").Trim();

                chwordfound = " rch ";
                chindex = line.IndexOf(" rch ", System.StringComparison.OrdinalIgnoreCase);
            }
            if (chindex == -1)
            {
                chwordfound = " rch ";
                chindex = line.IndexOf(" rch ", System.StringComparison.OrdinalIgnoreCase);
            }

            if (chindex != -1)
            {
                var possiblenumbers = line;
                var position = string.Empty;
                var tag = activePlayer?.Player?.ChChainTagOverlay;
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    if (!possiblenumbers.StartsWith(tag))
                    {
                        return null;
                    }
                }
                else
                {
                    var possibletagindex = possiblenumbers.IndexOf(position);
                    if (possibletagindex == -1)
                    {
                        return null;
                    }
                    tag = possiblenumbers.Substring(0, possibletagindex).Trim();
                }
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    possiblenumbers = possiblenumbers.Substring(tag.Length);
                }
                var possiblepositionstosearch = possiblenumbers.Split(' ').Where(a => a.Length == 3).ToList();
                possiblepositionstosearch.Reverse();
                foreach (var item in possiblepositionstosearch)
                {
                    var n = new string(item.Where(a => char.IsDigit(a)).ToArray());
                    if (n.Length == 3)
                    {
                        position = n;
                        break;
                    }

                    if (item.Distinct().Count() == 1 && char.IsLetter(item[0]))
                    {
                        position = item;
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(position))
                {
                    possiblepositionstosearch = possiblenumbers.Split(' ').Where(a => a.StartsWith("ramp", System.StringComparison.OrdinalIgnoreCase)).ToList();
                    possiblepositionstosearch.Reverse();
                    foreach (var item in possiblepositionstosearch)
                    {
                        var rampindex = item.IndexOf("ramp", System.StringComparison.OrdinalIgnoreCase);
                        if (rampindex != -1 && (item.Length == 5 || item.Length == 6))
                        {
                            position = item;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(position))
                {
                    position = "000";
                }

                chindex = possiblenumbers.IndexOf(chwordfound, System.StringComparison.OrdinalIgnoreCase);
                var beforestring = possiblenumbers.Substring(0, chindex).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (beforestring.Length > 2)
                {
                    return null;
                }
                possiblenumbers = possiblenumbers.Substring(chindex + chwordfound.Length);
                var possiblerecipt = new string(possiblenumbers.Replace(position, string.Empty).Where(a => a == ' ' || char.IsLetter(a) || a == '\'' || a == '`').ToArray()).Trim();
                var recipient = string.Empty;
                if (MasterNPCList.NPCs.Contains(possiblerecipt))
                {
                    recipient = possiblerecipt;
                }
                else
                {
                    var splits = possiblerecipt.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length > 2)
                    {
                        return null;
                    }
                    _ = splits.Reverse();
                    foreach (var item in splits)
                    {
                        if (item?.Length >= 3)
                        {
                            recipient = item;
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(recipient) || recipient.Length < 3)
                {
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(tag) && tag.Contains(" "))
                {
                    tag = string.Empty;
                    if (!string.IsNullOrWhiteSpace(activePlayer?.Player?.ChChainTagOverlay))
                    {
                        return null;
                    }
                }

                var ret = new CompleteHealEvent
                {
                    Caster = sender,
                    Position = position,
                    Recipient = recipient,
                    Tag = tag,
                    TimeStamp = timestamp
                };
                if (ret.Caster.Contains(" "))
                {
                    var lastspace = ret.Caster.LastIndexOf(" ");
                    ret.Caster = ret.Caster.Substring(lastspace + 1).Trim();
                }
                return ret;
            }

            return null;
        }
    }
}
