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
            var totalseconds = 0;
            if (dt.TotalSeconds > startZoneBoat.TripTimeInSeconds)
            {
                totalseconds = (int)(dt.TotalSeconds / startZoneBoat.TripTimeInSeconds);
                totalseconds = (int)((totalseconds * startZoneBoat.TripTimeInSeconds) - dt.TotalSeconds);
            }
            else
            {
                totalseconds = (int)(dt.TotalSeconds);
            }
            var workingBoats = new List<Boats>() { Boats.BarrelBarge, Boats.NroIcecladBoat };
            if (workingBoats.Contains(boat.Boat))
            {
                var timeToStartDock = startZoneBoat.AnnouncementToDockInSeconds - totalseconds;
                var timeToEndDock = endZoneBoat.AnnouncementToDockInSeconds - totalseconds;
                if (timeToStartDock > 0)
                {
                    startBoat.TotalRemainingDuration = TimeSpan.FromSeconds(timeToStartDock);
                }
                else
                {
                    timeToStartDock = startZoneBoat.TripTimeInSeconds - totalseconds + startZoneBoat.AnnouncementToDockInSeconds;
                    startBoat.TotalRemainingDuration = TimeSpan.FromSeconds(timeToStartDock);
                }
                if (timeToEndDock > 0)
                {
                    endBoat.TotalRemainingDuration = TimeSpan.FromSeconds(timeToEndDock);
                }
                else
                {
                    timeToEndDock = endZoneBoat.TripTimeInSeconds - totalseconds + endZoneBoat.AnnouncementToDockInSeconds;
                    endBoat.TotalRemainingDuration = TimeSpan.FromSeconds(timeToEndDock);
                }
            } 
        }
    }
}
