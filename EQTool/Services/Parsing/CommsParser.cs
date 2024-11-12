using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Text.RegularExpressions;


namespace EQTool.Services.Parsing
{
    //
    // this parser will watch for Player comms
    //
    public class CommsParser : IEqLogParser
    {
        // class data
        private readonly ActivePlayer activePlayer;
        private readonly LogEvents logEvents;

        private const string patternParty = @"^(?<sender>.+) ((tell your party)|(tells the group)), '(?<content>.+)'";
        private const string patternTells = @"^(?<sender>.+) ((say to your)|(tells the)) guild, '(?<content>.+)'";
        private const string patternInternalTell = @"^(?<sender>.+) -> (?<receiver>.+): (?<content>.+)";
        private const string patternIsNotOnline = @"^(?<content>.+) is not online at this time.";
        private const string patternSays = @"^(?<sender>.+) (say|says), '(?<content>.+)'";
        private const string patternTold = @"^(?<sender>.+) (told|tells) (?<receiver>.+), '(?<content>.+)'";
        private const string patternAuctions = @"^(?<sender>.+) auction(s)?, '(?<content>.+)'";
        private const string patternOutOfCharacter = @"^(?<sender>.+) say(s)? out of character, '(?<content>.+)'";
        private const string patternShouts = @"^(?<sender>.+) shout(s)?, '(?<content>.+)'";


        private readonly Regex regexParty = new Regex(patternParty, RegexOptions.Compiled);
        private readonly Regex regexTells = new Regex(patternTells, RegexOptions.Compiled);
        private readonly Regex regexInternalTell = new Regex(patternInternalTell, RegexOptions.Compiled);
        private readonly Regex regexIsNotOnline = new Regex(patternIsNotOnline, RegexOptions.Compiled);
        private readonly Regex regexSays = new Regex(patternSays, RegexOptions.Compiled);
        private readonly Regex regexTold = new Regex(patternTold, RegexOptions.Compiled);
        private readonly Regex regexAuctions = new Regex(patternAuctions, RegexOptions.Compiled);
        private readonly Regex regexOutOfCharacter = new Regex(patternOutOfCharacter, RegexOptions.Compiled);
        private readonly Regex regexShouts = new Regex(patternShouts, RegexOptions.Compiled);

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
        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var commsEvent = Match(line, timestamp, lineCounter);
            if (commsEvent != null)
            {
                commsEvent.Line = line;
                commsEvent.TimeStamp = timestamp;
                commsEvent.LineCounter = lineCounter;
                logEvents.Handle(commsEvent);
                return true;
            }
            return false;
        }

        // parse this line to see if it contains what we are looking for
        // returns a CommsEvent object if a comms event is detecte, else
        // returns null
        public CommsEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            //
            // begin checking for the various channels
            //

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // group

            //You tell your party, 'oh interesting'
            //Jaloy tells the group, 'wiki says he can be in 1 of 2 locations'

            var match = regexParty.Match(line);
            if (match.Success)
            {
                return new CommsEvent
                {
                    Content = match.Groups["content"].Value,
                    Line = line,
                    LineCounter = lineCounter,
                    Receiver = string.Empty,
                    Sender = match.Groups["sender"].Value,
                    TheChannel = CommsEvent.Channel.GROUP,
                    TimeStamp = timestamp
                };
            }

            //
            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // guild

            //You say to your guild, 'nice'
            //[Wed Oct 16 17:17:25 2024] Okeanos tells the guild, 'it literally says speedway but the  products inside the store are 7/11 branded '

            match = regexTells.Match(line);
            if (match.Success)
            {
                return new CommsEvent
                {
                    Content = match.Groups["content"].Value,
                    Line = line,
                    LineCounter = lineCounter,
                    Receiver = string.Empty,
                    Sender = match.Groups["sender"].Value,
                    TheChannel = CommsEvent.Channel.GUILD,
                    TimeStamp = timestamp
                };
            }

            //Azleep -> Jamori: ok
            //[Thu Aug 18 14:31:48 2022] Berrma -> Azleep: ya just need someone to invite i believe

