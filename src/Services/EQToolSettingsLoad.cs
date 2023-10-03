using EQTool.Models;
using EQToolShared.Enums;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace EQTool.Services
{
    public class EQToolSettingsLoad
    {
        private readonly FindEq findEq;
        private readonly LoggingService loggingService;
        private readonly object filelock = new object();

        public EQToolSettingsLoad(FindEq findEq, LoggingService loggingService)
        {
            this.findEq = findEq;
            this.loggingService = loggingService;
        }

        public EQToolSettings Load(int counter = 0)
        {
            if (File.Exists("settings.json"))
            {
                try
                {
                    var json = File.ReadAllText("settings.json");
                    var ret1 = JsonConvert.DeserializeObject<EQToolSettings>(json);
                    if (ret1 != null)
                    {
                        if (ret1.Players == null)
                        {
                            ret1.Players = new System.Collections.Generic.List<PlayerInfo>();
                        }

                        foreach (var item in ret1.Players)
                        {
                            if (item.ShowSpellsForClasses == null)
                            {
                                item.ShowSpellsForClasses = Enum.GetValues(typeof(PlayerClasses)).Cast<PlayerClasses>().ToList();
                            }
                        }
                        return ret1;
                    }
                }
                catch (Exception e)
                {
                    if (counter++ < 1)
                    {
                        Thread.Sleep(1000);
                        return Load(counter);
                    }

                    loggingService.Log(e.ToString(), App.EventType.Error);

                }
            }
            var match = findEq.LoadEQPath();
            var ret = new EQToolSettings
            {
                DefaultEqDirectory = match?.EqBaseLocation,
                EqLogDirectory = match?.EQlogLocation,
                BestGuessSpells = true,
                YouOnlySpells = false,
                Players = new System.Collections.Generic.List<PlayerInfo>(),
                DpsWindowState = new WindowState
                {
                    Closed = false,
                    State = System.Windows.WindowState.Normal
                },
                MapWindowState = new WindowState
                {
                    Closed = false,
                    State = System.Windows.WindowState.Normal
                },
                MobWindowState = new WindowState
                {
                    Closed = false,
                    State = System.Windows.WindowState.Normal
                },
                SpellWindowState = new WindowState
                {
                    Closed = false,
                    State = System.Windows.WindowState.Normal
                }
            };
            return ret;
        }

        public void Save(EQToolSettings model)
        {
            try
            {
                var txt = JsonConvert.SerializeObject(model, Formatting.Indented);
                lock (filelock)
                {
                    File.WriteAllText("settings.json", txt);
                }
            }
            catch (Exception)
            {
                //loggingService.Log(e.ToString(), App.EventType.Error);
            }
        }
    }
}
