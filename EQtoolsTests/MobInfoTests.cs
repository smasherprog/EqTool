using EQTool.Services.Parsing;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class MobInfoTests
    {
        private const string ResponseFromServer = @"
{{Velious Era}}
{{Namedmobpage

| imagefilename     = Mraaka.jpg

| emu_id            = 120064
| illia_id          = 6503

| name              = Mraaka
| race              = Wurm
| class             = [[Warrior]]
| level             = 66
| agro_radius       = 300
| run_speed         = 1.9

| zone              = [[Western Wastes]]
| location          = 100% @ (-1699, -73)
| respawn_time      = ?

| AC                = 492
| HP                = ~51k (?)
| HP_regen          = 36
| mana_regen        = 0

| attacks_per_round = Normal (3) - Flurry (6)
| attack_speed      = 78%
| damage_per_hit    = 161 - 264
| special           = [[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, <br />Unmezzable, See Invis

| description = Rampages and Flurries. Casts Lava Breath. Huge aggro radius and looks just like a cragwurm.

| known_loot = 

<ul><li>  {{:Exquisite Velium Warsword}}     <span class='drare'>(Uncommon)</span> <span class='ddb'>[Overall: 35.0%]</span>
</li><li> {{:Wurm Meat}}                     <span class='drare'>(Common)</span> <span class='ddb'>[Overall: 55.0%]</span>
</li><li> {{:Exquisite Velium Claidhmore}}   <span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 0.9%]</span>
</li><li> {{:Exquisite Velium Battlehammer}} <span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 3.7%]</span>
</li><li> {{:Exquisite Velium Spear}}        <span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 3.7%]</span>
</li><li> {{:Exquisite Velium Brawl Stick}}  <span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 2.8%]</span>
</li><li> {{:Exquisite Velium Battle Axe}}   <span class='drare'>(Rare)</span> <span class='ddb'>[Overall: 7.3%]</span>
</li><li> Level 50+ Velious Spells           <span class='drare'>(Uncommon)</span>
</li></ul>

| factions = 

* [[Claws of Veeshan]] <span class='profac'>(-30)</span>
* [[Yelinak]] <span class='profac'>(-30)</span>

| opposing_factions = 

* [[Kromzek]] <span class='oppfac'>(10)</span>

| related_quests = 

* None

}}

