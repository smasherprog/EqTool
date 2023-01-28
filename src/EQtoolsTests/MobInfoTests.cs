using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
