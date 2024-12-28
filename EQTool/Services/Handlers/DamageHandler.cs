using EQTool.Models;

namespace EQTool.Services.Handlers
{
    public class DamageHandler : BaseHandler
    {
        private readonly FightHistory fightHistory;
        public DamageHandler(FightHistory fightHistory, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.fightHistory = fightHistory;
            logEvents.DamageEvent += LogEvents_DamageEvent;
        }

        private void LogEvents_DamageEvent(object sender, DamageEvent e)
        {
            fightHistory.Add(e.TargetName, e.TimeStamp);
            fightHistory.Add(e.AttackerName, e.TimeStamp);
        }
    }
}