[[Category:Western Wastes]]";

        [TestMethod]
        public void ParseName()
        {
            var model = new MobInfoViewModel
            {
                Results = ResponseFromServer
            };
            Assert.AreEqual("Mraaka", model.Name);
            Assert.AreEqual("Warrior", model.Class);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

        private const string ResponsFromServer2 = @"{{Classic Era}}
{{Namedmobpage

| imagefilename     = npc_taia_lyfol.png

| emu_id            = 51095
| illia_id          = 20183

| name              = Taia Lyfol
| race              = [[Erudite]]
| class             = [[Paladin]]
| level             = 37
| agro_radius       = 35
| run_speed         = 1.25

| zone              = [[Lake Rathetear]]
| location          = 100% @ (2573, -895)

| AC                = 259
| HP                = 1739
| HP_regen          = 1
| mana_regen        = 1

| attacks_per_round = 2
| attack_speed      = 100%
| damage_per_hit    = 14  - 88
| special           = Root, Stun (spell), Heal, Lay on Hands (rarely)

| description = AFK camp, 6min 40s respawn timer. This NPC will not drop any money or items ever!

| known_loot = 

* None

| factions = 

* None

| opposing_factions = 

* None

| related_quests = 

* None

}}

[[Category:Lake Rathetear]]";

        [TestMethod]
        public void ParseName1()
        {
            var model = new MobInfoViewModel
            {
                Results = ResponsFromServer2
            };
            Assert.AreEqual("Taia Lyfol", model.Name);
            Assert.AreEqual("Paladin", model.Class);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

        private const string ResponsFromServer3 = @"{{Velious Era}}
{{Namedmobpage

| imagefilename     = npc_vilefang.png

| emu_id            = 123036
| illia_id          = 7972

| name              = Vilefang
| race              = Giant Snake
| class             = [[Warrior]]
| level             = 60
| agro_radius       = 75
| run_speed         = 1.8

| zone              = [[Dragon Necropolis]]
| location          = 100% @ (-315, -1257)
| respawn_time      = 1 Day

| AC                = 511
| HP                = 19000
| HP_regen          = 13
| mana_regen        = 0

| attacks_per_round = 2
| attack_speed      = 81%
| damage_per_hit    = 142  - 284
| special           = 
High MR, Summons
*[[Blinding Poison III]]
*[[Deadly Poison]]

| description = The single target blind can be cast from well outside melee range. [[Nightmare Hide]] can remove it.

| known_loot = 

<ul><li>  {{:Infestation}}                   <span class='drare'>(Common)</span> <span class='ddb'>[Overall: 50.0%]</span>
</li><li> {{:Poison Etched Wristband}}       <span class='drare'>(Common)</span> <span class='ddb'>[Overall: 50.0%]</span>
</li><li> {{:Chipped Fang}}                  <span class='drare'>(Always)</span> <span class='ddb'>[Overall: 17.5%]</span>
</li><li> {{:Old Dragon Horn}}               <span class='drare'>(Common)</span> <span class='ddb'>[Overall: 17.5%]</span>
</li></ul>

| factions = 

* None

| opposing_factions = 

* None

| related_quests = 

* [[Ralgyn's Promise]]

}}

[[Category:Dragon Necropolis]]";

        [TestMethod]
        public void ParseName3()
        {
            var model = new MobInfoViewModel
            {
                Results = ResponsFromServer3
            };
            Assert.AreEqual("Vilefang", model.Name);
            Assert.AreEqual("Warrior", model.Class);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

        private const string ResponsFromServer4 = @"{{Velious Era}}{{Namedmobpage

| imagefilename     = a_burning_guardian.png

| emu_id            = 124032
| illia_id          = 6708

| name              = a burning guardian
| race              = Wurm
| class             = [[Warrior]]
| level             = 64
| agro_radius       = 70
| run_speed         = 1.85

| zone              = [[Temple of Veeshan]]
| location          = 50% @ (-1040, -1040), 50% @ (-1540, -1355), 50% @ (-820, -1117)

| AC                = 543
| HP                = 25000
| HP_regen          = 18
| mana_regen        = 0

| attacks_per_round = 2
| attack_speed      = 80%
| damage_per_hit    = 146  - 296
| special           = See Invis

<b>Casts:</b>
<ul><li>[[Rain of Molten Lava]]</li>(PB AE 300 Fire Damage)
<li>[[Wave of Heat]]</li>(PB AE 200 Fire Damage)</ul>

| description = Check out [[HOT Mobs Guide]].

| known_loot = 

<ul><li>  [[Form of the Great Bear|Spell: Form of the Great Bear]]<span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 0.9%]</span>
</li><li> [[Circle of Cobalt Scar|Spell: Circle of Cobalt Scar]]<span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 0.9%]</span>
</li><li> [[Stun Command|Spell: Stun Command]]<span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 0.9%]</span>
</li><li> [[Nature Walker's Behest|Spell: Nature Walker`s Behest]]<span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 1.9%]</span>
</li><li> {{:Wurm Meat}}                     <span class='drare'>(Uncommon)</span> <span class='ddb'>[Overall: 48.1%]</span>
</li></ul>

| factions = 

* None

| opposing_factions = 

* None

| related_quests = 

* None

}}

[[Category:Temple of Veeshan]]";

        [TestMethod]
        public void ParseName2()
        {
            var model = new MobInfoViewModel
            {
                Results = ResponsFromServer4
            };
            Assert.AreEqual("a burning guardian", model.Name);
            Assert.AreEqual("Warrior", model.Class);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

        [TestMethod]
        public void ParseKnownLoot()
        {
            var cleanresults = ResponsFromServer4.Replace("\r\n", "\n").Replace("| ", "^");
            var splits = cleanresults.Split('^').Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim().TrimStart('\n')).ToList();
            var ret = MobInfoParsing.ParseKnownLoot(splits);
            Assert.IsNotNull(ret);
            Assert.AreEqual(5, ret.Count);
            Assert.IsTrue(ret.Any(a => a.Name == "Form of the Great Bear"));
            Assert.IsTrue(ret.Any(a => a.Name == "Circle of Cobalt Scar"));
            Assert.IsTrue(ret.Any(a => a.Name == "Stun Command"));
            Assert.IsTrue(ret.Any(a => a.Name == "Nature Walker's Behest"));
        }

        private const string ResponsFromServer5 = @"{{Velious Era}}{{Namedmobpage

| imagefilename     = a_burning_guardian.png

| emu_id            = 124032
| illia_id          = 6708

| name              = a burning guardian
| race              = Wurm
| class             = [[Warrior]]
| level             = 64
| agro_radius       = 70
| run_speed         = 1.85

| zone              = [[Temple of Veeshan]]
| location          = 50% @ (-1040, -1040), 50% @ (-1540, -1355), 50% @ (-820, -1117)

| AC                = 543
| HP                = 25000
| HP_regen          = 18
| mana_regen        = 0

| attacks_per_round = 2
| attack_speed      = 80%
| damage_per_hit    = 146  - 296
| special           = See Invis

<b>Casts:</b>
<ul><li>[[Rain of Molten Lava]]</li>(PB AE 300 Fire Damage)
<li>[[Wave of Heat]]</li>(PB AE 200 Fire Damage)</ul>

| description = Check out [[HOT Mobs Guide]].

| known_loot = 

<ul><li>  [[Form of the Great Bear|Spell: Form of the Great Bear]]<span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 0.9%]</span>
</li><li> [[Circle of Cobalt Scar|Spell: Circle of Cobalt Scar]]<span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 0.9%]</span>
</li><li> [[Stun Command|Spell: Stun Command]]<span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 0.9%]</span>
</li><li> [[Nature Walker's Behest|Spell: Nature Walker`s Behest]]<span class='drare'>(Ultra Rare)</span> <span class='ddb'>[Overall: 1.9%]</span>
</li><li> {{:Wurm Meat}}                     <span class='drare'>(Uncommon)</span> <span class='ddb'>[Overall: 48.1%]</span>
</li></ul>

| factions = 

* None

| opposing_factions = 

* None

| related_quests = 

* None

}}

