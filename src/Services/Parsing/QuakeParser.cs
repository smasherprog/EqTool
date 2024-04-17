using EQTool.Services.Parsing;
using static EQTool.Services.EventsList;

namespace EQTool.Services
{
    public class QuakeParser : ILogParser
    {
        private readonly EventsList eventsList;

        public QuakeParser(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string line)
        {
            if (line.Contains("You feel you should get somewhere safe as soon as possible"))
            {
                this.eventsList.Handle(new QuakeArgs());
                return true;
            }
            return false;
        }
    }
}
