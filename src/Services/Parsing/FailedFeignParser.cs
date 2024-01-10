using EQTool.ViewModels;

namespace EQTool.Services
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
            if (line == $"{activePlayer?.Player?.Name} has fallen to the ground.")
            {
                return activePlayer?.Player?.Name;
            }

            return string.Empty;
        }
    }
}
