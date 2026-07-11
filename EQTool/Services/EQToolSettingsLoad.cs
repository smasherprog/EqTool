using EQTool.Models;
using EQToolShared.Enums;
using EQToolShared.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace EQTool.Services
{
    public class EQToolSettingsLoad
    {
        private static readonly string settingsFilePath = Paths.InExecutableDirectory("settings.json");
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
            if (File.Exists(settingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(settingsFilePath);
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
                        AddMissingEnums(ret1);
                        if (SyncBuiltInTriggers(ret1))
                        {
                            Save(ret1);
                        }
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
                FontSize = 12,
                ShowRandomRolls = true
            };
            AddMissingEnums(ret);
            _ = SyncBuiltInTriggers(ret);
            return ret;
        }

        // Keeps the built-in triggers in settings.Triggers in sync with the definitions in code.
        // Built-ins live in the trigger list like any other trigger (so their own TriggerEnabled is
        // the single source of truth). The IsBuiltIn/BuiltInFolder markers aren't persisted, so they
        // are re-derived here by BuiltInId.
        //   - A built-in the user hasn't edited (Customized == false) has its definition refreshed
        //     from code, so fixes (regex/timer/folder) reach everyone; its enabled state + id are kept.
        //   - A built-in the user HAS edited (Customized == true) is left as-is, so their edits stick.
        //   - A newly shipped built-in is added enabled.
        // Returns whether a new built-in was added (so the caller can persist the seed).
        public static bool SyncBuiltInTriggers(EQToolSettings settings)
        {
            if (settings.Triggers == null)
            {
                settings.Triggers = new List<Trigger>();
            }

            var defs = BuiltInTriggers.All()
                .Where(b => !string.IsNullOrEmpty(b.BuiltInId))
                .GroupBy(b => b.BuiltInId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var rebuilt = new List<Trigger>();
            var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in settings.Triggers)
            {
                if (!string.IsNullOrEmpty(t.BuiltInId) && defs.TryGetValue(t.BuiltInId, out var def))
                {
                    _ = present.Add(t.BuiltInId);
                    if (t.Customized)
                    {
                        // Keep the user's edited definition; just re-derive the structural markers.
                        t.IsBuiltIn = true;
                        t.BuiltInFolder = def.BuiltInFolder;
                        rebuilt.Add(t);
                    }
                    else
                    {
                        // Refresh the definition from code, preserving enabled state + id.
                        def.TriggerEnabled = t.TriggerEnabled;
                        def.TriggerId = t.TriggerId;
                        rebuilt.Add(def);
                    }
                }
                else
                {
                    // User trigger (or an orphaned built-in no longer defined in code).
                    rebuilt.Add(t);
                }
            }

            var changed = false;
            foreach (var def in defs.Values)
            {
                if (present.Contains(def.BuiltInId))
                {
                    continue;
                }
                // Newly shipped built-in: enabled the first time the user sees it.
                def.TriggerEnabled = true;
                rebuilt.Add(def);
                changed = true;
            }

            settings.Triggers.Clear();
            settings.Triggers.AddRange(rebuilt);
            return changed;
        }

        private void AddMissingEnums(EQToolSettings settings)
        {
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
#if TESTS
                return;
#endif
                lock (filelock)
                {
                    File.WriteAllText(settingsFilePath, txt);
                }
            }
            catch (Exception)
            {
                //loggingService.Log(e.ToString(), App.EventType.Error);
            }
        }
    }
}
