using EQTool.ViewModels;

namespace EQTool.Services.Spells.Log
{
    public class LevelLogParse
    {
        private readonly ActivePlayer activePlayer;
        private readonly string YouHaveGainedALevel = "You have gained a level! Welcome to level";

        public LevelLogParse(ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
        }

        public void MatchLevel(string linelog)
        {
            if (string.IsNullOrWhiteSpace(linelog) || linelog.Length >= 27)
            {
                var message = linelog.Substring(27);
                if (message.StartsWith(YouHaveGainedALevel))
                {
                    var levelstring = message.Replace(YouHaveGainedALevel, string.Empty).Trim().TrimEnd('!');
                    if (int.TryParse(levelstring, out var level))
                    {
                        var player = activePlayer.Player;
                        if (player != null)
                        {
                            player.Level = level;
                        }
                    }
                }
            }
        }
    }
}
