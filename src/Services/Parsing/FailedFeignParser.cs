using EQTool.Services.Parsing;
using EQTool.ViewModels;

namespace EQTool.Services
{
    public class FailedFeignParser : ILogParser
    {
        private readonly ActivePlayer activePlayer;
        private readonly EventsList eventsList;

        public FailedFeignParser(ActivePlayer activePlayer, EventsList eventsList)
        {
            this.activePlayer = activePlayer;
            this.eventsList = eventsList;
        }

        public bool Evaluate(string message, string previousline)
        {
            if (message == $"{activePlayer?.Player?.Name} has fallen to the ground.")
            {
                this.eventsList.HandleFailedFeign(activePlayer?.Player?.Name);
                return true;
            }

            return false;
        }
    }
}
