using EQTool.ViewModels;

namespace EQTool.Services.Parsing
{
    public class FailedFeignParser
    {
        private readonly ActivePlayer activePlayer;

        public FailedFeignParser(ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
        }

        public string FailedFaignCheck(string line)
        {
            return line == $"{activePlayer?.Player?.Name} has fallen to the ground." ? (activePlayer?.Player?.Name) : string.Empty;
        }
    }
}
