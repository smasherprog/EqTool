using EQTool.Services.Parsing;

namespace EQTool.Services
{
    public class LevParser : ILogParser
    {
        public enum LevStatus
        {
            Fading,
            Faded,
            Applied
        }

        private readonly EventsList eventsList;

        public LevParser(EventsList eventsList)
        {
            this.eventsList = eventsList;
        }

        public bool Evaluate(string line)
        {
            if (line == "You feel as if you are about to fall.")
            {
                this.eventsList.Handle(LevStatus.Fading);
                return true;

            }

            return false;
        }
    }
}