[[Category:Temple of Veeshan]]";

        [TestMethod]
        public void ParseSpecials()
        {
            var model = new MobInfoViewModel
            {
                Results = ResponsFromServer4
            };
            Assert.AreEqual("a burning guardian", model.Name);
            Assert.AreEqual("Warrior", model.Class);
            Assert.AreEqual(3, model.Specials.Count);
            Assert.IsTrue(model.Specials.Any(a => a.Url.ToString() == "https://wiki.project1999.com/Rain_of_Molten_Lava"));
            Assert.IsTrue(model.Specials.Any(a => a.Url.ToString() == "https://wiki.project1999.com/Wave_of_Heat"));
        }

        private const string ResponsFromServer6 = @"{{Velious Era}}{{bug}}{{Namedmobpage

| imagefilename     = npc_gozzrem.png
| width             = 200px

| emu_id            = 124105
| illia_id          = 6321

| name              = Gozzrem
| race              = Lava Dragon
| class             = [[Cleric]]
| level             = 66
| agro_radius       = 70
| run_speed         = 0

| zone              = [[Temple of Veeshan]]
| location          = 15% @ (-691, -210)
| respawn_time      = 7 days +/- 8 hours

| AC                = 1057
| HP                = ~140k
| HP_regen          = 101
| mana_regen        = 101

| attacks_per_round = 3
| attack_speed      = 78%
| damage_per_hit    = 272  - 630
| special           = [[Frost Breath]], Enrage, Summon, Immune to Flee, Uncharmable,<br> Unfearable, Unsnareable, Unslowable, Unmezzable, Unstunnable, See Invis, See Hide

| description = Cold Resist. Dispells 1 slot with AE. Weak to Fire nukes and lures. Does cast CH and gates.

'''Although Gozzrem was named by the patch notes as intended to be perma-rooted, it currently is not rooted as of 6/11/2021.'''

| known_loot = 

