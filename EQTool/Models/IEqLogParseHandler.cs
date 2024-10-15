using System;

namespace EQTool.Models
{
    public interface IEqLogParseHandler
    {
        bool Handle(string line, DateTime timestamp);
    }
}