            match = regexInternalTell.Match(line);
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
                return new CommsEvent
                {
                    Content = match.Groups["content"].Value,
                    Line = line,
                    LineCounter = lineCounter,
                    Receiver = receiver,
                    Sender = sender,
                    TheChannel = CommsEvent.Channel.TELL,
                    TimeStamp = timestamp
                };
            }

            // this is actually a tell, even though it doesn't look like it.  It doesn't show up for anyone but the recceiver
            //[Mon Oct 21 11:50:55 2024] .PigTimer-30 is not online at this time.

            match = regexIsNotOnline.Match(line);
            if (match.Success)
            {
                return new CommsEvent
                {
                    Content = match.Groups["content"].Value,
                    Line = line,
                    LineCounter = lineCounter,
                    Receiver = "You",
                    Sender = "System",
                    TheChannel = CommsEvent.Channel.TELL,
                    TimeStamp = timestamp
                };
            }


            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // say

            //You say, 'Hail, Wenglawks Kkeak'
            //[Wed Nov 20 20:29:06 2019] Jaloy says, 'i am a new warrior'
            //[Wed Nov 20 20:28:44 2019] Manik Compolten says, 'Greetings, young one. I am Manik Compolten, High Watchman. Are you a [new warrior] or an [experienced fighter]?'

            match = regexSays.Match(line);
            if (match.Success)
            {
                return new CommsEvent
                {
                    Content = match.Groups["content"].Value,
                    Line = line,
                    LineCounter = lineCounter,
                    Receiver = string.Empty,
                    Sender = match.Groups["sender"].Value,
                    TheChannel = CommsEvent.Channel.SAY,
                    TimeStamp = timestamp
                };
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // tells

            //You told Qdyil, 'not even sure'
            //[Sat Mar 21 17:45:14 2020] Thalistair tells you, 'omw'
            //[Sat Mar 21 17:55:33 2020] a spectre tells you, 'Attacking a spectre Master.'
            //[Sat Mar 21 19:21:37 2020] Cleonae Kalen tells you, 'I'll give you 9 gold 8 silver 8 copper per Globe of Fear'

            match = regexTold.Match(line);
            if (match.Success)
            {
                return new CommsEvent
                {
                    Content = match.Groups["content"].Value,
                    Line = line,
                    LineCounter = lineCounter,
                    Receiver = match.Groups["receiver"].Value,
                    Sender = match.Groups["sender"].Value,
                    TheChannel = CommsEvent.Channel.TELL,
                    TimeStamp = timestamp
                };
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // auction

            //You auction, 'wtb diamond'
            //[Mon Feb 22 14:40:47 2021] Mezzter auctions, 'WTS bone chips 7p per stack pst'

            match = regexAuctions.Match(line);
            if (match.Success)
            {
                return new CommsEvent
                {
                    Content = match.Groups["content"].Value,
                    Line = line,
                    LineCounter = lineCounter,
                    Receiver = string.Empty,
                    Sender = match.Groups["sender"].Value,
                    TheChannel = CommsEvent.Channel.AUCTION,
                    TimeStamp = timestamp
                };
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // ooc

            //You say out of character, 'train to west'
            //[Wed Nov 20 20:18:47 2019] Enudara says out of character, 'grats'

            match = regexOutOfCharacter.Match(line);
            if (match.Success)
            {
                return new CommsEvent
                {
                    Content = match.Groups["content"].Value,
                    Line = line,
                    LineCounter = lineCounter,
                    Receiver = string.Empty,
                    Sender = match.Groups["sender"].Value,
                    TheChannel = CommsEvent.Channel.OOC,
                    TimeStamp = timestamp
                };
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------
            // shout

            //You shout, 'When it is time - Horse Charmers will be Leffingwell and Ceous'
            //[Sat Aug 22 18:54:17 2020] Fizzix shouts, 'ASSIST Fizzix on --- [ an essence tamer ]'

            match = regexShouts.Match(line);
            return match.Success
                ? new CommsEvent
                {
                    Content = match.Groups["content"].Value,
                    Line = line,
                    LineCounter = lineCounter,
                    Receiver = string.Empty,
                    Sender = match.Groups["sender"].Value,
                    TheChannel = CommsEvent.Channel.SHOUT,
                    TimeStamp = timestamp
                } : null;
        }
    }
}
