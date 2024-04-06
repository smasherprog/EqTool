using EQTool.Services.Parsing;

namespace EQTool.Services
{
    public class QuakeParser : ILogParser
    {
        private readonly EventsList eventsList;

        public QuakeParser(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }


        public bool IsQuake(string line)
        {
            return line.Contains("You feel you should get somewhere safe as soon as possible");
        }
    }
}
