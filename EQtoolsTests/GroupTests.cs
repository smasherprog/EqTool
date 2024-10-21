using Autofac;
using EQTool.Services;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class GroupTests : BaseTestClass
    { 
        public GroupTests()
        { 
        }

        [TestMethod]
        public void GroupTest_HOT_ClericSameGroup_4_groups()
        {
            var service = container.Resolve<PlayerGroupService>();
            var groups = service.CreateHOT_Clerics_SameGroups(new System.Collections.Generic.List<EQToolShared.APIModels.PlayerControllerModels.Player>
            {
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Rogue },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Ranger },

                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.ShadowKnight },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Druid },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Magician },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Druid },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Shaman, Level=60 },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Ranger }
            });

            Assert.AreEqual(4, groups.Count);

            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Magician));

            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Shaman));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Rogue));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));

            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Druid));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));

            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Druid));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Ranger));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Ranger));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
        }

        [TestMethod]
        public void GroupTest_Standard_Balanced_2_groups()
        {
            var service = container.Resolve<PlayerGroupService>();
            var groups = service.CreateStandardGroups(new System.Collections.Generic.List<EQToolShared.APIModels.PlayerControllerModels.Player>
            {
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Rogue },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Ranger }
            });

            Assert.AreEqual(2, groups.Count);
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Rogue));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Ranger));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
        }

        [TestMethod]
        public void GroupTest_Standard_Balanced_4_groups()
        {
            var service = container.Resolve<PlayerGroupService>();
            var groups = service.CreateStandardGroups(new System.Collections.Generic.List<EQToolShared.APIModels.PlayerControllerModels.Player>
            {
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Rogue },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Ranger },

                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Rogue },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Ranger }
            });

            Assert.AreEqual(4, groups.Count);

            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Rogue));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Rogue));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));

            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Ranger));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Ranger));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
        }

        [TestMethod]
        public void GroupTest_Standard_OutOfBalanced_4_groups()
        {
            var service = container.Resolve<PlayerGroupService>();
            var groups = service.CreateStandardGroups(new System.Collections.Generic.List<EQToolShared.APIModels.PlayerControllerModels.Player>
            {
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Cleric },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Bard },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Rogue },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Ranger },

                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Druid },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Druid },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Enchanter },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Paladin },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Warrior },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Necromancer },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Rogue },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Rogue },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Rogue },
                    new EQToolShared.APIModels.PlayerControllerModels.Player { PlayerClass = PlayerClasses.Ranger }
            });

            Assert.AreEqual(4, groups.Count);

            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Rogue));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));

            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Rogue));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));

            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Druid));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Rogue));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Ranger));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Ranger));
            Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));

            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Druid));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Rogue));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Paladin));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
            Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
        }
    }
}
