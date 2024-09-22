using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared;
using System;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class CompleteHealParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;

        public CompleteHealParser(ActivePlayer activePlayer, LogEvents logEvents)
        {
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = ChCheck(line);
            if (m != null)
            {
                logEvents.Handle(m);
                return true;
            }
            return false;
        }

        public CompleteHealEvent ChCheck(string line)
        {
            var startindexofmessage = line.IndexOf(", '");
            var startindexsize = 3;
            if (startindexofmessage == -1)
            {
                startindexofmessage = line.IndexOf(",  '");
                startindexsize = 4;
            }

            var chwordfound = " ch ";
            var chindex = line.IndexOf(chwordfound, System.StringComparison.OrdinalIgnoreCase);
            if (chindex == -1)
            {
                chwordfound = "ch ";
                chindex = line.IndexOf("'ch ", System.StringComparison.OrdinalIgnoreCase);
            }

            if (chindex == -1)
            {
                chwordfound = " rch ";
                chindex = line.IndexOf(" rch ", System.StringComparison.OrdinalIgnoreCase);
            }

            if (chindex != -1 && startindexofmessage != -1)
            {
                var endoftext = line.LastIndexOf("'");
                if (endoftext == -1)
                {
                    return null;
                }

                var possiblenumbers = line.Substring(startindexofmessage + startindexsize, endoftext - startindexofmessage - startindexsize);
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

                var caster = line.Substring(0, line.IndexOf(" ")).Trim('\'').Trim();
                var ret = new CompleteHealEvent
                {
                    Caster = caster,
                    Position = position,
                    Recipient = recipient,
                    RecipientGuild = tag
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
