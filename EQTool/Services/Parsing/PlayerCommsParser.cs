using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Controls;


namespace EQTool.Services.Parsing
{
    //
    // this parser will watch for Player comms
    //
    public class PlayerCommsParser : IEqLogParseHandler
    {
        // class data
        private readonly ActivePlayer activePlayer;
        private readonly LogEvents logEvents;

        //
        // ctor
        //
        public PlayerCommsParser(ActivePlayer activePlayer, LogEvents logEvents)
        {
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
        }

        // handle a line from the log file.
        // If we find what we are seeking, fire off our event
        public bool Handle(string line, DateTime timestamp)
        {
            PlayerCommsEvent e = Match(line, timestamp);
            if (e != null)
            {
                logEvents.Handle(e);
                return true;
            }
            return false;
        }

        // parse this line to see if it contains what we are looking for
        // returns a PlayerCommsEvent object if a comms event is detecte, else
        // returns null
        public PlayerCommsEvent Match(string line, DateTime timestamp)
        {
            PlayerCommsEvent rv = null;

            // get playername
            string playerName = "Unknown";
            if (activePlayer != null)
                playerName = activePlayer.Player.Name;

            //
            // begin checking for the various channels
            //

            //You told Qdyil, 'not even sure'
            var pattern = @"^You told (?<receiver>.+), '(?<content>.+)'";
            var regex = new Regex(pattern, RegexOptions.Compiled);
            var match = regex.Match(line);
            if (match.Success)
                rv = new PlayerCommsEvent(timestamp, line, PlayerCommsEvent.Channel.TELL, match.Groups["content"].Value, "You", match.Groups["receiver"].Value);

            //You say, 'Hail, Wenglawks Kkeak'
            pattern = @"^You say, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
                rv = new PlayerCommsEvent(timestamp, line, PlayerCommsEvent.Channel.SAY, match.Groups["content"].Value, "You");

            //You tell your party, 'oh interesting'
            pattern = @"^You tell your party, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
                rv = new PlayerCommsEvent(timestamp, line, PlayerCommsEvent.Channel.GROUP, match.Groups["content"].Value, "You");

            //You say to your guild, 'nice'
            pattern = @"^You say to your guild, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
                rv = new PlayerCommsEvent(timestamp, line, PlayerCommsEvent.Channel.GUILD, match.Groups["content"].Value, "You");

            //You auction, 'wtb diamond'
            pattern = @"^You auction, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
                rv = new PlayerCommsEvent(timestamp, line, PlayerCommsEvent.Channel.AUCTION, match.Groups["content"].Value, "You");

            //You say out of character, 'train to west'
            pattern = @"^You say out of character, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
                rv = new PlayerCommsEvent(timestamp, line, PlayerCommsEvent.Channel.OOC, match.Groups["content"].Value, "You");

            //You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'
            pattern = @"^You shout, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
                rv = new PlayerCommsEvent(timestamp, line, PlayerCommsEvent.Channel.SHOUT, match.Groups["content"].Value, "You");

            //Azleep -> Jamori: ok
            pattern = @"^.+ -> (?<receiver>.+): (?<content>.+)";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
                rv = new PlayerCommsEvent(timestamp, line, PlayerCommsEvent.Channel.TELL, match.Groups["content"].Value, "You", match.Groups["receiver"].Value);

            return rv;
        }
    }
}
