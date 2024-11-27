using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class DamageHandler : BaseHandler
    {
        private readonly FightHistory fightHistory;
        public DamageHandler(
            FightHistory fightHistory,
            LogEvents logEvents,
            ActivePlayer activePlayer, 
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        { 
            this.fightHistory = fightHistory;
            this.logEvents.DamageEvent += LogEvents_DamageEvent; 
        }
         
        private void LogEvents_DamageEvent(object sender, DamageEvent e)
        { 
            this.fightHistory.Add(e.TargetName, e.TimeStamp); 
            this.fightHistory.Add(e.AttackerName, e.TimeStamp);
        } 
    }
}
