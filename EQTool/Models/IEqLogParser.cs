using System;

namespace EQTool.Models
{
    public interface IEqLogParser
    {
        bool Handle(string line, DateTime timestamp, int lineCounter);
    }
}
