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
            var startindexofmessage = line.IndexOf(", '");
            if (startindexofmessage == -1)
            {
                startindexofmessage = line.IndexOf(",  '");
            }

            if (chindex != -1)
            {
                var endoftext = line.LastIndexOf("'");
                if (endoftext == -1)
                {
                    return null;
                }

                var possiblenumbers = line.Substring(startindexofmessage + 3, endoftext - startindexofmessage - 3);
                var possiblenumbersplits = possiblenumbers.Split(' ');
                var numbers = string.Empty;
                foreach (var item in possiblenumbersplits)
                {
                    numbers = new string(item.Where(a => char.IsDigit(a)).ToArray());
                    if (numbers.Length != 0)
                    {
                        break;
                    }
                }
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
                    if (possibletagindex == -1)
                    {
                        return null;
                    }
                    tag = possiblenumbers.Substring(0, possibletagindex).Trim();
                }
                var firstspace = line.IndexOf(" ");
                var recipient = new string(line.Substring(chindex + 3).Where(a => char.IsLetter(a)).ToArray());
                if (string.IsNullOrWhiteSpace(recipient) || recipient.Length < 3)
                {
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(tag) && tag.Contains(" "))
                {
                    return null;
                }

                var ret = new ChParseData
                {
                    Caster = line.Substring(0, firstspace).Trim('\'').Trim(),
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
