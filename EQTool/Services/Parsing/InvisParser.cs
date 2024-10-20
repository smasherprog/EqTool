using EQTool.Models;
using System; 

namespace EQTool.Services.Parsing
{
    public class InvisParser : IEqLogParseHandler
    {
        public enum InvisStatus
        {
            Fading,
            Faded,
            Applied
        }
        private readonly LogEvents logEvents;

        public InvisParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }
        public bool Handle(string line, DateTime timestamp)
        {
            var m = Parse(line);
            if (m != null)
            { 
                logEvents.Handle(new InvisEvent { InvisStatus = m.Value, TimeStamp = timestamp });
                return true;
            }
            return false;
        }

        public InvisStatus? Parse(string line)
        {
            return line == "You feel yourself starting to appear." ? InvisStatus.Fading : (InvisStatus?)null;
        }
    }
}
