using EQTool.Services.Parsing;

namespace EQTool.Services
{
    public class FTEParser : ILogParser
    {
        public class FTEParserData
        {
            public string NPCName { get; set; }
            public string FTEPerson { get; set; }
        }
        private readonly EventsList eventsList;
        public FTEParser(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string message, string previousline)
        {
            var endwithexclimation = message.EndsWith("!");
            if (!endwithexclimation)
            {
                return false;
            }

            var lastspaceindex = message.LastIndexOf(" ");
            if (lastspaceindex == -1)
            {
                return false;
            }

            var engagesstring = " engages ";
            var engagesindex = message.LastIndexOf(engagesstring);
            if (engagesindex == -1)
            {
                return false;
            }

            if (lastspaceindex != (engagesindex + engagesstring.Length - 1))
            {
                return false;
            }

            var playername = message.Substring(engagesindex + engagesstring.Length).TrimEnd('!').Trim();
            var npcname = message.Substring(0, engagesindex).Trim();
            this.eventsList.Handle(new FTEParserData
            {
                FTEPerson = playername,
                NPCName = npcname
            });
            return true;
        }
    }
}
