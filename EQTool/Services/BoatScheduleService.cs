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
        public static readonly List<Boats> SupportdBoats = new List<Boats>() { Boats.BarrelBarge, Boats.NroIcecladBoat, Boats.BloatedBelly };
        public BoatScheduleService()
        {

        }

        public void UpdateBoatInformation(BoatActivityResponce boat, List<BoatViewModel> boats)
        {
            var startZoneBoat = Zones.Boats.FirstOrDefault(a => a.Boat == boat.Boat && a.StartPoint == boat.StartPoint);
            var endZoneBoat = Zones.Boats.FirstOrDefault(a => a.Boat == boat.Boat && a.StartPoint == startZoneBoat.EndPoint);
            var startBoat = boats.FirstOrDefault(a => a.Boat == startZoneBoat);
            var endBoat = boats.FirstOrDefault(a => a.Boat.Boat == boat.Boat && startBoat.Boat.EndPoint == a.Boat.StartPoint);
            var now = DateTimeOffset.Now;
            var dt = now - boat.LastSeen;
            var totalseconds = 0;
            var dtseconds = totalseconds = (int)Math.Abs(dt.TotalSeconds);
            if (dtseconds > startZoneBoat.TripTimeInSeconds)
            {
                totalseconds = (int)(dtseconds / startZoneBoat.TripTimeInSeconds);
                totalseconds = (int)(dtseconds- (totalseconds * startZoneBoat.TripTimeInSeconds));
            } 
          
            if (SupportdBoats.Contains(boat.Boat))
            {
                var timeToStartDock = startZoneBoat.AnnouncementToDockInSeconds - totalseconds;
                startBoat.LastSeenDateTime = boat.LastSeen;
                if (timeToStartDock > 0)
                {
                    startBoat.TotalRemainingDuration = TimeSpan.FromSeconds(timeToStartDock);
                }
                else
                {
                    timeToStartDock = (int)(startZoneBoat.TripTimeInSeconds - totalseconds + startZoneBoat.AnnouncementToDockInSeconds);
                    startBoat.TotalRemainingDuration = TimeSpan.FromSeconds(timeToStartDock);
                }
                if(endBoat!= null)
                {
                    endBoat.LastSeenDateTime = boat.LastSeen;
                    var timeToEndDock = endZoneBoat.AnnouncementToDockInSeconds - totalseconds;
                    if (timeToEndDock > 0)
                    {
                        endBoat.TotalRemainingDuration = TimeSpan.FromSeconds(timeToEndDock);
                    }
                    else
                    {
                        timeToEndDock = (int)(endZoneBoat.TripTimeInSeconds - totalseconds + endZoneBoat.AnnouncementToDockInSeconds);
                        endBoat.TotalRemainingDuration = TimeSpan.FromSeconds(timeToEndDock);
                    }
                }
            } 
        }
    }
}
