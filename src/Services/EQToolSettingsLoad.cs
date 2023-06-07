using EQTool.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace EQTool.Services
{
    public class EQToolSettingsLoad
    {
        private readonly FindEq findEq;
        private readonly LoggingService loggingService;
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
                                item.ShowSpellsForClasses = new System.Collections.Generic.List<PlayerClasses>();
                            }
                        }
                        return ret1;
                    }
                }
                catch (Exception e)
                {
                    if (counter++ < 1)
                    {
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
                },
                Theme = Themes.Light
            };
            return ret;
        }

        public void Save(EQToolSettings model)
        {
            try
            {
                var txt = JsonConvert.SerializeObject(model, Formatting.Indented);
                File.WriteAllText("settings.json", txt);
            }
            catch (Exception e)
            {
                loggingService.Log(e.ToString(), App.EventType.Error);
            }
        }
    }
}
