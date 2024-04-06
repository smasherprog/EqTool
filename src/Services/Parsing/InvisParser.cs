using EQTool.Services.Parsing;

namespace EQTool.Services
{
    public class InvisParser : ILogParser
    {
        public enum InvisStatus
        {
            Fading,
            Faded,
            Applied
        }

        private readonly EventsList eventsList;

        public InvisParser(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string line)
        {
            if (line == "You feel yourself starting to appear.")
            {
                this.eventsList.Handle(InvisStatus.Fading);
                return true;
            }
            return false;
        }
    }
}
