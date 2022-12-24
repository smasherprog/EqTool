using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQTool.Services.Fight
{
    public class FightLogService
    {
        public FightLogService()
        {

        }

        public void Log(List<EntittyDPS> entities)
        {
            if (!entities.Any())
            {
                return;
            }

            if (!File.Exists("FightLog.csv"))
            {
                File.WriteAllText("Start,End,Target,Source,TotalDamage,OverallDps,12 Second Highest Damage Delt")
            }
        }
    }
}
