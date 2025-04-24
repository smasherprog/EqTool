using EQTool.Models;
using EQToolShared.Enums;
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
                        AddMissingEnums(ret1);
                        MigrateOldTriggersToNewTriggers(ret1);
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
                FontSize = 12,
                ShowRandomRolls = true
            };
            AddMissingEnums(ret);
            MigrateOldTriggersToNewTriggers(ret);
            return ret;
        }

        private void MigrateOldTriggersToNewTriggers(EQToolSettings ret1)
        {
            if (!ret1.Triggers.Any())
            {
                var triggers = ReadTriggers();
                foreach (var trigger in triggers)
                {
                    ret1.Triggers.Add(new Models.Trigger
                    {
                        TriggerEnabled = trigger.TriggerEnabled,
                        TriggerName = trigger.TriggerName,
                        SearchText = trigger.SearchText,
                        AudioEnabled = trigger.AudioEnabled,
                        AudioText = trigger.AudioText,
                        DisplayText = trigger.DisplayText,
                        DisplayTextEnabled = trigger.TextEnabled
                    });
                }
            }
        }

        private static void CreateDefaultTriggerFile(string fileName)
        {
            try
            {

                var fileContents = new List<string>
            {
                "#",
                "# comment line:                 hashtag # in first column",
                "# field separator symbol:       semi-colon ;",
                "#",
                "# fields:",
                "#   triggerID       int         unique value for this trigger",
                "#   triggerEnabled  int         1/0 boolean enable/disable this trigger",
                "#   triggerName     string      descriptive name",
                "#   searchText      string      pattern to match (can be regular expression)",
                "#   textEnabled     int         1/0 boolean enable/disable text alerts",
                "#   displayText     string      text to be displayed when this trigger finds a matching line in the log",
                "#   audioEnabled    int         1/0 boolean enable/disable audible text-to-speech alerts",
                "#   audioText       string      text to be spoken when this trigger finds a matching line in the log",
                "# ",
                "# triggerID;triggerEnabled;triggerName;searchText;textEnabled;displayText;audioEnabled;audioText",
                "#",
                "100;1;Spell Interrupted;^Your spell is interrupted.;1;Spell Interrupted;0;Interrupted",
                "101;1;Spell Fizzle;^Your spell fizzles!;1;Spell Fizzles;0;Fizzle",
                "102;1;Backstabber;^{backstabber} backstabs {target} for {damage} points of damage.;1;{backstabber} backstabs {target} for {damage};0;Backstabber",
                "103;1;Corpse Need Consent;^You do not have consent to summon that corpse;1;Need Consent;0;Need Consent",
                "104;1;Corpse Out of Range;^The corpse is too far away to summon;1;Corpse OOR;0;Corpse out of range",
                "105;1;Select a Target;^(You must first select a target for this spell)|(You must first click on the being you wish to attack);1;Select a target;0;Select a target",
                "106;1;Insufficient Mana;^Insufficient Mana to cast this spell!;1;OOM;0;out of mana",
                "107;1;Target Out of Range;^Your target is out of range;1;Target out of range;0;Out of range",
                "108;1;Spell Did Not Take Hold;^Your spell did not take hold;1;Spell did not take hold;0;Spell did not take hold",
                "109;1;Must be standing to cast;^(You must be standing)|(You are too distracted to cast a spell now);1;Stand up!;0;stand up",
                "110;1;Dispelled;^You feel a bit dispelled;1;You have been dispelled;0;dispelled",
                "111;1;Regen Faded;^You have stopped regenerating;1;===== Regen faded =====;0;re-gen faded",
                "112;1;Can't See Target;^You can't see your target;1;Can't see target;0;Can't see target",
                "113;1;Sense Heading;^You think you are heading {direction};1;Direction = {direction};0;{direction}",
                "114;1;Sense Heading Failed;^You have no idea what direction you are facing;1;No idea;0;no idea"
            };

                // write the file
                Console.WriteLine($"Creating file: [{fileName}]");
                using (var writer = new StreamWriter(fileName))
                {
                    foreach (var line in fileContents)
                    {
                        // write a line
                        // Console.WriteLine(line);
                        writer.WriteLine(line);
                    }
                }
            }
            catch { }
        }

        private static List<UserDefinedTrigger> ReadTriggers()
        {
            var ret = new List<UserDefinedTrigger>();
            try
            {
                var userTriggerFileName = "UserTriggers.txt";
                // if the trigger file does not exist, then create a starter default set
                if (File.Exists(userTriggerFileName) == false)
                {
                    CreateDefaultTriggerFile(userTriggerFileName);
                }
                // read the file, open it in read-only mode
                Console.WriteLine($"Reading UserTrigger file: [{userTriggerFileName}]");
                var fs = new FileStream(userTriggerFileName, FileMode.Open, FileAccess.Read);
                using (var reader = new StreamReader(fs))
                {
                    // read a line
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // attempt to create a trigger from this line
                        var commentChar = '#';
                        if (line.IndexOf(commentChar) != 0)
                        {
                            // split the line into fields
                            var fieldSeparater = ';';
                            var fields = line.Split(fieldSeparater);
                            if (fields.Count() == 8)
                            {
                                // create the trigger and add it to the list
                                var t = new UserDefinedTrigger()
                                {
                                    TriggerID = int.Parse(fields[0]),
                                    TriggerEnabled = int.Parse(fields[1]) == 1,
                                    TriggerName = fields[2],
                                    SearchText = fields[3],
                                    TextEnabled = int.Parse(fields[4]) == 1,
                                    DisplayText = fields[5],
                                    AudioEnabled = int.Parse(fields[6]) == 1,
                                    AudioText = fields[7],
                                };
                                ret.Add(t);
                            }
                        }
                    }
                }
            }
            catch { }
            return ret;
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
