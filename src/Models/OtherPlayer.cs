using EQToolShared.HubModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static EQTool.Services.MapLoad;

namespace EQTool.Models
{
    public class OtherPlayer
    {
        public PlayerLocation Location { get; set; }
        public List<FrameworkElement> MapElements { get; set; }

        public OtherPlayer()
        {
            MapElements = new List<FrameworkElement>();
        }
    }
}
