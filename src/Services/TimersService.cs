using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EQTool.Services
{
    public class TimerInfo
    {
        public string Name { get; set; }
        public string ZoneName { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime EndTime => StartTime.Add(Duration);
        public double Fontsize { get; set; }
        private Point _Location = new Point();
        public Point Location
        {
            get
            {
                return _Location;
            }
            set
            {
                _Location = value;
            }
        }
    }

    public class TimersService
    {
        private Dictionary<string, List<TimerInfo>> ZoneTimers = new Dictionary<string, List<TimerInfo>>();
        public List<MapWidget> LoadTimersForZone(string zonename)
        {
            var ret = new List<MapWidget>();
            if (ZoneTimers.TryGetValue(zonename, out var timerInfos))
            {
                var expired = new List<TimerInfo>();
                foreach (var timerInfo in timerInfos)
                {
                    var expiredtime = timerInfo.StartTime.Add(timerInfo.Duration);
                    if (expiredtime > DateTime.Now)
                    {
                        var mw = new MapWidget(timerInfo);
                        var textlabel = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                        var forgregroundlabel = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                        mw.SetTheme(textlabel, forgregroundlabel);
                        ret.Add(mw);
                    }
                    else
                    {
                        expired.Add(timerInfo);
                    }
                }
                foreach (var item in expired)
                {
                    timerInfos.Remove(item);
                }
            }

            return ret;
        }

        public bool TimerExists(string zonename, string name)
        {
            if (ZoneTimers.TryGetValue(zonename, out var zone))
            {
                return zone.Any(a => a.Name == name);
            }
            return false;
        }

        public void RemoveTimer(TimerInfo removeTimerInfo)
        {
            if (ZoneTimers.TryGetValue(removeTimerInfo.ZoneName, out var zone))
            {
                zone.RemoveAll(a => a == removeTimerInfo);
            }
        }

        public TimerInfo RemoveTimer(string name)
        {
            foreach (var item in ZoneTimers.Values)
            {
                var t = item.FirstOrDefault(a => a.Name == name);
                if (t != null)
                {
                    item.Remove(t);
                    return t;
                }
            }
            return null;
        }

        public MapWidget AddTimer(TimerInfo addTimer)
        {
            var mw = new MapWidget(addTimer);
            var textlabel = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            var forgregroundlabel = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
            mw.SetTheme(textlabel, forgregroundlabel);
            if (ZoneTimers.TryGetValue(addTimer.ZoneName, out var zone))
            {
                zone.Add(addTimer);
            }
            else
            {
                ZoneTimers.Add(addTimer.ZoneName, new List<TimerInfo>() { addTimer });
            }

            return mw;
        }
    }
}
