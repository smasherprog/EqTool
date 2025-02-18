using Autofac;
using EQTool.Models;
using EQTool.ViewModels.MobInfoComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class PetTests : BaseTestClass
    {
        private readonly PetViewModel playerPet;
        private readonly Pets pets;
        // ctor
        public PetTests()
        {
            playerPet = container.Resolve<PetViewModel>();
            pets = container.Resolve<Pets>();
        }

        [TestMethod]
        public void TestLoad()
        {
            var petSpell = pets.PetSpellDictionary["Emissary of Thule"];
            playerPet.PetSpell = petSpell;
            playerPet.PetName = "Bakalakadaka";

            // 6x pet ranks: 5x min to max, 1x max+focus
            Assert.AreEqual(6, petSpell.PetRankList.Count);

            // 2x bone chips, 1x peridot
            Assert.AreEqual(2, petSpell.PetReagents.Count);

            var reagent0 = petSpell.PetReagents[0];
            Assert.AreEqual(PetReagent.BoneChip, reagent0.Item1);
            Assert.AreEqual(2, reagent0.Item2);

            var reagent1 = petSpell.PetReagents[1];
            Assert.AreEqual(PetReagent.Peridot, reagent1.Item1);
            Assert.AreEqual(1, reagent1.Item2);

        }

        [TestMethod]
        public void TestFindRank()
        {
            var petSpell = pets.PetSpellDictionary["Emissary of Thule"];
            playerPet.PetSpell = petSpell;
            playerPet.PetName = "Bakalakadaka";

            // confirm we can find the right rank
            // for Emissary, max damage of the 4th rank (index 3) = 59
            playerPet.CheckMaxMelee(59);
            Assert.AreEqual(3, playerPet.RankIndex);
        }



    }
}
