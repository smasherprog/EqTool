using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Text.RegularExpressions;


namespace EQTool.Services.Parsing
{
    //
    // this parser will watch for Player comms
    //
    public class CommsParser : IEqLogParseHandler
    {
        // class data
        private readonly ActivePlayer activePlayer;
        private readonly LogEvents logEvents;

        //
        // ctor
        //
        public CommsParser(ActivePlayer activePlayer, LogEvents logEvents)
        {
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
        }

        // handle a line from the log file.
        // If we find what we are seeking, fire off our event
        public bool Handle(string line, DateTime timestamp)
        {
            var commsEvent = Match(line, timestamp);
            if (commsEvent != null)
            {
                logEvents.Handle(commsEvent);
                return true;
            }
            return false;
        }

        // parse this line to see if it contains what we are looking for
        // returns a CommsEvent object if a comms event is detecte, else
        // returns null
        public CommsEvent Match(string line, DateTime timestamp)
        {
            //
            // begin checking for the various channels
            //

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // tells

            //You told Qdyil, 'not even sure'
            //[Sat Mar 21 17:45:14 2020] Thalistair tells you, 'omw'
            //[Sat Mar 21 17:55:33 2020] a spectre tells you, 'Attacking a spectre Master.'
            //[Sat Mar 21 19:21:37 2020] Cleonae Kalen tells you, 'I'll give you 9 gold 8 silver 8 copper per Globe of Fear'
            var pattern = @"^(?<sender>.+) (told|tells) (?<receiver>.+), '(?<content>.+)'";
            var regex = new Regex(pattern, RegexOptions.Compiled);
            var match = regex.Match(line);
            if (match.Success)
            {
                return new CommsEvent(timestamp, line, CommsEvent.Channel.TELL, match.Groups["content"].Value, match.Groups["sender"].Value, match.Groups["receiver"].Value);
            }

            //Azleep -> Jamori: ok
            //[Thu Aug 18 14:31:48 2022] Berrma -> Azleep: ya just need someone to invite i believe
            pattern = @"^(?<sender>.+) -> (?<receiver>.+): (?<content>.+)";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                var sender = match.Groups["sender"].Value;
                if (sender == activePlayer.Player.Name)
                {
                    sender = "You";
                }

                var receiver = match.Groups["receiver"].Value;
                if (receiver == activePlayer.Player.Name)
                {
                    receiver = "You";
                }

                return new CommsEvent(timestamp, line, CommsEvent.Channel.TELL, match.Groups["content"].Value, sender, receiver);
            }

            // this is actually a tell, even though it doesn't look like it.  It doesn't show up for anyone but the recceiver
            //[Mon Oct 21 11:50:55 2024] .PigTimer-30 is not online at this time.
            pattern = @"^(?<content>.+) is not online at this time.";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                return new CommsEvent(timestamp, line, CommsEvent.Channel.TELL, match.Groups["content"].Value, "System", "You");
            }


            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // say

            //You say, 'Hail, Wenglawks Kkeak'
            //[Wed Nov 20 20:29:06 2019] Jaloy says, 'i am a new warrior'
            //[Wed Nov 20 20:28:44 2019] Manik Compolten says, 'Greetings, young one. I am Manik Compolten, High Watchman. Are you a [new warrior] or an [experienced fighter]?'
            pattern = @"^(?<sender>.+) (say|says), '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                return new CommsEvent(timestamp, line, CommsEvent.Channel.SAY, match.Groups["content"].Value, match.Groups["sender"].Value);
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // group

            //You tell your party, 'oh interesting'
            //Jaloy tells the group, 'wiki says he can be in 1 of 2 locations'
            pattern = @"^(?<sender>.+) ((tell your party)|(tells the group)), '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                return new CommsEvent(timestamp, line, CommsEvent.Channel.GROUP, match.Groups["content"].Value, match.Groups["sender"].Value);
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // guild

            //You say to your guild, 'nice'
            //[Wed Oct 16 17:17:25 2024] Okeanos tells the guild, 'it literally says speedway but the  products inside the store are 7/11 branded '
            pattern = @"^(?<sender>.+) ((say to your)|(tells the)) guild, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                return new CommsEvent(timestamp, line, CommsEvent.Channel.GUILD, match.Groups["content"].Value, match.Groups["sender"].Value);
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // auction

            //You auction, 'wtb diamond'
            //[Mon Feb 22 14:40:47 2021] Mezzter auctions, 'WTS bone chips 7p per stack pst'
            pattern = @"^(?<sender>.+) auction(s)?, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                return new CommsEvent(timestamp, line, CommsEvent.Channel.AUCTION, match.Groups["content"].Value, match.Groups["sender"].Value);
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // ooc

            //You say out of character, 'train to west'
            //[Wed Nov 20 20:18:47 2019] Enudara says out of character, 'grats'
            pattern = @"^(?<sender>.+) say(s)? out of character, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            if (match.Success)
            {
                return new CommsEvent(timestamp, line, CommsEvent.Channel.OOC, match.Groups["content"].Value, match.Groups["sender"].Value);
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // shout

            //You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'
            //[Sat Aug 22 18:54:17 2020] Fizzix shouts, 'ASSIST Fizzix on --- [ an essence tamer ]'
            pattern = @"^(?<sender>.+) shout(s)?, '(?<content>.+)'";
            regex = new Regex(pattern, RegexOptions.Compiled);
            match = regex.Match(line);
            return match.Success
                ? new CommsEvent(timestamp, line, CommsEvent.Channel.SHOUT, match.Groups["content"].Value, match.Groups["sender"].Value)
                : null;
        }
    }
}
