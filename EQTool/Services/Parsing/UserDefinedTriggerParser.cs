using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;

namespace EQTool.Services.Parsing
{


    //
    // parser for user-defined triggers
    //
    public class UserDefinedTriggerParser : IEqLogParser
    { 
        // UserTriggers file and the corresponding Triggers
        private const string userTriggerFileName = "UserTriggers.txt";
        private static readonly List<string> triggerFileContents = new List<string>();          // the raw file contents
        public static List<UserDefinedTrigger> triggerList = new List<UserDefinedTrigger>();    // the corresponding UserDefinedTriggers from the file

        private readonly ActivePlayer activePlayer;
        private readonly LogEvents logEvents;

        // watcher for the UserTriggers.txt file changed
        private readonly FileSystemWatcher watcher;

        // ctor
        public UserDefinedTriggerParser(ActivePlayer activePlayer, LogEvents logEvents) 
        {
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;

            // set up the watcher for the user triggers file changed notification
            string cwd = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine($"cwd = {cwd}");
            watcher = new FileSystemWatcher(cwd);
            watcher.NotifyFilter = NotifyFilters.Attributes
                     | NotifyFilters.CreationTime
                     | NotifyFilters.DirectoryName
                     | NotifyFilters.FileName
                     | NotifyFilters.LastAccess
                     | NotifyFilters.LastWrite
                     | NotifyFilters.Security
                     | NotifyFilters.Size;
            watcher.Changed += OnChanged;
            watcher.Filter = userTriggerFileName;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            // load triggers from file
            ReadTriggers();
        }

        // handle a line from the log file
        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            // walk the list of user defined triggers and see if this line matches any
            foreach (UserDefinedTrigger trigger in triggerList)
            {
                // is this trigger enabled?
                if (trigger.TriggerEnabled)
                {
                    // check for a match
                    UserDefinedTriggerEvent e = Match(trigger, line, timestamp, lineCounter);
                    if (e != null)
                    {
                        // found a match, so fire off the event
                        logEvents.Handle(e);
                    }
                                                             }
            }

            // return false in all cases, so a user-defined trigger doesn't accidentally usurp one of PigParser's hard coded triggers, 
            // also, need to ensure this parser is FIRST, so a hard-coded PigParser parser doesn't handle it first and prevent this
            // user defined parser from getting a shot at it
            return false;
        }

        // check if this line matches this trigger
        UserDefinedTriggerEvent Match(UserDefinedTrigger trigger, string line, DateTime timestamp, int lineCounter)
        {
            // return value
            UserDefinedTriggerEvent returnValue = null;

            // do the regex search
            Regex regex = trigger.TriggerRegex;
            var match = regex.Match(line);
            if (match.Success)
            {
                Console.WriteLine($"Trigger: {trigger.TriggerName}, Line: {line}");

                // save the values discovered in the named groups
                trigger.SaveNamedGroupValues(match);

                // populate the return value object
                returnValue = new UserDefinedTriggerEvent
                {
                    LineCounter = lineCounter,
                    Line = line,
                    TimeStamp = timestamp,
                    Trigger = trigger
                };
            }

            return returnValue;
        }

        //
        // utility function to load the trigger file
        //
        private static bool ReadTriggers()
        {
            // clear out any old triggers, if needed
            triggerFileContents.Clear();
            triggerList.Clear();

            // if the trigger file does not exist, then create a starter default set
            if (File.Exists(userTriggerFileName) == false)
            {
                CreateDefaultTriggerFile(userTriggerFileName);
            }

            // read the file, open it in read-only mode
            Console.WriteLine($"Reading UserTrigger file: [{userTriggerFileName}]");
            var fs = new FileStream(userTriggerFileName, FileMode.Open, FileAccess.Read);
            using (StreamReader reader = new StreamReader(fs))
            {
                // read a line
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    triggerFileContents.Add(line);

                    // attempt to create a trigger from this line
                    char commentChar = '#';
                    if (line.IndexOf(commentChar) != 0)
                    {
                        // split the line into fields
                        char fieldSeparater = ';';
                        string[] fields = line.Split(fieldSeparater);
                        if (fields.Count() == 8)
                        {
                            // create the trigger and add it to the list
                            UserDefinedTrigger t = new UserDefinedTrigger()
                            {
                                TriggerID = int.Parse(fields[0]),
                                TriggerEnabled = (int.Parse(fields[1]) == 1),
                                TriggerName = fields[2],
                                SearchText = fields[3],
                                TextEnabled = (int.Parse(fields[4]) == 1),
                                DisplayText = fields[5],
                                AudioEnabled = (int.Parse(fields[6]) == 1),
                                AudioText = fields[7],
                            };
                            triggerList.Add(t);
                        }
                    }
                }
            }



