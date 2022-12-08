using EQTool.Models;
using Newtonsoft.Json;
using System.IO;

namespace EQTool.Services
{
    public class EQToolSettingsLoad
    {
        private readonly FindEq findEq;
        public EQToolSettingsLoad(FindEq findEq)
        {
            this.findEq = findEq;
        }

        public EQToolSettings Load()
        {
            if (File.Exists("settings.json"))
            {
                try
                {
                    using (var r = new StreamReader("settings.json"))
                    {
                        var json = r.ReadToEnd();
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
                }
                catch { }
            }

            var ret = new EQToolSettings
            {
                DefaultEqDirectory = findEq.LoadEQPath(),
                Theme = "Light.Blue",
                BestGuessSpells = true,
                FontSize = 12,
                GlobalTriggerWindowOpacity = .5,
                GlobalDPSWindowOpacity = .5,
                YouOnlySpells = false,
                Players = new System.Collections.Generic.List<PlayerInfo>(),
                TriggerWindowTopMost = true
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
            catch
            {

            }
        }
    }
}
