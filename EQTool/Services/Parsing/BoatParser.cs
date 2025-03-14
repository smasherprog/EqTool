﻿using EQTool.Models;
using EQToolShared;
using System;

namespace EQTool.Services.Parsing
{
    public class BoatParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        public BoatParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            foreach (var item in Zones.Boats)
            {
                if (!string.IsNullOrWhiteSpace(item.StartAnnoucement) && line.StartsWith(item.StartAnnoucement))
                {
                    this.logEvents.Handle(new BoatEvent
                    {
                        Line = line,
                        LineCounter = lineCounter,
                        TimeStamp = timestamp,
                        StartPoint = item.StartPoint,
                        Boat = item.Boat
                    });
                    return true;
                }
            }
            return false;
        }

    }
}
