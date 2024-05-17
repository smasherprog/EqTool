using EQTool.ViewModels;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class ChParser
    {
        public class ChParseData
        {
            public string Recipient { get; set; }
            public string RecipientGuild { get; set; }
            public string Position { get; set; }
            public string Caster { get; set; }
        }

        private readonly ActivePlayer activePlayer;

        public ChParser(ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
        }

        public ChParseData ChCheck(string line)
        {
            var chindex = line.IndexOf(" ch ", System.StringComparison.OrdinalIgnoreCase);
            var startindexofmessage = line.IndexOf(", '");
            if (startindexofmessage == -1)
            {
                startindexofmessage = line.IndexOf(",  '");
            }

            if (chindex == -1)
            {
                chindex = line.IndexOf("'ch ", System.StringComparison.OrdinalIgnoreCase);
            }

            if (chindex != -1)
            {
                var endoftext = line.LastIndexOf("'");
                if (endoftext == -1)
                {
                    return null;
                }

                var possiblenumbers = line.Substring(startindexofmessage + 3, endoftext - startindexofmessage - 3);
                var position = string.Empty;
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
                        if (rampindex != -1 && item.Length == 5)
                        {
                            position = item;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(position))
                {
                    return null;
                }

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
                chindex = possiblenumbers.IndexOf(" ch ", System.StringComparison.OrdinalIgnoreCase);
                if (chindex == -1)
                {
                    chindex = possiblenumbers.IndexOf("ch ", System.StringComparison.OrdinalIgnoreCase);
                }
                possiblenumbers = possiblenumbers.Substring(chindex + 3);
                var possiblerecipt = new string(possiblenumbers.Replace(position, string.Empty).Where(a => a == ' ' || char.IsLetter(a)).ToArray());
                var splits = possiblerecipt.Split(' ');
                _ = splits.Reverse();
                var recipient = string.Empty;
                foreach (var item in splits)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        recipient = item;
                        break;
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
                var ret = new ChParseData
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
