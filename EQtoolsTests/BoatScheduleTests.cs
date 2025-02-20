using Autofac;
using EQTool.Services;
using EQTool.Services.Handlers;
using EQTool.Services.Parsing;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using EQToolShared.APIModels.BoatControllerModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;

namespace EQtoolsTests
{

    [TestClass]
    public class BoatScheduleTests : BaseTestClass
    {
        private readonly BoatScheduleService boatScheduleService;
        private readonly List<BoatViewModel> boatViewModelList = new List<BoatViewModel>();

        public BoatScheduleTests()
        {
            boatScheduleService = container.Resolve<BoatScheduleService>();
            foreach (var item in Zones.Boats)
            {
                boatViewModelList.Add(new BoatViewModel
                {
                    Name = $"{item.StartPoint} -> {item.EndPoint}",
                    Boat = item.Boat
                });
            }
        }

        [TestMethod]
        public void HappyTimeNotPassed()
        {
            var d = DateTimeOffset.Now;
            var boat = new BoatActivityResponce
            {
                Boat = EQToolShared.Boats.BarrelBarge,
                LastSeen = d.AddSeconds(-10),
                StartPoint = "oasis"
            };
            this.boatScheduleService.UpdateBoatInformation(boat, boatViewModelList);
            var startZoneBoat = Zones.Boats.FirstOrDefault(a => a.Boat == boat.Boat && a.StartPoint == boat.StartPoint);
            var endZoneBoat = Zones.Boats.FirstOrDefault(a => a.Boat == boat.Boat && a.StartPoint == startZoneBoat.EndPoint);
            var startBoat = boatViewModelList.FirstOrDefault(a => a.Boat == boat.Boat && a.Name.StartsWith(boat.StartPoint));
            var endBoat = boatViewModelList.FirstOrDefault(a => a.Boat == boat.Boat && a.Name.StartsWith(startZoneBoat.EndPoint));
            Assert.AreEqual((int)startBoat.TotalRemainingDuration.TotalSeconds, 109, 1);
            Assert.AreEqual((int)endBoat.TotalRemainingDuration.TotalSeconds, 500, 1);
        }

        [TestMethod]
        public void HappyTimePassedOasis()
        {
            var d = DateTimeOffset.Now;
            var boat = new BoatActivityResponce
            {
                Boat = EQToolShared.Boats.BarrelBarge,
                LastSeen = d.AddSeconds(-200),
                StartPoint = "oasis"
            };
            this.boatScheduleService.UpdateBoatInformation(boat, boatViewModelList);
            var startZoneBoat = Zones.Boats.FirstOrDefault(a => a.Boat == boat.Boat && a.StartPoint == boat.StartPoint);
            var endZoneBoat = Zones.Boats.FirstOrDefault(a => a.Boat == boat.Boat && a.StartPoint == startZoneBoat.EndPoint);
            var startBoat = boatViewModelList.FirstOrDefault(a => a.Boat == boat.Boat && a.Name.StartsWith(boat.StartPoint));
            var endBoat = boatViewModelList.FirstOrDefault(a => a.Boat == boat.Boat && a.Name.StartsWith(startZoneBoat.EndPoint));
            Assert.AreEqual((int)startBoat.TotalRemainingDuration.TotalSeconds, 698, 1);
            Assert.AreEqual((int)endBoat.TotalRemainingDuration.TotalSeconds, 310, 1);
        }
    }
}
