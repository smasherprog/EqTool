using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using EQTool.Models;

namespace EQTool.Services.Handlers
{
    //
    // create a class just to hold the (x, y, z) and timestamp info for each /loc
    // the main point of using this data structure instead of the nearly-identical one in PlayerLocationEvent,
    // is the Timestamp values in PlayerLocationEvent are from the log, and therefore only accurate to 1 second,
    // where this structure can use a Datetime.Now timestamp for a bit of increased accuracy
    //
    public class LocDatum
    {
        private Point3D Location;
        private readonly DateTime TimeStamp;

        // ctor initialize parameters
        public LocDatum(Point3D location, DateTime timeStamp)
        {
            Location = location;
            TimeStamp = timeStamp;
        }

        // calculate runSpeed between two datums
        // normalized to return 100% for an unencumbered player running on flat terrain
        public double RunSpeed(LocDatum prev)
        {
            // this value was determined empirically
            const double RunningLocsPerSecond = 29.8;

            double deltaX = Location.X - prev.Location.X;
            double deltaY = Location.Y - prev.Location.Y;
            double deltaZ = Location.Z - prev.Location.Z;
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

            TimeSpan elapsed = TimeStamp - prev.TimeStamp;
            double speed = 0.0;
            if (elapsed.TotalSeconds > 0)
            {
                speed = distance / elapsed.TotalSeconds / RunningLocsPerSecond * 100.0;
            }

            return speed;
        }
    }

    public class PlayerLocationHandler : BaseHandler
    {
        private LocDatum prevLocation = new LocDatum(new Point3D(0, 0, 0), DateTime.Now);

        public PlayerLocationHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.PlayerLocationEvent += LogEvents_PlayerLocationEvent;
        }

        private void LogEvents_PlayerLocationEvent(object sender, PlayerLocationEvent e)
        {
            // calculate the player runSpeed using last two loc data
            LocDatum currentLocation = new LocDatum(e.Location, DateTime.Now);
            double runSpeed = currentLocation.RunSpeed(prevLocation);
            prevLocation = currentLocation;

            // save to ActivePlayer
            activePlayer.RunSpeed = runSpeed;

            // display the runSpeed?
            if (activePlayer?.Player?.RunSpeedOverlay == true)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    logEvents.Handle(new OverlayEvent { Text = $"Runspeed: {runSpeed:F0}%", ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(5000);
                    logEvents.Handle(new OverlayEvent { Text = $"Runspeed: {runSpeed:F0}%", ForeGround = Brushes.Red, Reset = true });
                });
            }

            // other loc behavior
            appDispatcher.DispatchUI(() =>
            {
                activePlayer.Location = e.Location;
            });
        }
    }
}
