using System;
using System.ComponentModel.DataAnnotations;

namespace SevenStuds.Models
{
    public class ActionAvailability
    {
        public ActionEnum Action { get ; }
        public AvailabilityEnum Availability { get; set; }

        public ActionAvailability(ActionEnum ac, AvailabilityEnum av)
        {
            Action = ac;
            Availability = av;
        }


    }
}