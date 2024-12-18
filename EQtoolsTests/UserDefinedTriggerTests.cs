using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class UserDefinedTriggerTests : BaseTestClass
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

        private readonly LogParser logParser;
        public UserDefinedTriggerTests()
        {
            logParser = container.Resolve<LogParser>();
        }

        [TestMethod]
        public void TriggerTest1()
        {
            logParser.Push("Your spell is interrupted.", DateTime.Now);
        }




    }
}
