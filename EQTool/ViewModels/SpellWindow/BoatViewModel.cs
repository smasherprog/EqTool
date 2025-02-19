using EQTool.Models;
using EQToolShared;
using EQToolShared.Enums;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace EQTool.ViewModels.SpellWindow
{
    public class BoatViewModel : TimerViewModel
    {
        public BoatViewModel()
        {
            this.GroupName = "Boat Schedules";
        }
        public Boats Boat { get; set; }
        public override string Sorting => "ZZZ";//force to sort last
        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Boat;
    }
}
