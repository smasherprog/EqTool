using EQTool.Models;
using EQToolShared;

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
            if (MasterNPCList.NPCs.Contains(e.TargetName) && MasterNPCList.NPCs.Contains(e.AttackerName))
            {
                return;
            }
            fightHistory.Add(e.TargetName, e.TimeStamp);
            fightHistory.Add(e.AttackerName, e.TimeStamp);
        }
    }
}
