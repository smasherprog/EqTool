using EQTool.Services.Parsing;

namespace EQTool.Services
{
    public class POFDTParser : ILogParser
    {
        public class POF_DT_Event
        {
            public string NpcName { get; set; }
            public string DTReceiver { get; set; }
        }

        private readonly EventsList eventsList;

        public POFDTParser(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string line)
        {
            if (line.StartsWith("Fright says '"))
            {
                var firsttick = line.IndexOf("'") + 1;
                var lasttick = line.LastIndexOf("'") - firsttick;
                var possiblename = line.Substring(firsttick, lasttick);
                if (!possiblename.Contains(" "))
                {
                    this.eventsList.Handle(new POF_DT_Event { NpcName = "Fright", DTReceiver = possiblename });
                    return true;
                }
            }

            if (line.StartsWith("Dread says '"))
            {
                var firsttick = line.IndexOf("'") + 1;
                var lasttick = line.LastIndexOf("'") - firsttick;
                var possiblename = line.Substring(firsttick, lasttick);
                if (!possiblename.Contains(" "))
                {
                    this.eventsList.Handle(new POF_DT_Event { NpcName = "Dread", DTReceiver = possiblename });
                    return true;
                }
            }
            return false;
        }
    }
}
