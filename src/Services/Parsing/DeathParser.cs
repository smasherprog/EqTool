using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class DeathTouchParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        public DeathTouchParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = DtCheck(line);
            if (m != null)
            {
                logEvents.Handle(m);
                return true;
            }
            return false;
        }

        public DeathTouchEvent DtCheck(string line)
        {
            if (line.StartsWith("Fright says '"))
            {
                var firsttick = line.IndexOf("'") + 1;
                var lasttick = line.LastIndexOf("'") - firsttick;
                var possiblename = line.Substring(firsttick, lasttick);
                if (!possiblename.Contains(" "))
                {
                    return new DeathTouchEvent { NpcName = "Fright", DTReceiver = possiblename };
                }
            }

            if (line.StartsWith("Dread says '"))
            {
                var firsttick = line.IndexOf("'") + 1;
                var lasttick = line.LastIndexOf("'") - firsttick;
                var possiblename = line.Substring(firsttick, lasttick);
                if (!possiblename.Contains(" "))
                {
                    return new DeathTouchEvent { NpcName = "Dread", DTReceiver = possiblename };
                }
            }
            return null;
        }
    }
}
