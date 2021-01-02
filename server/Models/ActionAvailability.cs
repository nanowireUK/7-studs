using System;
using System.ComponentModel.DataAnnotations;

namespace SevenStuds.Models
{
    public class ActionAvailability
    {
        // Objects of this class record the availability that is currently associated with a given action.
        // These are used in an ActionAvailabilityMap as the basis for a lookup table.
        // We are not using Hashtable because it doesn't appear to be supported by the JSON serialiser.
        public ActionEnum Action { get ; }
        public AvailabilityEnum Availability { get; set; }
        public ActionAvailability(ActionEnum ac, AvailabilityEnum av)
        {
            Action = ac;
            Availability = av;
        }
    }
}