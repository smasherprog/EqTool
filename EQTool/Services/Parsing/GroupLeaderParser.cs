using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EQTool.Services.Parsing
{
    // parse for the log lines indicating who the group leader is
    public class GroupLeaderParser : IEqLogParser
    {
        // set up the regular expressions
        private const string patternYouJoin = @"^You notify (?<group_leader>[\w` ]+) that you agree to join the group.";
        private const string patternYouInvite = @"^You invite [\w` ]+ to join your group.";
        private const string patternLeaderChanged = @"(?<group_leader>[\w` ]+) (is|are) now the leader of your group.";

        private readonly Regex regexYouJoin = new Regex(patternYouJoin, RegexOptions.Compiled);
        private readonly Regex regexYouInvite = new Regex(patternYouInvite, RegexOptions.Compiled);
        private readonly Regex regexLeaderChanged = new Regex(patternLeaderChanged, RegexOptions.Compiled);


        private readonly LogEvents logEvents;

        // ctor
        public GroupLeaderParser(LogEvents logEvents)
        {
            this.logEvents = logEvents; 
        }


        // handle a line from the log file.
        // If we find what we are seeking, fire off our event
        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            bool rv = false;

            GroupLeaderEvent groupLeaderEvent = Match(line, timestamp, lineCounter);
            if (groupLeaderEvent != null)
            {
                logEvents.Handle(groupLeaderEvent);
                rv = true;
            }

            return rv;
        }

        // parse this line to see if it containns the search phrase
        public GroupLeaderEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            GroupLeaderEvent rv = null;

            // you join a group
            var match = regexYouJoin.Match(line);
            if (match.Success)
            {
                rv = new GroupLeaderEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    GroupLeaderName = match.Groups["group_leader"].Value
                };
            }

            // you invite someone to join a group
            match = regexYouInvite.Match(line);
            if (match.Success)
            {
                rv = new GroupLeaderEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    GroupLeaderName = "You"
                };
            }

            // group leader changed
            match = regexLeaderChanged.Match(line);
            if (match.Success)
            {
                rv = new GroupLeaderEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    GroupLeaderName = match.Groups["group_leader"].Value
                };
            }

            // removed from the group or group disbanded
            if ((line == "Your group has been disbanded.") || (line == "You have been removed from the group."))
            {
                rv = new GroupLeaderEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    GroupLeaderName = "None"
                };
            }

            return rv;
        }


    }
}
