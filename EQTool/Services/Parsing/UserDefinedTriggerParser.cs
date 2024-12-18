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
        // x 100;1;Spell Interrupted;^Your spell is interrupted.;1;Interrupted;1;Interrupted
        // x 101;1;Spell Fizzle;^Your spell fizzles!;1;Fizzle;1;Fizzle
        //102;1;Backstabber;^(?<backstabber>[\w` ]+) backstabs [\w` ]+ for [0-9]+ points of damage;1;Backstabber: {s};1;Backstabber {s}
        // x 103;1;Corpse Need Consent;^You do not have consent to summon that corpse;1;Need consent;1;Need consent
        // x 104;1;Corpse Out of Range;^The corpse is too far away to summon;1;Corpse OOR;1;Corpse out of range
        // x 105;1;Select a Target;^(You must first select a target for this spell)|(You must first click on the being you wish to attack);1;Select a target;1;select a target
        // x 106;1;Insufficient Mana;^Insufficient Mana to cast this spell!;1;OOM;1;Out of Mana
        // x 107;1;Target Out of Range;^Your target is out of range;1;Target OOR;1;Out of range
        // x 108;1;Spell Did Not Take Hold;^Your spell did not take hold;1;Spell did not take hold;1;Spell did not take hold
        // x 109;1;You must be standing to cast;^(You must be standing)|(You are too distracted to cast a spell now);1; Stand Up!;1; Stand Up!
        // x 110;1;Dispelled;^You feel a bit dispelled;1;Dispelled;1;Dispelled
        // x 111;1;Regen faded;^You have stopped regenerating;1;===== Regen faded =====;1;re-gen faded
        // x 112;1;Can't See Your Target;^You can't see your target;1;Can't see target;1;Can't see target
        //113;1;Sense Heading;^You think you are heading (?<direction>[\w]+)\.;1;{s};1;{s}
        // x 114;1;Sense Heading Fail;^You have no idea what direction you are facing\.;1;No idea;1;No idea

        static List<UserDefinedTrigger> triggerList = new List<UserDefinedTrigger>()
            {
                new UserDefinedTrigger { TriggerID = 100, TriggerEnabled = true, TriggerName = "Spell Interrupted", SearchTest = "^Your spell is interrupted.", TextEnabled = true, DisplayText = "Spell Interrupted", AudioEnabled = true, AudioText = "Interrupted" },
                new UserDefinedTrigger { TriggerID = 101, TriggerEnabled = true, TriggerName = "Spell Fizzle", SearchTest = "^Your spell fizzles!", TextEnabled = true, DisplayText = "Spell Fizzles", AudioEnabled = true, AudioText = "Fizzle" },
                new UserDefinedTrigger { TriggerID = 103, TriggerEnabled = true, TriggerName = "Corpse Need Consent", SearchTest = "^You do not have consent to summon that corpse", TextEnabled = true, DisplayText = "Need Consent", AudioEnabled = true, AudioText = "Need Consent" },
                new UserDefinedTrigger { TriggerID = 104, TriggerEnabled = true, TriggerName = "Corpse Out of Range", SearchTest = "^The corpse is too far away to summon", TextEnabled = true, DisplayText = "Corpse OOR", AudioEnabled = true, AudioText = "Corpse out of range" },
                new UserDefinedTrigger { TriggerID = 105, TriggerEnabled = true, TriggerName = "Select a Target", SearchTest = "^(You must first select a target for this spell)|(You must first click on the being you wish to attack)", TextEnabled = true, DisplayText = "Select a target", AudioEnabled = true, AudioText = "Select a target" },
                new UserDefinedTrigger { TriggerID = 106, TriggerEnabled = true, TriggerName = "Insufficient Mana", SearchTest = "^Insufficient Mana to cast this spell!", TextEnabled = true, DisplayText = "OOM", AudioEnabled = true, AudioText = "out of mana" },
                new UserDefinedTrigger { TriggerID = 107, TriggerEnabled = true, TriggerName = "Target Out of Range", SearchTest = "^Your target is out of range", TextEnabled = true, DisplayText = "Target out of range", AudioEnabled = true, AudioText = "Out of range" },
                new UserDefinedTrigger { TriggerID = 108, TriggerEnabled = true, TriggerName = "Spell Did Not Take Hold", SearchTest = "^Your spell did not take hold", TextEnabled = true, DisplayText = "Spell did not take hold", AudioEnabled = true, AudioText = "Spell did not take hold" },
                new UserDefinedTrigger { TriggerID = 109, TriggerEnabled = true, TriggerName = "Must be standing to cast", SearchTest = "^(You must be standing)|(You are too distracted to cast a spell now)", TextEnabled = true, DisplayText = "Stand up!", AudioEnabled = true, AudioText = "stand up" },
                new UserDefinedTrigger { TriggerID = 110, TriggerEnabled = true, TriggerName = "Dispelled", SearchTest = "^You feel a bit dispelled", TextEnabled = true, DisplayText = "You have been dispelled", AudioEnabled = true, AudioText = "dispelled" },
                new UserDefinedTrigger { TriggerID = 111, TriggerEnabled = true, TriggerName = "Regen Faded", SearchTest = "^You have stopped regenerating", TextEnabled = true, DisplayText = "===== Regen faded =====", AudioEnabled = true, AudioText = "re-gen faded" },
                new UserDefinedTrigger { TriggerID = 112, TriggerEnabled = true, TriggerName = "Can't See Target", SearchTest = "^You can't see your target", TextEnabled = true, DisplayText = "Can't see target", AudioEnabled = true, AudioText = "Can't see target" },
                new UserDefinedTrigger { TriggerID = 114, TriggerEnabled = true, TriggerName = "Sense Heading Failed", SearchTest = "^You have no idea what direction you are facing", TextEnabled = true, DisplayText = "No idea", AudioEnabled = true, AudioText = "no idea" },
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
            Regex regex = new Regex(trigger.SearchTest);
            var match = regex.Match(line);
            if (match.Success)
            {
                Console.WriteLine($"Trigger: {trigger.TriggerName}, Line: {line}");

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
