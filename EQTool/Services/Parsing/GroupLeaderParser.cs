using EQTool.Models;
using System;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    public class GroupLeaderParser : IEqLogParser
    {
        private const string patternYouJoin = @"^You notify (?<group_leader>[\w` ]+) that you agree to join the group.";
        private const string patternYouInvite = @"^You invite [\w` ]+ to join your group.";
        private const string patternLeaderChanged = @"(?<group_leader>[\w` ]+) (is|are) now the leader of your group.";

        private readonly Regex regexYouJoin = new Regex(patternYouJoin, RegexOptions.Compiled);
        private readonly Regex regexYouInvite = new Regex(patternYouInvite, RegexOptions.Compiled);
        private readonly Regex regexLeaderChanged = new Regex(patternLeaderChanged, RegexOptions.Compiled);


        private readonly LogEvents logEvents;

        public GroupLeaderParser(LogEvents logEvents)
        {
            this.logEvents = logEvents; 
        }


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
