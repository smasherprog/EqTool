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
            {
                rv = new PlayerCommsEvent();
                rv.Receiver = match.Groups["receiver"].Value;
                rv.Content = match.Groups["content"].Value;
                rv.TheChannel = PlayerCommsEvent.Channel.TELL;
                rv.TimeStamp = timestamp;
                rv.Line = line;
            }

            //You say, 'Hail, Wenglawks Kkeak'
            pattern = @"^You say, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                rv = new PlayerCommsEvent();
                rv.Receiver = "";
                rv.Content = match.Groups["content"].Value;
                rv.TheChannel = PlayerCommsEvent.Channel.SAY;
                rv.TimeStamp = timestamp;
                rv.Line = line;
            }

            //You tell your party, 'oh interesting'
            pattern = @"^You tell your party, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                rv = new PlayerCommsEvent();
                rv.Receiver = "";
                rv.Content = match.Groups["content"].Value;
                rv.TheChannel = PlayerCommsEvent.Channel.GROUP;
                rv.TimeStamp = timestamp;
                rv.Line = line;
            }

            //You say to your guild, 'nice'
            pattern = @"^You say to your guild, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                rv = new PlayerCommsEvent();
                rv.Receiver = "";
                rv.Content = match.Groups["content"].Value;
                rv.TheChannel = PlayerCommsEvent.Channel.GUILD;
                rv.TimeStamp = timestamp;
                rv.Line = line;
            }

            //You auction, 'wtb diamond'
            pattern = @"^You auction, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                rv = new PlayerCommsEvent();
                rv.Receiver = "";
                rv.Content = match.Groups["content"].Value;
                rv.TheChannel = PlayerCommsEvent.Channel.AUCTION;
                rv.TimeStamp = timestamp;
                rv.Line = line;
            }

            //You say out of character, 'train to west'
            pattern = @"^You say out of character, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                rv = new PlayerCommsEvent();
                rv.Receiver = "";
                rv.Content = match.Groups["content"].Value;
                rv.TheChannel = PlayerCommsEvent.Channel.OOC;
                rv.TimeStamp = timestamp;
                rv.Line = line;
            }

            //You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'
            pattern = @"^You shout, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                rv = new PlayerCommsEvent();
                rv.Receiver = "";
                rv.Content = match.Groups["content"].Value;
                rv.TheChannel = PlayerCommsEvent.Channel.SHOUT;
                rv.TimeStamp = timestamp;
                rv.Line = line;
            }

            //Azleep -> Jamori: ok
            pattern = @"^.+ -> (?<receiver>.+): (?<content>.+)";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                rv = new PlayerCommsEvent();
                rv.Receiver = match.Groups["receiver"].Value;
                rv.Content = match.Groups["content"].Value;
                rv.TheChannel = PlayerCommsEvent.Channel.TELL;
                rv.TimeStamp = timestamp;
                rv.Line = line;
            }

            return rv;
        }
    }
}
