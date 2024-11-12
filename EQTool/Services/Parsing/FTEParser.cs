using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{


    public class FTEParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        public FTEParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = Parse(line, timestamp, lineCounter);
            if (m != null)
            {
                logEvents.Handle(m);
                return true;
            }
            return false;
        }

        public FTEEvent Parse(string line, DateTime timestamp, int lineCounter)
        {
            var endwithexclimation = line.EndsWith("!");
            if (!endwithexclimation)
            {
                return null;
            }

            var lastspaceindex = line.LastIndexOf(" ");
            if (lastspaceindex == -1)
            {
                return null;
            }

            var engagesstring = " engages ";
            var engagesindex = line.LastIndexOf(engagesstring);
            if (engagesindex == -1)
            {
                return null;
            }

            if (lastspaceindex != (engagesindex + engagesstring.Length - 1))
            {
                return null;
            }

            var playername = line.Substring(engagesindex + engagesstring.Length).TrimEnd('!').Trim();
            var npcname = line.Substring(0, engagesindex).Trim();
            return new FTEEvent
            {
                FTEPerson = playername,
                NPCName = npcname,
                TimeStamp = timestamp,
                LineCounter = lineCounter,
                Line = line
            };
        }
    }
}