<ul><li>  {{:Bracelet of Protection}}        <span class='drare'>(Uncommon)</span> <span class='ddb'>[1] 2x 100% (12%)</span>
</li><li> {{:Rekeklo's War Sword}}           <span class='drare'>(Uncommon)</span> <span class='ddb'>[1] 2x 100% (12%)</span>
</li><li> {{:Wand of the Black Dragon Eye}}  <span class='drare'>(Uncommon)</span> <span class='ddb'>[1] 2x 100% (12%)</span>
</li><li> {{:Cloak of Silver Eyes}}          <span class='drare'>(Uncommon)</span> <span class='ddb'>[1] 2x 100% (12%)</span>
</li><li> {{:Shovel of the Harvest}}         <span class='drare'>(Uncommon)</span> <span class='ddb'>[1] 2x 100% (12%)</span>
</li><li> {{:Unopenable Box}}                <span class='drare'>(Uncommon)</span> <span class='ddb'>[1] 2x 100% (12%)</span>
</li><li> {{:Eye of the Rigtorgn}}           <span class='drare'>(Uncommon)</span> <span class='ddb'>[1] 2x 100% (12%)</span>
</li><li> {{:Circlet of Silver Skies}}       <span class='drare'>(Uncommon)</span> <span class='ddb'>[1] 2x 100% (12%)</span>
</li></ul>

| factions = 

* [[Claws of Veeshan]] <span class='profac'>(-150)</span>
* [[Yelinak]] <span class='profac'>(-100)</span>

| opposing_factions = 

* [[Kromzek]] <span class='oppfac'>(100)</span>
* [[Drusella Sathir (Faction)|Drusella Sathir]] <span class='profac'>(-20)</span>
* [[Venril Sathir (Faction)|Venril Sathir]] <span class='profac'>(-30)</span>

| related_quests = 

* [[The First Arcane Test]]
* [[The Second Arcane Test]]
* [[Wisdom - The Long Battle]]
* [[Wisdom - The Short Battle]]
* [[Warrior Pike Quests|Warrior Pike #2 (Footman's Pike)]]
* [[Dreadscale Armor Quests|Dreadscale Boots]]
* [[Monk Shackle Quests|Shackle of Bronze]]

}}

[[Category:Temple of Veeshan]]";

        [TestMethod]
        public void ParseSpecialsGozz()
        {
            var model = new MobInfoViewModel
            {
                Results = ResponsFromServer6
            };
            Assert.AreEqual("Gozzrem", model.Name);
            Assert.AreEqual("Cleric", model.Class);
            Assert.AreEqual(12, model.Specials.Count);
            Assert.AreEqual(2, model.Factions.Count);
            Assert.IsTrue(model.Factions.Any(a => a.Name == "Claws of Veeshan (-150)"));
            Assert.IsTrue(model.Factions.Any(a => a.Name == "Yelinak (-100)"));
            Assert.AreEqual(3, model.OpposingFactions.Count);
            Assert.IsTrue(model.OpposingFactions.Any(a => a.Name == "Kromzek (100)"));
            Assert.IsTrue(model.OpposingFactions.Any(a => a.Name == "Drusella Sathir (-20)"));
            Assert.IsTrue(model.OpposingFactions.Any(a => a.Name == "Venril Sathir (-30)"));
            Assert.AreEqual(7, model.RelatedQuests.Count);
            Assert.IsTrue(model.RelatedQuests.Any(a => a.Name == "Wisdom - The Long Battle"));
            Assert.IsTrue(model.RelatedQuests.Any(a => a.Url == "https://wiki.project1999.com/Wisdom_-_The_Long_Battle"));
            Assert.IsTrue(model.RelatedQuests.Any(a => a.Name == "Warrior Pike Quests"));
            Assert.IsTrue(model.RelatedQuests.Any(a => a.Name == "Dreadscale Armor Quests"));
            Assert.IsTrue(model.RelatedQuests.Any(a => a.Name == "Monk Shackle Quests"));
        }

        [TestMethod]
        public void TestRemove1()
        {
            var now = DateTime.Now;
            var shouldremove = DPSWindowViewModel.ShouldRemove(now, null, now.AddSeconds(-5), 1);
            Assert.IsFalse(shouldremove);

            shouldremove = DPSWindowViewModel.ShouldRemove(now, null, now.AddSeconds(-40), 1);
            Assert.IsFalse(shouldremove);
            shouldremove = DPSWindowViewModel.ShouldRemove(now, null, now.AddSeconds(-41), 1);
            Assert.IsTrue(shouldremove);

            shouldremove = DPSWindowViewModel.ShouldRemove(now, now.AddSeconds(-39), now.AddSeconds(-60), 1);
            Assert.IsFalse(shouldremove);
            shouldremove = DPSWindowViewModel.ShouldRemove(now, now.AddSeconds(-41), now.AddSeconds(-60), 1);
            Assert.IsTrue(shouldremove);
        
            //shouldremove = DPSWindowViewModel.ShouldRemove(now, null, now.AddSeconds(-59), 1);
            //Assert.IsFalse(shouldremove);
            //shouldremove = DPSWindowViewModel.ShouldRemove(now, null, now.AddSeconds(-61), 1);
            //Assert.IsTrue(shouldremove);

            //shouldremove = DPSWindowViewModel.ShouldRemove(now, now.AddSeconds(-20), now.AddSeconds(-61), 1);
            //Assert.IsFalse(shouldremove);

            //shouldremove = DPSWindowViewModel.ShouldRemove(now, now.AddSeconds(-20), now.AddSeconds(-61), 8);
            //Assert.IsFalse(shouldremove);

            //shouldremove = DPSWindowViewModel.ShouldRemove(now, now.AddSeconds(-41), now.AddSeconds(-61), 2);
            //Assert.IsTrue(shouldremove);
            //shouldremove = DPSWindowViewModel.ShouldRemove(now, now.AddSeconds(-39), now.AddSeconds(-61), 2);
            //Assert.IsFalse(shouldremove);

            //shouldremove = DPSWindowViewModel.ShouldRemove(now, now.AddSeconds(-20), now.AddSeconds(-61), 3);
            //Assert.IsFalse(shouldremove);
            //shouldremove = DPSWindowViewModel.ShouldRemove(now, now.AddSeconds(-21), now.AddSeconds(-61), 3);
            //Assert.IsTrue(shouldremove);

            //shouldremove = DPSWindowViewModel.ShouldRemove(now, now.AddSeconds(-19), now.AddSeconds(-61), 8);
            //Assert.IsFalse(shouldremove);
        }
    }
}
