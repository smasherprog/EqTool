using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using EQToolShared.APIModels.BoatControllerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services
{
    public class BoatScheduleService
    {
        public BoatScheduleService()
        {

        }

        public void UpdateBoatInformation(BoatActivityResponce boat, List<BoatViewModel> boats)
        {
            var startZoneBoat = Zones.Boats.FirstOrDefault(a => a.Boat == boat.Boat && a.StartPoint == boat.StartPoint);
            var endZoneBoat = Zones.Boats.FirstOrDefault(a => a.Boat == boat.Boat && a.StartPoint == startZoneBoat.EndPoint);
            var startBoat = boats.FirstOrDefault(a => a.Boat == boat.Boat && a.Name.StartsWith(boat.StartPoint));
            var endBoat = boats.FirstOrDefault(a => a.Boat == boat.Boat && a.Name.StartsWith(startZoneBoat.EndPoint));
            var now = DateTimeOffset.Now;
            var dt = now - boat.LastSeen;
            if (boat.Boat == Boats.BarrelBarge)
            {
                var totalseconds = dt.TotalSeconds % startZoneBoat.TripTimeInSeconds;
                var timeToOasisDock = 119 - totalseconds;
                var timeToTDDock = 510 - totalseconds;
                if (timeToOasisDock > 0)
                {
                    startBoat.TotalRemainingDuration = TimeSpan.FromSeconds(timeToOasisDock);
                }
                else
                {
                    startBoat.TotalRemainingDuration = TimeSpan.FromSeconds(Math.Abs(timeToOasisDock));
                }
                if (timeToTDDock > 0)
                {
                    endBoat.TotalRemainingDuration = TimeSpan.FromSeconds(timeToTDDock);
                }
                else
                {
                    endBoat.TotalRemainingDuration = TimeSpan.FromSeconds(Math.Abs(timeToTDDock));
                }
            }
        }
    }
}
