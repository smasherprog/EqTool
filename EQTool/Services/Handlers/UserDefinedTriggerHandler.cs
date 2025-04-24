namespace EQTool.Services.Handlers
{
    //    public class UserDefinedTriggerHandler : BaseHandler
    //    {

    //        // UserTriggers file and the corresponding Triggers
    //        private const string userTriggerFileName = "UserTriggers.txt";
    //        private static readonly List<string> triggerFileContents = new List<string>();          // the raw file contents
    //        private static List<UserDefinedTrigger> triggerList = new List<UserDefinedTrigger>();    // the corresponding UserDefinedTriggers from the file
    //        public static List<UserDefinedTrigger> TriggerList
    //        {
    //            get
    //            {
    //                // there should always be >0 triggers, so if this is true, they haven't been loaded yet.
    //                if (triggerList.Count == 0)
    //                    ReadTriggers();
    //                return triggerList;
    //            }
    //        }

    //        // watcher for the UserTriggers.txt file changed
    //        private readonly FileSystemWatcher watcher;

    //        public UserDefinedTriggerHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
    //        {
    //            var cwd = System.IO.Directory.GetCurrentDirectory();
    //            watcher = new FileSystemWatcher(cwd)
    //            {
    //                NotifyFilter = NotifyFilters.Attributes
    //                     | NotifyFilters.CreationTime
    //                     | NotifyFilters.DirectoryName
    //                     | NotifyFilters.FileName
    //                     | NotifyFilters.LastAccess
    //                     | NotifyFilters.LastWrite
    //                     | NotifyFilters.Security
    //                     | NotifyFilters.Size
    //            };
    //            watcher.Changed += OnChanged;
    //            watcher.Filter = userTriggerFileName;
    //            watcher.IncludeSubdirectories = true;
    //            watcher.EnableRaisingEvents = true;

    //            // load triggers from file
    //            _ = ReadTriggers();

    //            logEvents.LineEvent += LogEvents_LineEvent;
    //        }

    //        private void LogEvents_LineEvent(object sender, LineEvent e)
    //        {
    //            foreach (var trigger in triggerList)
    //            {
    //                // is this trigger enabled?
    //                if (trigger.TriggerEnabled)
    //                {
    //                    // check for a match
    //                    var regex = trigger.TriggerRegex;
    //                    var match = regex.Match(e.Line);
    //                    if (match.Success)
    //                    {
    //                        // save the values discovered in the named groups
    //                        trigger.SaveNamedGroupValues(match);
    //                        // text to speech?
    //                        if (trigger.AudioEnabled == true)
    //                        {
    //                            textToSpeach.Say(trigger.AudioText);
    //                        }

    //                        // displayed text?
    //                        if (trigger.TextEnabled == true)
    //                        {
    //                            _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
    //                            {
    //                                logEvents.Handle(new OverlayEvent { Text = trigger.DisplayText, ForeGround = Brushes.Red, Reset = false });
    //                                System.Threading.Thread.Sleep(5000);
    //                                logEvents.Handle(new OverlayEvent { Text = trigger.DisplayText, ForeGround = Brushes.Red, Reset = true });
    //                            });
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        //
    //        // utility function to load the trigger file
    //        //
    //        private static bool ReadTriggers()
    //        {
    //            // clear out any old triggers, if needed
    //            triggerFileContents.Clear();
    //            triggerList.Clear();

    //            // if the trigger file does not exist, then create a starter default set
    //            if (File.Exists(userTriggerFileName) == false)
    //            {
    //                _ = CreateDefaultTriggerFile(userTriggerFileName);
    //            }

    //            // read the file, open it in read-only mode
    //            Console.WriteLine($"Reading UserTrigger file: [{userTriggerFileName}]");
    //            var fs = new FileStream(userTriggerFileName, FileMode.Open, FileAccess.Read);
    //            using (var reader = new StreamReader(fs))
    //            {
    //                // read a line
    //                string line;
    //                while ((line = reader.ReadLine()) != null)
    //                {
    //                    //Console.WriteLine(line);
    //                    triggerFileContents.Add(line);

    //                    // attempt to create a trigger from this line
    //                    var commentChar = '#';
    //                    if (line.IndexOf(commentChar) != 0)
    //                    {
    //                        // split the line into fields
    //                        var fieldSeparater = ';';
    //                        var fields = line.Split(fieldSeparater);
    //                        if (fields.Count() == 8)
    //                        {
    //                            // create the trigger and add it to the list
    //                            var t = new UserDefinedTrigger()
    //                            {
    //                                TriggerID = int.Parse(fields[0]),
    //                                TriggerEnabled = int.Parse(fields[1]) == 1,
    //                                TriggerName = fields[2],
    //                                SearchText = fields[3],
    //                                TextEnabled = int.Parse(fields[4]) == 1,
    //                                DisplayText = fields[5],
    //                                AudioEnabled = int.Parse(fields[6]) == 1,
    //                                AudioText = fields[7],
    //                            };
    //                            triggerList.Add(t);
    //                        }
    //                    }
    //                }
    //            }



    //            return true;
    //        }


    //        // utility function to create a starter user triggers file
    //        private static bool CreateDefaultTriggerFile(string fileName)
    //        {
    //            var fileContents = new List<string>
    //            {
    //                "#",
    //                "# comment line:                 hashtag # in first column",
    //                "# field separator symbol:       semi-colon ;",
    //                "#",
    //                "# fields:",
    //                "#   triggerID       int         unique value for this trigger",
    //                "#   triggerEnabled  int         1/0 boolean enable/disable this trigger",
    //                "#   triggerName     string      descriptive name",
    //                "#   searchText      string      pattern to match (can be regular expression)",
    //                "#   textEnabled     int         1/0 boolean enable/disable text alerts",
    //                "#   displayText     string      text to be displayed when this trigger finds a matching line in the log",
    //                "#   audioEnabled    int         1/0 boolean enable/disable audible text-to-speech alerts",
    //                "#   audioText       string      text to be spoken when this trigger finds a matching line in the log",
    //                "# ",
    //                "# triggerID;triggerEnabled;triggerName;searchText;textEnabled;displayText;audioEnabled;audioText",
    //                "#",
    //                "100;1;Spell Interrupted;^Your spell is interrupted.;1;Spell Interrupted;0;Interrupted",
    //                "101;1;Spell Fizzle;^Your spell fizzles!;1;Spell Fizzles;0;Fizzle",
    //                "102;1;Backstabber;^{backstabber} backstabs {target} for {damage} points of damage.;1;{backstabber} backstabs {target} for {damage};0;Backstabber",
    //                "103;1;Corpse Need Consent;^You do not have consent to summon that corpse;1;Need Consent;0;Need Consent",
    //                "104;1;Corpse Out of Range;^The corpse is too far away to summon;1;Corpse OOR;0;Corpse out of range",
    //                "105;1;Select a Target;^(You must first select a target for this spell)|(You must first click on the being you wish to attack);1;Select a target;0;Select a target",
    //                "106;1;Insufficient Mana;^Insufficient Mana to cast this spell!;1;OOM;0;out of mana",
    //                "107;1;Target Out of Range;^Your target is out of range;1;Target out of range;0;Out of range",
    //                "108;1;Spell Did Not Take Hold;^Your spell did not take hold;1;Spell did not take hold;0;Spell did not take hold",
    //                "109;1;Must be standing to cast;^(You must be standing)|(You are too distracted to cast a spell now);1;Stand up!;0;stand up",
    //                "110;1;Dispelled;^You feel a bit dispelled;1;You have been dispelled;0;dispelled",
    //                "111;1;Regen Faded;^You have stopped regenerating;1;===== Regen faded =====;0;re-gen faded",
    //                "112;1;Can't See Target;^You can't see your target;1;Can't see target;0;Can't see target",
    //                "113;1;Sense Heading;^You think you are heading {direction};1;Direction = {direction};0;{direction}",
    //                "114;1;Sense Heading Failed;^You have no idea what direction you are facing;1;No idea;0;no idea"
    //            };

    //            // write the file
    //            Console.WriteLine($"Creating file: [{fileName}]");
    //            using (var writer = new StreamWriter(fileName))
    //            {
    //                foreach (var line in fileContents)
    //                {
    //                    // write a line
    //                    // Console.WriteLine(line);
    //                    writer.WriteLine(line);
    //                }
    //            }

    //            return true;
    //        }


    //        //
    //        // callback for when UserTriggers.txt changes
    //        //
    //        private static void OnChanged(object sender, FileSystemEventArgs e)
    //        {
    //            if (e.ChangeType != WatcherChangeTypes.Changed)
    //            {
    //                return;
    //            }
    //            // wait 1 second for the editor which presumably just saved the file can let go
    //            System.Threading.Thread.Sleep(1000);
    //            _ = ReadTriggers();
    //            Console.WriteLine($"Changed: {e.FullPath}");

    //        }
    //    }
}
