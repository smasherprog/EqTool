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
            var shout = line.IndexOf("shouts, '", System.StringComparison.OrdinalIgnoreCase);
            var outofchar = line.IndexOf(" says out of character, '", System.StringComparison.OrdinalIgnoreCase);
            var guild = line.IndexOf(" tells the guild, '", System.StringComparison.OrdinalIgnoreCase);
            if (chindex != -1 && dashes != -1)
            {
                var numbers = new string(line.Where(a => char.IsDigit(a)).ToArray());
                if (numbers.Length == 0)
                {
                    return null;
                }

                var position = int.Parse(numbers);
                var recipientguild = line.Substring(line.IndexOf('\'') + 1, 2);
                var chtag = this.activePlayer?.Player?.ChChainTagOverlay;
                if (!string.IsNullOrWhiteSpace(chtag) && !string.Equals(chtag, recipientguild, System.StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (shout != -1)
                {
                    return new ChParseData
                    {
                        Caster = line.Substring(0, shout).Trim('\'').Trim(),
                        Position = position,
                        Recipient = line.Substring(dashes + 1).Trim('\'').Trim(),
                        RecipientGuild = recipientguild
                    };
                }

                if (guild != -1)
                {
                    return new ChParseData
                    {
                        Caster = line.Substring(0, guild).Trim('\'').Trim(),
                        Position = position,
                        Recipient = line.Substring(dashes + 1).Trim('\'').Trim(),
                        RecipientGuild = recipientguild
                    };
                }

                if (outofchar != -1)
                {
                    return new ChParseData
                    {
                        Caster = line.Substring(0, outofchar).Trim('\'').Trim(),
                        Position = position,
                        Recipient = line.Substring(dashes + 1).Trim('\'').Trim(),
                        RecipientGuild = recipientguild
                    };
                }
            }

            return null;
        }
    }
}
