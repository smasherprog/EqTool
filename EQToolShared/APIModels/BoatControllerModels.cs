using EQToolShared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace EQToolShared.APIModels.BoatControllerModels
{
    public class BoatActivityRequest
    {
        [Required]
        public string StartPoint { get; set; } 
        [EnumDataType(typeof(EQToolShared.Boats))]
        public Boats Boat { get; set; }
        [EnumDataType(typeof(Servers))]
        public Servers Server { get; set; }
    } 
    public class BoatActivityResponce
    {
        [Required]
        public string StartPoint { get; set; }
        [EnumDataType(typeof(EQToolShared.Boats))]
        public Boats Boat { get; set; }
        [Required]
        public DateTimeOffset LastSeen { get; set; }
    }
}
