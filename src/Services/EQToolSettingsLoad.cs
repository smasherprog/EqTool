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
                        return JsonConvert.DeserializeObject<EQToolSettings>(json);
                    }
                }
                catch { }
            }

            var ret = new EQToolSettings
            {
                DefaultEqDirectory = findEq.LoadEQPath(),
                BestGuessSpells = true,
                FontSize = 12,
                GlobalTriggerWindowOpacity = .5,
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
