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

                        if (ret1.Windows == null)
                        {
                            ret1.Windows = new System.Collections.Generic.Dictionary<WindowsEnum, WindowState>();
                        }
                        foreach (var item in ret1.Players)
                        {
                            if (item.ShowSpellsForClasses == null)
                            {
                                item.ShowSpellsForClasses = Enum.GetValues(typeof(PlayerClasses)).Cast<PlayerClasses>().ToList();
                            }
                        }
                        AddMissingEnums(ret1);
                        return ret1;
                    }
                }
                catch (Exception e)
                {
                    if (counter++ < 3)
                    {
                        Thread.Sleep(1000);
                        return Load(counter);
                    }

                    loggingService.Log(e.ToString(), EventType.Error, null);

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
                OverlayWindowState = new WindowState
                {
                    AlwaysOnTop = true,
                    Closed = false,
                    State = System.Windows.WindowState.Normal
                },
                SettingsWindowState = new WindowState
                {
                    Closed = true,
                    State = System.Windows.WindowState.Normal
                },
                Windows = new System.Collections.Generic.Dictionary<WindowsEnum, WindowState>(),
                FontSize = 12,
                ShowRandomRolls = true
            };
            AddMissingEnums(ret);
            return ret;
        }

        private void AddMissingEnums(EQToolSettings settings)
        {
            if (!settings.Windows.ContainsKey(WindowsEnum.SettingsWindow))
            {
                settings.Windows.Add(WindowsEnum.SettingsWindow, settings.SettingsWindowState);
            }
            if (!settings.Windows.ContainsKey(WindowsEnum.MapWindow))
            {
                settings.Windows.Add(WindowsEnum.MapWindow, settings.MapWindowState);
            }
            if (!settings.Windows.ContainsKey(WindowsEnum.DpsWindow))
            {
                settings.Windows.Add(WindowsEnum.DpsWindow, settings.DpsWindowState);
            }
            if (!settings.Windows.ContainsKey(WindowsEnum.OverlayWindow))
            {
                settings.Windows.Add(WindowsEnum.OverlayWindow, settings.OverlayWindowState);
            }
            if (!settings.Windows.ContainsKey(WindowsEnum.MobWindow))
            {
                settings.Windows.Add(WindowsEnum.MobWindow, settings.MobWindowState);
            }
            if (!settings.Windows.ContainsKey(WindowsEnum.SpellsWindow))
            {
                settings.Windows.Add(WindowsEnum.SpellsWindow, settings.SpellWindowState);
            }
            foreach (var player in settings.Players)
            {
                var enumsinlist = player.OverlaySettings.Select(a => a.OverlayType).ToList();
                var allenums = Enum.GetValues(typeof(OverlayTypes)).Cast<OverlayTypes>().ToList().Where(a => !enumsinlist.Contains(a));
                foreach (var item in allenums)
                {
                    player.OverlaySettings.Add(new OverLaySetting
                    {
                        OverlayType = item,
                        WarningAudio = true,
                        WarningOverlay = true,
                    });
                }
            }
        }

        public void Save(EQToolSettings model)
        {
            try
            {
                AddMissingEnums(model);
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
