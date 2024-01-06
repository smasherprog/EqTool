using EQTool.ViewModels;
using EQToolShared.Map;
using System;

namespace EQTool.Services
{
    public class FTEParser
    {
        public class FTEParserData
        {
            public string NPCName { get; set; }
            public string FTEPerson { get; set; }
        }

        private readonly ActivePlayer activePlayer;

        public FTEParser(ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
        }

        public FTEParserData Parse(string line)
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
            if (ZoneParser.ZoneInfoMap.TryGetValue(this.activePlayer.Player.Zone, out var zone))
            {
                foreach (var item in zone.NotableNPCs)
                {
                    if (string.Equals(item, npcname, StringComparison.OrdinalIgnoreCase))
                    {
                        return new FTEParserData
                        {
                            FTEPerson = playername,
                            NPCName = npcname
                        };
                    }
                }
            }
            return null;
        }
    }
}
