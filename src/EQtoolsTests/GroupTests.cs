using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Spells.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EQToolTests
{
    [TestClass]
    public class GroupTests
    {
        private readonly IContainer container;
        public GroupTests()
        {
            container = DI.Init();
        }

        [TestMethod]
        public void GroupTest_Standard_Balanced_2_groups()
        {
            var service = container.Resolve<PlayerGroupService>();
            var groups = service.CreateStandardGroups(new System.Collections.Generic.List<PlayerWhoLogParse.PlayerInfo>
            {
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Cleric },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Cleric },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Enchanter },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Enchanter },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Warrior },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Warrior },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Necromancer },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Necromancer },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Bard },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Bard },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Rogue },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Ranger }
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
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Ranger));
            Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
            Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
        }

        [TestMethod]
        public void GroupTest_Standard_Balanced_4_groups()
        {
            var service = container.Resolve<PlayerGroupService>();
            var groups = service.CreateStandardGroups(new System.Collections.Generic.List<PlayerWhoLogParse.PlayerInfo>
            {
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Cleric },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Cleric },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Enchanter },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Enchanter },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Warrior },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Warrior },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Necromancer },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Necromancer },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Bard },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Bard },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Rogue },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Ranger },

                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Cleric },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Cleric },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Enchanter },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Enchanter },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Warrior },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Warrior },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Necromancer },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Necromancer },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Bard },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Bard },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Rogue },
                    new PlayerWhoLogParse.PlayerInfo { PlayerClass = PlayerClasses.Ranger }
            });

            Assert.AreEqual(4, groups.Count);

            //Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            //Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            //Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            //Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            //Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            //Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            //Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            //Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            //Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Rogue)); 
            //Assert.IsTrue(groups[1].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
            //Assert.IsTrue(groups[0].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));

            //Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            //Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Cleric));
            //Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            //Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Enchanter));
            //Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            //Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Warrior));
            //Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            //Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Bard));
            //Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Rogue));
            //Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Ranger));
            //Assert.IsTrue(groups[2].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
            //Assert.IsTrue(groups[3].Players.Any(a => a.PlayerClass == PlayerClasses.Necromancer));
        }
    }
}
