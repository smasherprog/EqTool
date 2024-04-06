using EQTool.Services.Parsing;
using EQTool.ViewModels;

namespace EQTool.Services.Spells.Log
{
    public class LevelLogParse : ILogParser
    {
        private readonly ActivePlayer activePlayer;
        private readonly string YouHaveGainedALevel = "You have gained a level! Welcome to level";

        public LevelLogParse(ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
        }

        public bool Evaluate(string line)
        {
            if (line.StartsWith(YouHaveGainedALevel))
            {
                var levelstring = line.Replace(YouHaveGainedALevel, string.Empty).Trim().TrimEnd('!');
                if (int.TryParse(levelstring, out var level))
                {
                    var player = activePlayer.Player;
                    if (player != null)
                    {
                        player.Level = level;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
