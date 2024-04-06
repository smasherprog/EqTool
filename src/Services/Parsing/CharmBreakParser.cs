using EQTool.Services.Parsing;
using static EQTool.Services.EventsList;

namespace EQTool.Services
{
    public class CharmBreakParser : ILogParser
    {
        private readonly EventsList eventsList;

        public CharmBreakParser(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string line)
        {
            if (line == "Your charm spell has worn off.")
            {
                this.eventsList.Handle(new CharmBreakArgs());
                return true;
            }
            return false;
        }
    }
}
