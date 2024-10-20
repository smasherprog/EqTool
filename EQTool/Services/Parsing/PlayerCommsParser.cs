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

            // examples
            //You told Qdyil, 'not even sure'
            //You say, 'Hail, Wenglawks Kkeak'
            //You tell your party, 'oh interesting'
            //You say to your guild, 'nice'
            //You auction, 'wtb diamond'
            //You say out of character, 'train to west'
            //You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'
            //Azleep -> Jamori: ok

            // does this line contain a communication?
            PlayerCommsEvent.Channel channel = PlayerCommsEvent.Channel.NONE;
            if (line.StartsWith("You told"))
                channel = PlayerCommsEvent.Channel.TELL;
            else if (line.StartsWith("You say, "))
                channel = PlayerCommsEvent.Channel.SAY;
            else if (line.StartsWith("You tell your party, "))
                channel = PlayerCommsEvent.Channel.GROUP;
            else if (line.StartsWith("You say to your guild, "))
                channel = PlayerCommsEvent.Channel.GUILD;
            else if (line.StartsWith("You auction, "))
                channel = PlayerCommsEvent.Channel.AUCTION;
            else if (line.StartsWith("You say out of character, "))
                channel = PlayerCommsEvent.Channel.OOC;
            else if (line.StartsWith("You shout, "))
                channel = PlayerCommsEvent.Channel.SHOUT;
            else if (line.StartsWith($"{playerName} ->"))
                channel = PlayerCommsEvent.Channel.TELL;

            // did we find a comms event in any channel?
            if (channel != PlayerCommsEvent.Channel.NONE)
            {
                rv = new PlayerCommsEvent();
                rv.TimeStamp = timestamp;
                rv.Line = line;
                rv.theChannel = channel;
            }

            return rv;
        }
    }
}