            return true;
        }


        // utility function to create a starter user triggers file
        private static bool CreateDefaultTriggerFile(string fileName)
        {
            List<string> fileContents = new List<string>();
            fileContents.Add("#");
            fileContents.Add("# comment line:                 hashtag # in first column");
            fileContents.Add("# field separator symbol:       semi-colon ;");
            fileContents.Add("#");
            fileContents.Add("# fields:");
            fileContents.Add("#   triggerID       int         unique value for this trigger");
            fileContents.Add("#   triggerEnabled  int         1/0 boolean enable/disable this trigger");
            fileContents.Add("#   triggerName     string      descriptive name");
            fileContents.Add("#   searchText      string      pattern to match (can be regular expression)");
            fileContents.Add("#   textEnabled     int         1/0 boolean enable/disable text alerts");
            fileContents.Add("#   displayText     string      text to be displayed when this trigger finds a matching line in the log");
            fileContents.Add("#   audioEnabled    int         1/0 boolean enable/disable audible text-to-speech alerts");
            fileContents.Add("#   audioText       string      text to be spoken when this trigger finds a matching line in the log");
            fileContents.Add("# ");
            fileContents.Add("# triggerID;triggerEnabled;triggerName;searchText;textEnabled;displayText;audioEnabled;audioText");
            fileContents.Add("#");
            fileContents.Add("100;1;Spell Interrupted;^Your spell is interrupted.;1;Spell Interrupted;0;Interrupted");
            fileContents.Add("101;1;Spell Fizzle;^Your spell fizzles!;1;Spell Fizzles;0;Fizzle");
            fileContents.Add("102;1;Backstabber;^{backstabber} backstabs {target} for {damage} points of damage.;1;{backstabber} backstabs {target} for {damage};0;Backstabber");
            fileContents.Add("103;1;Corpse Need Consent;^You do not have consent to summon that corpse;1;Need Consent;0;Need Consent");
            fileContents.Add("104;1;Corpse Out of Range;^The corpse is too far away to summon;1;Corpse OOR;0;Corpse out of range");
            fileContents.Add("105;1;Select a Target;^(You must first select a target for this spell)|(You must first click on the being you wish to attack);1;Select a target;0;Select a target");
            fileContents.Add("106;1;Insufficient Mana;^Insufficient Mana to cast this spell!;1;OOM;0;out of mana");
            fileContents.Add("107;1;Target Out of Range;^Your target is out of range;1;Target out of range;0;Out of range");
            fileContents.Add("108;1;Spell Did Not Take Hold;^Your spell did not take hold;1;Spell did not take hold;0;Spell did not take hold");
            fileContents.Add("109;1;Must be standing to cast;^(You must be standing)|(You are too distracted to cast a spell now);1;Stand up!;0;stand up");
            fileContents.Add("110;1;Dispelled;^You feel a bit dispelled;1;You have been dispelled;0;dispelled");
            fileContents.Add("111;1;Regen Faded;^You have stopped regenerating;1;===== Regen faded =====;0;re-gen faded");
            fileContents.Add("112;1;Can't See Target;^You can't see your target;1;Can't see target;0;Can't see target");
            fileContents.Add("113;1;Sense Heading;^You think you are heading {direction};1;Direction = {direction};0;{direction}");
            fileContents.Add("114;1;Sense Heading Failed;^You have no idea what direction you are facing;1;No idea;0;no idea");

            // write the file
            Console.WriteLine($"Creating file: [{fileName}]");
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                foreach (string line in fileContents)
                {
                    // write a line
                    Console.WriteLine(line);
                    writer.WriteLine(line);
                }
            }

            return true;
        }


        //
        // callback for when UserTriggers.txt changes
        //
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            // wait 1 second for the editor which presumably just saved the file can let go
            System.Threading.Thread.Sleep(1000);
            ReadTriggers();
            Console.WriteLine($"Changed: {e.FullPath}");
            
        }

    }
}
