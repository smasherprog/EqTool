using System;
using System.Collections.Generic;
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
        public double Fontsize { get; set; }
        public Point Location { get; set; }
    }

    public class RemoveTimerInfo
    {
        public string Name { get; set; }
        public string ZoneName { get; set; }
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
                        var mw = new MapWidget(timerInfo.StartTime.Add(timerInfo.Duration), timerInfo.Fontsize, timerInfo.Name);
                        var textlabel = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                        var forgregroundlabel = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                        mw.SetTheme(textlabel, forgregroundlabel);
                        ret.Add(mw);
                        mw.Tag = timerInfo;
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

        public void RemoveTimer(RemoveTimerInfo removeTimerInfo)
        {
            if (ZoneTimers.TryGetValue(removeTimerInfo.ZoneName, out var zone))
            {
                zone.RemoveAll(a => a.Name == removeTimerInfo.Name);
            }
        }

        public MapWidget AddTimer(TimerInfo addTimer)
        {
            var mw = new MapWidget(addTimer.StartTime.Add(addTimer.Duration), addTimer.Fontsize, addTimer.Name);
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
