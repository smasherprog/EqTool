using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            try
            {
                if (!File.Exists("FightLog.csv"))
                {
                    File.AppendAllLines("FightLog.csv", new string[] { "Start,End,Target,Source,TotalDamage,Dps,12 Second Highest Damage,Biggest Hit" });
                }
                var datatoadd = new List<string>();
                foreach (var item in entities)
                {
                    var endtime = item.LastDamageDone ?? (item.DeathTime.HasValue ? item.DeathTime.Value : DateTime.Now);
                    datatoadd.Add($"{item.StartTime:MM/dd/yyyy hh:mm tt},{endtime:MM/dd/yyyy hh:mm tt},{item.TargetName},{item.SourceName},{item.TotalDamage},{item.TotalDPS},{item.TotalTwelveSecondDamage},{item.HighestHit}");
                }
                File.AppendAllLines("FightLog.csv", datatoadd);
            }
            catch (Exception)
            {

            }

        }
    }
}
