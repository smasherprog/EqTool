using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EQTool.Services.Parsing
{


    //
    // parser for user-defined triggers
    //
    public class UserDefinedTriggerParser : IEqLogParser
    { 
        // todo - revise this to read from disk
        public static List<UserDefinedTrigger> triggerList = new List<UserDefinedTrigger>()
            {
                new UserDefinedTrigger { TriggerID = 100, TriggerEnabled = true, TriggerName = "Spell Interrupted", SearchText = "^Your spell is interrupted.", TextEnabled = true, DisplayText = "Spell Interrupted", AudioEnabled = true, AudioText = "Interrupted" },
                new UserDefinedTrigger { TriggerID = 101, TriggerEnabled = true, TriggerName = "Spell Fizzle", SearchText = "^Your spell fizzles!", TextEnabled = true, DisplayText = "Spell Fizzles", AudioEnabled = true, AudioText = "Fizzle" },
                new UserDefinedTrigger { TriggerID = 102, TriggerEnabled = true, TriggerName = "Backstabber", SearchText = "^{backstabber} backstabs {target} for {damage} points of damage\\.", TextEnabled = true, DisplayText = "{backstabber} backstabs {target} for {damage}", AudioEnabled = true, AudioText = "Backstabber" },
                new UserDefinedTrigger { TriggerID = 103, TriggerEnabled = true, TriggerName = "Corpse Need Consent", SearchText = "^You do not have consent to summon that corpse", TextEnabled = true, DisplayText = "Need Consent", AudioEnabled = true, AudioText = "Need Consent" },
                new UserDefinedTrigger { TriggerID = 104, TriggerEnabled = true, TriggerName = "Corpse Out of Range", SearchText = "^The corpse is too far away to summon", TextEnabled = true, DisplayText = "Corpse OOR", AudioEnabled = true, AudioText = "Corpse out of range" },
                new UserDefinedTrigger { TriggerID = 105, TriggerEnabled = true, TriggerName = "Select a Target", SearchText = "^(You must first select a target for this spell)|(You must first click on the being you wish to attack)", TextEnabled = true, DisplayText = "Select a target", AudioEnabled = true, AudioText = "Select a target" },
                new UserDefinedTrigger { TriggerID = 106, TriggerEnabled = true, TriggerName = "Insufficient Mana", SearchText = "^Insufficient Mana to cast this spell!", TextEnabled = true, DisplayText = "OOM", AudioEnabled = true, AudioText = "out of mana" },
                new UserDefinedTrigger { TriggerID = 107, TriggerEnabled = true, TriggerName = "Target Out of Range", SearchText = "^Your target is out of range", TextEnabled = true, DisplayText = "Target out of range", AudioEnabled = true, AudioText = "Out of range" },
                new UserDefinedTrigger { TriggerID = 108, TriggerEnabled = true, TriggerName = "Spell Did Not Take Hold", SearchText = "^Your spell did not take hold", TextEnabled = true, DisplayText = "Spell did not take hold", AudioEnabled = true, AudioText = "Spell did not take hold" },
                new UserDefinedTrigger { TriggerID = 109, TriggerEnabled = true, TriggerName = "Must be standing to cast", SearchText = "^(You must be standing)|(You are too distracted to cast a spell now)", TextEnabled = true, DisplayText = "Stand up!", AudioEnabled = true, AudioText = "stand up" },
                new UserDefinedTrigger { TriggerID = 110, TriggerEnabled = true, TriggerName = "Dispelled", SearchText = "^You feel a bit dispelled", TextEnabled = true, DisplayText = "You have been dispelled", AudioEnabled = true, AudioText = "dispelled" },
                new UserDefinedTrigger { TriggerID = 111, TriggerEnabled = true, TriggerName = "Regen Faded", SearchText = "^You have stopped regenerating", TextEnabled = true, DisplayText = "===== Regen faded =====", AudioEnabled = true, AudioText = "re-gen faded" },
                new UserDefinedTrigger { TriggerID = 112, TriggerEnabled = true, TriggerName = "Can't See Target", SearchText = "^You can't see your target", TextEnabled = true, DisplayText = "Can't see target", AudioEnabled = true, AudioText = "Can't see target" },
                new UserDefinedTrigger { TriggerID = 113, TriggerEnabled = true, TriggerName = "Sense Heading", SearchText = "^You think you are heading {direction}", TextEnabled = true, DisplayText = "Direction = {direction}", AudioEnabled = true, AudioText = "{direction}" },
                new UserDefinedTrigger { TriggerID = 114, TriggerEnabled = true, TriggerName = "Sense Heading Failed", SearchText = "^You have no idea what direction you are facing", TextEnabled = true, DisplayText = "No idea", AudioEnabled = true, AudioText = "no idea" },
            };

        private readonly ActivePlayer activePlayer;
        private readonly LogEvents logEvents;

        // ctor
        public UserDefinedTriggerParser(ActivePlayer activePlayer, LogEvents logEvents) 
        {
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
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
            // also, need to ensure this handler is FIRST, so a hard-coded PigParser handler doesn't handle it first and prevent this handler
            // from getting a shot at it
            return false;
        }

        // check if this line matches this trigger
        UserDefinedTriggerEvent Match(UserDefinedTrigger trigger, string line, DateTime timestamp, int lineCounter)
        {
            // return value
            UserDefinedTriggerEvent returnValue = null;

            // do the regex search
            Regex regex = new Regex(trigger.SearchText);
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
    }
}
