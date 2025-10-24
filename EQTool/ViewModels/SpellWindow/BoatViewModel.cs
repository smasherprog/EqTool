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
            this.Target = "Boat Schedules";
            this.DeleteButtonVisibility = Visibility.Collapsed;
        }

        public string LastSeen { get; set; }
        private DateTimeOffset _LastSeenDateTime { get; set; }
        public DateTimeOffset LastSeenDateTime
        {
            get { return this._LastSeenDateTime; }
            set
            {
                this._LastSeenDateTime = value;
                this.LastSeen = value.LocalDateTime.ToString("MM/dd hh:mm:ss tt");
                this.OnPropertyChanged(nameof(LastSeen));
            }
        }
        public BoatInfo Boat { get; set; }
        public override string Sorting => "ZZZ";//force to sort last
        public override SpellViewModelType SpellViewModelType => SpellViewModelType.Boat;
    }
}
