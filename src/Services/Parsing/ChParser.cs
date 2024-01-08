using EQTool.ViewModels;
using System.Linq;

namespace EQTool.Services
{
    public class ChParser
    {
        public class ChParseData
        {
            public string Recipient { get; set; }
            public string RecipientGuild { get; set; }
            public int Position { get; set; }
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
            var dashes = line.LastIndexOf("-", System.StringComparison.OrdinalIgnoreCase);
            var shoutindex = line.IndexOf("shouts, '", System.StringComparison.OrdinalIgnoreCase);
            var outofcharindex = line.IndexOf(" says out of character, '", System.StringComparison.OrdinalIgnoreCase);
            var guildindex = line.IndexOf(" tells the guild, '", System.StringComparison.OrdinalIgnoreCase);
            var raidindex = line.IndexOf(" tells the raid, '", System.StringComparison.OrdinalIgnoreCase);
            if (chindex != -1 && dashes != -1)
            {
                var endoftext = line.LastIndexOf("'");
                var startindex = shoutindex;
                if (startindex == -1)
                {
                    startindex = outofcharindex;
                }
                if (startindex == -1)
                {
                    startindex = guildindex;
                }
                if (startindex == -1)
                {
                    startindex = raidindex;
                }
                var startsearch = line.IndexOf("'", startindex);
                if (startindex == -1)
                {
                    return null;
                }

                var possiblenumbers = line.Substring(startsearch + 1, endoftext - startsearch - 1);
                var numbers = new string(possiblenumbers.Where(a => char.IsDigit(a)).ToArray());
                if (numbers.Length == 0)
                {
                    return null;
                }

                if (!int.TryParse(numbers, out var position))
                {
                    return null;
                }

                var tag = this.activePlayer?.Player?.ChChainTagOverlay;
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    if (!possiblenumbers.StartsWith(tag))
                    {
                        return null;
                    }
                }
                else
                {
                    var possibletagindex = possiblenumbers.IndexOf(numbers);
                    tag = possiblenumbers.Substring(0, possibletagindex).Trim();
                }

                if (shoutindex != -1)
                {
                    var ret = new ChParseData
                    {
                        Caster = line.Substring(0, shoutindex).Trim('\'').Trim(),
                        Position = position,
                        Recipient = line.Substring(dashes + 1, endoftext - (dashes + 1)).Trim('\'').Trim(),
                        RecipientGuild = tag
                    };
                    if (ret.Caster.Contains(" "))
                    {
                        var lastspace = ret.Caster.LastIndexOf(" ");
                        ret.Caster = ret.Caster.Substring(lastspace + 1).Trim();
                    }
                    return ret;
                }

                if (guildindex != -1)
                {
                    var ret = new ChParseData
                    {
                        Caster = line.Substring(0, guildindex).Trim('\'').Trim(),
                        Position = position,
                        Recipient = line.Substring(dashes + 1, endoftext - (dashes + 1)).Trim('\'').Trim(),
                        RecipientGuild = tag
                    };
                    if (ret.Caster.Contains(" "))
                    {
                        var lastspace = ret.Caster.LastIndexOf(" ");
                        ret.Caster = ret.Caster.Substring(lastspace + 1).Trim();
                    }
                    return ret;
                }

                if (outofcharindex != -1)
                {
                    var ret = new ChParseData
                    {
                        Caster = line.Substring(0, outofcharindex).Trim('\'').Trim(),
                        Position = position,
                        Recipient = line.Substring(dashes + 1, endoftext - (dashes + 1)).Trim('\'').Trim(),
                        RecipientGuild = tag
                    };
                    if (ret.Caster.Contains(" "))
                    {
                        var lastspace = ret.Caster.LastIndexOf(" ");
                        ret.Caster = ret.Caster.Substring(lastspace + 1).Trim();
                    }
                    return ret;
                }

                if (raidindex != -1)
                {
                    var ret = new ChParseData
                    {
                        Caster = line.Substring(0, raidindex).Trim('\'').Trim(),
                        Position = position,
                        Recipient = line.Substring(dashes + 1, endoftext - (dashes + 1)).Trim('\'').Trim(),
                        RecipientGuild = tag
                    };
                    if (ret.Caster.Contains(" "))
                    {
                        var lastspace = ret.Caster.LastIndexOf(" ");
                        ret.Caster = ret.Caster.Substring(lastspace + 1).Trim();
                    }
                    return ret;
                }
            }

            return null;
        }
    }
}
