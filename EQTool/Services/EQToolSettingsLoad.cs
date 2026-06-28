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
                        var changed = EnableAvatarOfWarTriggerIfMissing(ret1);
                        changed |= EnableVPHoskarRestoTriggerIfMissing(ret1);
                        changed |= EnableSpellWornOffTriggerIfMissing(ret1);
                        if (changed)
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
            if (ret.Triggers == null)
            {
                ret.Triggers = new List<Trigger>();
            }
            _ = EnableAvatarOfWarTriggerIfMissing(ret);
            _ = EnableVPHoskarRestoTriggerIfMissing(ret);
            _ = EnableSpellWornOffTriggerIfMissing(ret);
            return ret;
        }

        // Seeds and enables ONLY the Avatar of War lockout built-in, and only when the user doesn't
        // already have it (matched by BuiltInId). Once it's in the user's trigger list they can
        // disable or delete it without it coming back. This replaces the old always-on AOWTimerHandler.
        // No other built-ins are auto-enabled here.
        private bool EnableAvatarOfWarTriggerIfMissing(EQToolSettings settings)
        {
            // Already in the user's trigger list? Leave it exactly as the user has it.
            if (settings.Triggers.Any(a => string.Equals(a.BuiltInId, BuiltInTriggers.AvatarOfWarBuiltInId, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var copy = BuiltInTriggers.CreateAvatarOfWarLockout();
            copy.TriggerId = Guid.NewGuid();
            copy.FolderId = null;
            copy.BuiltInId = BuiltInTriggers.AvatarOfWarBuiltInId;
            copy.TriggerEnabled = true;
            settings.Triggers.Add(copy);
            return true;
        }

        // Seeds and enables ONLY the VP Hoskar Resto built-in, and only when the user doesn't
        // already have it (matched by BuiltInId). Same behavior as the Avatar of War seeding:
        // once it's in the user's trigger list they can disable or delete it without it coming back.
        // This replaces the old always-on DiseasedCloudHandler.
        private bool EnableVPHoskarRestoTriggerIfMissing(EQToolSettings settings)
        {
            // Already in the user's trigger list? Leave it exactly as the user has it.
            if (settings.Triggers.Any(a => string.Equals(a.BuiltInId, BuiltInTriggers.VPHoskarRestoBuiltInId, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var copy = BuiltInTriggers.CreateVPHoskarResto();
            copy.TriggerId = Guid.NewGuid();
            copy.FolderId = null;
            copy.BuiltInId = BuiltInTriggers.VPHoskarRestoBuiltInId;
            copy.TriggerEnabled = true;
            settings.Triggers.Add(copy);
            return true;
        }

        // Seeds and enables ONLY the Spell Worn Off built-in, and only when the user doesn't
        // already have it (matched by BuiltInId). Same behavior as the Avatar of War seeding:
        // once it's in the user's trigger list they can disable or delete it without it coming back.
        // This replaces the alert portion of the old always-on SpellWornOffOtherHandler.
        private bool EnableSpellWornOffTriggerIfMissing(EQToolSettings settings)
        {
            // Already in the user's trigger list? Leave it exactly as the user has it.
            if (settings.Triggers.Any(a => string.Equals(a.BuiltInId, BuiltInTriggers.SpellWornOffBuiltInId, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var copy = BuiltInTriggers.CreateSpellWornOff();
            copy.TriggerId = Guid.NewGuid();
            copy.FolderId = null;
            copy.BuiltInId = BuiltInTriggers.SpellWornOffBuiltInId;
            copy.TriggerEnabled = true;
            settings.Triggers.Add(copy);
            return true;
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
